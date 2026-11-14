using FinanceTracker.Core.Preferences;
using FinanceTracker.Core.Receipts;
using FinanceTracker.Core.Sync;
using FinanceTracker.Core.Tests.Fakes;

namespace FinanceTracker.Core.Tests;

/// <summary>A record changed on two devices since they last synced: kept on both until the Owner chooses.</summary>
public class SyncConflictTests
{
    private static readonly DateOnly Today = new(2026, 11, 9);

    private readonly FakeClock _clock = new(new DateTimeOffset(2026, 11, 9, 18, 0, 0, TimeSpan.Zero));
    private readonly InMemoryCloudFileStore _drive = new();
    private readonly InMemoryRecordStore _desktopStore = new();
    private readonly AppCore _phone;
    private readonly AppCore _desktop;

    public SyncConflictTests()
    {
        _phone = new AppCore(new InMemoryRecordStore(), _clock, _drive);
        _desktop = new AppCore(_desktopStore, _clock, _drive);
    }

    private void Later() => _clock.UtcNow = _clock.UtcNow.AddMinutes(5);

    private static Task UpdateTotalAsync(AppCore app, Receipt receipt, decimal total) =>
        app.Receipts.UpdateReceiptAsync(receipt.Id, receipt.Date, total);

    private static async Task<decimal?> TotalAsync(AppCore app, Receipt receipt) =>
        (await app.Receipts.GetReceiptAsync(receipt.Id))?.Total.Amount;

    private static async Task<int> PendingAsync(AppCore app) => (await app.Sync.GetStatusAsync()).PendingConflicts;

    private static async Task SyncAllAsync(params AppCore[] devices)
    {
        foreach (var device in devices)
        {
            await device.Sync.SyncNowAsync();
        }
    }

    private async Task ConnectBothAsync()
    {
        await _phone.Sync.ConnectAsync();
        await _desktop.Sync.ConnectAsync();
    }

    /// <summary>A Receipt known to both devices, synced, then corrected differently on each: $9.50 on the phone, $9.00 on the desktop.</summary>
    private async Task<Receipt> ConflictingReceiptAsync()
    {
        await ConnectBothAsync();
        var receipt = await _phone.Receipts.AddReceiptAsync(Today, 10m);
        await SyncAllAsync(_phone, _desktop);
        Later();
        await UpdateTotalAsync(_phone, receipt, 9.50m);
        Later();
        await UpdateTotalAsync(_desktop, receipt, 9m);
        await SyncAllAsync(_phone, _desktop);
        return receipt;
    }

    [Fact]
    public async Task Different_edits_on_two_devices_become_a_sync_conflict_and_neither_is_overwritten()
    {
        var receipt = await ConflictingReceiptAsync();

        await _phone.Sync.SyncNowAsync();

        Assert.Equal(1, await PendingAsync(_desktop));
        Assert.Equal(9m, await TotalAsync(_desktop, receipt));
        Assert.Equal(9.50m, await TotalAsync(_phone, receipt));
    }

    [Fact]
    public async Task The_same_edit_on_two_devices_is_not_a_conflict()
    {
        await ConnectBothAsync();
        var receipt = await _phone.Receipts.AddReceiptAsync(Today, 10m);
        await SyncAllAsync(_phone, _desktop);
        Later();
        await UpdateTotalAsync(_phone, receipt, 9.50m);
        Later();
        await UpdateTotalAsync(_desktop, receipt, 9.50m);

        await SyncAllAsync(_phone, _desktop, _phone);

        Assert.Equal(0, await PendingAsync(_desktop));
        Assert.Equal(0, await PendingAsync(_phone));
    }

    [Fact]
    public async Task A_conflict_is_still_waiting_after_the_app_is_reopened()
    {
        await ConflictingReceiptAsync();

        var reopened = new AppCore(_desktopStore, _clock, _drive);

        Assert.Equal(1, await PendingAsync(reopened));
        Assert.Single(await reopened.Sync.ListConflictsAsync());
    }

    [Fact]
    public async Task Syncing_again_before_settling_keeps_one_conflict()
    {
        await ConflictingReceiptAsync();

        await SyncAllAsync(_desktop, _phone, _desktop);

        Assert.Equal(1, await PendingAsync(_desktop));
    }

    [Fact]
    public async Task Other_changes_keep_syncing_while_a_conflict_waits()
    {
        await ConflictingReceiptAsync();
        await _phone.Receipts.AddReceiptAsync(Today, 42m);

        await SyncAllAsync(_phone, _desktop);

        Assert.Contains(42m, (await _desktop.Receipts.ListReceiptsAsync()).Select(receipt => receipt.Total.Amount));
        Assert.Equal(1, await PendingAsync(_desktop));
    }

    [Fact]
    public async Task A_receipt_conflict_shows_both_versions_side_by_side_with_the_difference_marked()
    {
        await ConflictingReceiptAsync();

        var conflict = Assert.Single(await _desktop.Sync.ListConflictsAsync());

        Assert.Equal(("Receipt", "Mon, Nov 9, 2026, $9.00"), (conflict.RecordType, conflict.Name));
        var total = conflict.Lines.Single(line => line.Label == "Total");
        Assert.Equal(("$9.00", "$9.50", true), (total.ThisDevice, total.OtherDevice, total.Differs));
        Assert.False(conflict.Lines.Single(line => line.Label == "Date").Differs);
    }

    [Fact]
    public async Task A_settings_conflict_shows_appearance_and_home_currency()
    {
        await ConnectBothAsync();
        Later();
        await _phone.Settings.SetAppearanceAsync(Appearance.Dark);
        Later();
        await _desktop.Settings.SetHomeCurrencyAsync(Money.Cad);

        await SyncAllAsync(_phone, _desktop);

        var conflict = Assert.Single(await _desktop.Sync.ListConflictsAsync());
        Assert.Equal(("Settings", "Settings"), (conflict.RecordType, conflict.Name));
        Assert.Equal(
            [("Appearance", "Match my device", "Dark"), ("Home Currency", "CAD", "USD")],
            conflict.Lines.Select(line => (line.Label, line.ThisDevice, line.OtherDevice)));
    }

    [Fact]
    public async Task Keeping_this_devices_version_sends_it_to_the_other_device()
    {
        var receipt = await ConflictingReceiptAsync();
        var conflict = Assert.Single(await _desktop.Sync.ListConflictsAsync());
        Later();

        await _desktop.Sync.SettleAsync(conflict.Id, SyncConflictChoice.ThisDevice);
        await SyncAllAsync(_desktop, _phone);

        Assert.Equal(0, await PendingAsync(_desktop));
        Assert.Equal(9m, await TotalAsync(_phone, receipt));
        Assert.Equal(9m, await TotalAsync(_desktop, receipt));
    }

    [Fact]
    public async Task Keeping_the_other_devices_version_takes_it_here_and_it_stays_settled()
    {
        var receipt = await ConflictingReceiptAsync();
        var conflict = Assert.Single(await _desktop.Sync.ListConflictsAsync());
        Later();

        await _desktop.Sync.SettleAsync(conflict.Id, SyncConflictChoice.OtherDevice);

        Assert.Equal(9.50m, await TotalAsync(_desktop, receipt));
        await SyncAllAsync(_desktop, _phone, _desktop);
        Assert.Equal(0, await PendingAsync(_desktop));
        Assert.Equal(0, await PendingAsync(_phone));
        Assert.Equal(9.50m, await TotalAsync(_phone, receipt));
    }

    [Fact]
    public async Task Settling_a_conflict_that_is_no_longer_waiting_is_refused()
    {
        await ConflictingReceiptAsync();
        var conflict = Assert.Single(await _desktop.Sync.ListConflictsAsync());
        await _desktop.Sync.SettleAsync(conflict.Id, SyncConflictChoice.ThisDevice);

        await Assert.ThrowsAsync<RuleViolationException>(() => _desktop.Sync.SettleAsync(conflict.Id, SyncConflictChoice.OtherDevice));
    }

    [Fact]
    public async Task A_deletion_on_one_device_and_an_edit_on_the_other_is_a_conflict()
    {
        await ConnectBothAsync();
        var receipt = await _phone.Receipts.AddReceiptAsync(Today, 10m);
        await SyncAllAsync(_phone, _desktop);
        Later();
        await _phone.Receipts.DeleteReceiptAsync(receipt.Id);
        Later();
        await UpdateTotalAsync(_desktop, receipt, 11m);

        await SyncAllAsync(_phone, _desktop);

        var conflict = Assert.Single(await _desktop.Sync.ListConflictsAsync());
        Assert.Equal(("No", "Yes"), conflict.Lines.Where(line => line.Label == "Deleted").Select(line => (line.ThisDevice, line.OtherDevice)).Single());
        await _desktop.Sync.SettleAsync(conflict.Id, SyncConflictChoice.OtherDevice);
        Assert.Null(await _desktop.Receipts.GetReceiptAsync(receipt.Id));
        await SyncAllAsync(_desktop, _phone);
        Assert.Null(await _phone.Receipts.GetReceiptAsync(receipt.Id));
        Assert.Equal(0, await PendingAsync(_desktop));
    }

    [Fact]
    public async Task A_record_that_differs_on_a_devices_first_sync_is_a_conflict()
    {
        // The desktop got the phone's Receipt from a backup, so it has the record but no sync history for it.
        var receipt = await _phone.Receipts.AddReceiptAsync(Today, 10m);
        await _desktop.Backup.ImportAsync(await _phone.Backup.ExportAsync());
        await _phone.Sync.ConnectAsync();
        Later();
        await UpdateTotalAsync(_phone, receipt, 9.50m);
        await _phone.Sync.SyncNowAsync();
        Later();
        await UpdateTotalAsync(_desktop, receipt, 9m);

        await _desktop.Sync.ConnectAsync();
        await _desktop.Sync.SyncNowAsync();

        Assert.Equal(1, await PendingAsync(_desktop));
    }

    [Fact]
    public async Task Reconnecting_keeps_the_sync_history_so_an_edit_made_while_disconnected_is_not_a_conflict()
    {
        await ConnectBothAsync();
        var receipt = await _phone.Receipts.AddReceiptAsync(Today, 10m);
        await SyncAllAsync(_phone, _desktop);
        await _desktop.Sync.DisconnectAsync();
        Later();
        await UpdateTotalAsync(_desktop, receipt, 9m);

        await _desktop.Sync.ConnectAsync();
        await SyncAllAsync(_desktop, _phone);

        Assert.Equal(0, await PendingAsync(_desktop));
        Assert.Equal(9m, await TotalAsync(_phone, receipt));
    }
}
