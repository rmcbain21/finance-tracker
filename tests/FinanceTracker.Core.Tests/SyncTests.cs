using System.Text.Json.Nodes;
using FinanceTracker.Core.Ports;
using FinanceTracker.Core.Preferences;
using FinanceTracker.Core.Receipts;
using FinanceTracker.Core.Sync;
using FinanceTracker.Core.Tests.Fakes;

namespace FinanceTracker.Core.Tests;

/// <summary>Syncing two devices through one shared sync file.</summary>
public class SyncTests
{
    private static readonly DateOnly Today = new(2026, 11, 9);

    private readonly FakeClock _clock = new(new DateTimeOffset(2026, 11, 9, 18, 0, 0, TimeSpan.Zero));
    private readonly InMemoryCloudFileStore _drive = new();
    private readonly AppCore _phone;
    private readonly AppCore _desktop;

    public SyncTests()
    {
        // Two devices with their own data and one Drive, sharing a clock so tests control which edit is newer.
        _phone = new AppCore(new InMemoryRecordStore(), _clock, _drive);
        _desktop = new AppCore(new InMemoryRecordStore(), _clock, _drive);
    }

    private void Later() => _clock.UtcNow = _clock.UtcNow.AddMinutes(5);

    private static Task<Receipt> AddReceiptAsync(AppCore app, decimal total) => app.Receipts.AddReceiptAsync(Today, total);

    private static Task UpdateTotalAsync(AppCore app, Receipt receipt, decimal total) =>
        app.Receipts.UpdateReceiptAsync(receipt.Id, receipt.Date, total);

    private static async Task<decimal[]> TotalsAsync(AppCore app) =>
        (await app.Receipts.ListReceiptsAsync()).Select(receipt => receipt.Total.Amount).Order().ToArray();

    private static async Task<decimal?> TotalAsync(AppCore app, Receipt receipt) =>
        (await app.Receipts.GetReceiptAsync(receipt.Id))?.Total.Amount;

    private async Task ConnectBothAsync()
    {
        await _phone.Sync.ConnectAsync();
        await _desktop.Sync.ConnectAsync();
    }

    [Fact]
    public async Task A_receipt_added_on_one_device_shows_up_on_the_other()
    {
        await ConnectBothAsync();
        await AddReceiptAsync(_phone, 84.37m);

        await _phone.Sync.SyncNowAsync();
        await _desktop.Sync.SyncNowAsync();

        Assert.Equal([84.37m], await TotalsAsync(_desktop));
    }

    [Fact]
    public async Task Each_device_ends_up_with_both_devices_additions()
    {
        await ConnectBothAsync();
        await AddReceiptAsync(_phone, 1m);
        await AddReceiptAsync(_desktop, 2m);

        await _phone.Sync.SyncNowAsync();
        await _desktop.Sync.SyncNowAsync();
        await _phone.Sync.SyncNowAsync();

        Assert.Equal([1m, 2m], await TotalsAsync(_phone));
        Assert.Equal([1m, 2m], await TotalsAsync(_desktop));
    }

    [Fact]
    public async Task Edits_travel_in_both_directions()
    {
        await ConnectBothAsync();
        var receipt = await AddReceiptAsync(_phone, 10m);
        await _phone.Sync.SyncNowAsync();
        await _desktop.Sync.SyncNowAsync();

        Later();
        await UpdateTotalAsync(_desktop, receipt, 11m);
        await _desktop.Sync.SyncNowAsync();
        await _phone.Sync.SyncNowAsync();
        Assert.Equal(11m, await TotalAsync(_phone, receipt));

        Later();
        await UpdateTotalAsync(_phone, receipt, 12m);
        await _phone.Sync.SyncNowAsync();
        await _desktop.Sync.SyncNowAsync();
        Assert.Equal(12m, await TotalAsync(_desktop, receipt));
    }

    [Fact]
    public async Task A_deletion_on_one_device_deletes_the_receipt_on_the_other()
    {
        await ConnectBothAsync();
        var receipt = await AddReceiptAsync(_phone, 10m);
        await _phone.Sync.SyncNowAsync();
        await _desktop.Sync.SyncNowAsync();

        Later();
        await _desktop.Receipts.DeleteReceiptAsync(receipt.Id);
        await _desktop.Sync.SyncNowAsync();
        await _phone.Sync.SyncNowAsync();

        Assert.Null(await _phone.Receipts.GetReceiptAsync(receipt.Id));
    }

    [Fact]
    public async Task A_deletion_syncs_as_a_tombstone_in_the_file()
    {
        await _phone.Sync.ConnectAsync();
        var receipt = await AddReceiptAsync(_phone, 10m);
        await _phone.Receipts.DeleteReceiptAsync(receipt.Id);

        await _phone.Sync.SyncNowAsync();

        var receipts = JsonNode.Parse(_drive.Text!)!["records"]!["receipts"]!.AsArray();
        var tombstone = Assert.Single(receipts)!;
        Assert.Equal((receipt.Id, true), ((string)tombstone["id"]!, (bool)tombstone["deleted"]!));
    }

    [Fact]
    public async Task A_deletion_stays_deleted_after_the_other_device_syncs_again()
    {
        await ConnectBothAsync();
        var receipt = await AddReceiptAsync(_phone, 10m);
        await _phone.Sync.SyncNowAsync();
        await _desktop.Sync.SyncNowAsync();
        Later();
        await _phone.Receipts.DeleteReceiptAsync(receipt.Id);
        await _phone.Sync.SyncNowAsync();

        // The desktop still has the old copy, unchanged since the last sync: it mustn't bring it back.
        await _desktop.Sync.SyncNowAsync();
        await _phone.Sync.SyncNowAsync();

        Assert.Null(await _phone.Receipts.GetReceiptAsync(receipt.Id));
        Assert.Null(await _desktop.Receipts.GetReceiptAsync(receipt.Id));
    }

    [Fact]
    public async Task A_change_only_this_device_made_wins_even_if_its_clock_was_behind()
    {
        await ConnectBothAsync();
        var receipt = await AddReceiptAsync(_phone, 10m);
        await _phone.Sync.SyncNowAsync();
        await _desktop.Sync.SyncNowAsync();

        // The desktop's clock is wrong: its edit is stamped earlier than the version it last synced.
        _clock.UtcNow = _clock.UtcNow.AddHours(-1);
        await UpdateTotalAsync(_desktop, receipt, 11m);
        await _desktop.Sync.SyncNowAsync();
        await _phone.Sync.SyncNowAsync();

        Assert.Equal(11m, await TotalAsync(_desktop, receipt));
        Assert.Equal(11m, await TotalAsync(_phone, receipt));
    }

    [Fact]
    public async Task Settings_sync_too()
    {
        await ConnectBothAsync();
        await _phone.Settings.SetAppearanceAsync(Appearance.Dark);
        await _phone.Settings.SetHomeCurrencyAsync(Money.Cad);

        await _phone.Sync.SyncNowAsync();
        await _desktop.Sync.SyncNowAsync();

        var settings = await _desktop.Settings.GetAsync();
        Assert.Equal((Appearance.Dark, Money.Cad), (settings.Appearance, settings.HomeCurrency));
    }

    [Fact]
    public async Task The_first_sync_with_no_file_yet_writes_this_devices_data_to_the_file()
    {
        await _phone.Sync.ConnectAsync();
        var receipt = await AddReceiptAsync(_phone, 10m);

        await _phone.Sync.SyncNowAsync();

        var receipts = JsonNode.Parse(_drive.Text!)!["records"]!["receipts"]!.AsArray();
        Assert.Equal(receipt.Id, (string)Assert.Single(receipts)!["id"]!);
    }

    [Fact]
    public async Task The_sync_file_holds_no_sync_bookkeeping_and_neither_does_an_export()
    {
        await _phone.Sync.ConnectAsync();
        await AddReceiptAsync(_phone, 10m);

        await _phone.Sync.SyncNowAsync();

        Assert.DoesNotContain("syncState", _drive.Text);
        Assert.DoesNotContain("syncState", await _phone.Backup.ExportAsync());
    }

    [Fact]
    public async Task The_summary_says_what_arrived_from_other_devices()
    {
        await ConnectBothAsync();
        var receipt = await AddReceiptAsync(_phone, 10m);
        await _phone.Sync.SyncNowAsync();
        await _desktop.Sync.SyncNowAsync();
        Later();
        await UpdateTotalAsync(_phone, receipt, 11m);
        await AddReceiptAsync(_phone, 20m);
        await _phone.Sync.SyncNowAsync();

        var summary = await _desktop.Sync.SyncNowAsync();

        Assert.Equal([new RecordTypeChanges("Receipts", Added: 1, Updated: 1, Deleted: 0)], summary.Changes);
        Assert.True((await _desktop.Sync.SyncNowAsync()).NothingChanged);
    }

    [Fact]
    public async Task A_sync_records_when_it_happened()
    {
        await _phone.Sync.ConnectAsync();
        Assert.Null((await _phone.Sync.GetStatusAsync()).LastSyncedAt);
        Later();

        await _phone.Sync.SyncNowAsync();

        Assert.Equal(_clock.UtcNow, (await _phone.Sync.GetStatusAsync()).LastSyncedAt);
    }

    [Fact]
    public async Task A_failed_write_is_retried_by_the_next_sync()
    {
        await ConnectBothAsync();
        await _phone.Sync.SyncNowAsync();
        var lastSynced = (await _phone.Sync.GetStatusAsync()).LastSyncedAt;
        Later();
        await AddReceiptAsync(_phone, 10m);
        _drive.FailWrites = true;

        await Assert.ThrowsAsync<CloudFileStoreException>(() => _phone.Sync.SyncNowAsync());

        Assert.Equal(lastSynced, (await _phone.Sync.GetStatusAsync()).LastSyncedAt);
        _drive.FailWrites = false;
        await _phone.Sync.SyncNowAsync();
        await _desktop.Sync.SyncNowAsync();
        Assert.Equal([10m], await TotalsAsync(_desktop));
    }

    [Fact]
    public async Task A_device_that_syncs_in_the_middle_of_another_devices_sync_keeps_its_changes()
    {
        await ConnectBothAsync();
        await AddReceiptAsync(_phone, 1m);
        await AddReceiptAsync(_desktop, 2m);
        _drive.AfterNextRead = () => _desktop.Sync.SyncNowAsync();

        await _phone.Sync.SyncNowAsync();
        await _desktop.Sync.SyncNowAsync();

        Assert.Equal([1m, 2m], await TotalsAsync(_phone));
        Assert.Equal([1m, 2m], await TotalsAsync(_desktop));
    }

    [Fact]
    public async Task Syncing_needs_google_drive_connected_first()
    {
        var refusal = await Assert.ThrowsAsync<RuleViolationException>(() => _phone.Sync.SyncNowAsync());

        Assert.Contains("Connect", refusal.Message);
    }

    [Fact]
    public async Task Disconnecting_keeps_the_data_on_the_device()
    {
        await _phone.Sync.ConnectAsync();
        await AddReceiptAsync(_phone, 10m);
        await _phone.Sync.SyncNowAsync();

        await _phone.Sync.DisconnectAsync();

        Assert.False((await _phone.Sync.GetStatusAsync()).Connected);
        Assert.Equal([10m], await TotalsAsync(_phone));
        await Assert.ThrowsAsync<RuleViolationException>(() => _phone.Sync.SyncNowAsync());
    }

    [Fact]
    public async Task Being_connected_is_remembered_after_the_app_is_reopened()
    {
        var store = new InMemoryRecordStore();
        await new AppCore(store, _clock, _drive).Sync.ConnectAsync();

        Assert.True((await new AppCore(store, _clock, _drive).Sync.GetStatusAsync()).Connected);
    }

    [Fact]
    public async Task A_sync_file_in_an_unknown_format_is_refused_and_changes_nothing()
    {
        await _phone.Sync.ConnectAsync();
        _drive.Text = """{"formatVersion": 99, "records": {}}""";
        await AddReceiptAsync(_phone, 10m);

        var refusal = await Assert.ThrowsAsync<RuleViolationException>(() => _phone.Sync.SyncNowAsync());

        Assert.Contains("newer version", refusal.Message);
        Assert.Contains("99", _drive.Text);
        Assert.Null((await _phone.Sync.GetStatusAsync()).LastSyncedAt);
    }
}
