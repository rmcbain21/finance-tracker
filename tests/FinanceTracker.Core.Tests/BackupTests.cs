using System.Text.Json.Nodes;
using FinanceTracker.Core.Preferences;
using FinanceTracker.Core.Receipts;
using FinanceTracker.Core.Sync;
using FinanceTracker.Core.Tests.Fakes;

namespace FinanceTracker.Core.Tests;

/// <summary>Exporting everything to a JSON file and importing such a file back, combined record by record.</summary>
public class BackupTests
{
    private static readonly DateOnly Today = new(2026, 11, 9);

    private readonly FakeClock _clock = new(new DateTimeOffset(2026, 11, 9, 18, 0, 0, TimeSpan.Zero));
    private readonly AppCore _phone;
    private readonly AppCore _desktop;

    public BackupTests()
    {
        // Two devices with their own data, sharing one clock so tests control which edit is newer.
        _phone = new AppCore(new InMemoryRecordStore(), _clock);
        _desktop = new AppCore(new InMemoryRecordStore(), _clock);
    }

    private void Later() => _clock.UtcNow = _clock.UtcNow.AddMinutes(5);

    private static Task<Receipt> AddReceiptAsync(AppCore app, decimal total) => app.Receipts.AddReceiptAsync(Today, total);

    private static Task UpdateTotalAsync(AppCore app, Receipt receipt, decimal total) =>
        app.Receipts.UpdateReceiptAsync(receipt.Id, receipt.Date, total);

    private static async Task<decimal[]> TotalsAsync(AppCore app) =>
        (await app.Receipts.ListReceiptsAsync()).Select(receipt => receipt.Total.Amount).Order().ToArray();

    /// <summary>A phone with a bit of everything: Receipts, a deleted one, and Settings.</summary>
    private async Task FillPhoneAsync()
    {
        await AddReceiptAsync(_phone, 84.37m);
        await _phone.Receipts.AddReceiptAsync(Today.AddDays(-3), 12m);
        var gone = await AddReceiptAsync(_phone, 5m);
        await _phone.Receipts.DeleteReceiptAsync(gone.Id);
        await _phone.Settings.SetAppearanceAsync(Appearance.Dark);
        await _phone.Settings.SetHomeCurrencyAsync(Money.Cad);
    }

    [Fact]
    public async Task An_export_holds_a_format_version_and_every_record_including_deleted_ones()
    {
        var kept = await AddReceiptAsync(_phone, 10m);
        var gone = await AddReceiptAsync(_phone, 20m);
        await _phone.Receipts.DeleteReceiptAsync(gone.Id);

        var file = JsonNode.Parse(await _phone.Backup.ExportAsync())!;

        Assert.Equal(SyncFile.FormatVersion, (int)file["formatVersion"]!);
        var receipts = file["records"]!["receipts"]!.AsArray();
        Assert.Contains(receipts, record => (string)record!["id"]! == kept.Id);
        Assert.Contains(receipts, record => (string)record!["id"]! == gone.Id && (bool)record["deleted"]!);
    }

    [Fact]
    public async Task Exporting_then_importing_into_an_empty_app_reproduces_the_same_data()
    {
        await FillPhoneAsync();
        var export = await _phone.Backup.ExportAsync();

        await _desktop.Backup.ImportAsync(export);

        Assert.Equal(export, await _desktop.Backup.ExportAsync());
        Assert.Equal([12m, 84.37m], await TotalsAsync(_desktop));
        var settings = await _desktop.Settings.GetAsync();
        Assert.Equal((Appearance.Dark, Money.Cad), (settings.Appearance, settings.HomeCurrency));
    }

    [Fact]
    public async Task Importing_the_same_file_twice_changes_nothing_the_second_time()
    {
        await FillPhoneAsync();
        var export = await _phone.Backup.ExportAsync();
        await _desktop.Backup.ImportAsync(export);
        var afterFirst = await _desktop.Backup.ExportAsync();

        var summary = await _desktop.Backup.ImportAsync(export);

        Assert.True(summary.NothingChanged);
        Assert.Equal(afterFirst, await _desktop.Backup.ExportAsync());
    }

    [Fact]
    public async Task Records_only_in_the_file_are_added_alongside_existing_ones()
    {
        await AddReceiptAsync(_phone, 1m);
        await AddReceiptAsync(_desktop, 2m);

        await _desktop.Backup.ImportAsync(await _phone.Backup.ExportAsync());

        Assert.Equal([1m, 2m], await TotalsAsync(_desktop));
    }

    [Fact]
    public async Task A_newer_version_in_the_file_replaces_the_local_one()
    {
        var receipt = await AddReceiptAsync(_phone, 10m);
        await _desktop.Backup.ImportAsync(await _phone.Backup.ExportAsync());
        Later();
        await UpdateTotalAsync(_phone, receipt, 11m);

        await _desktop.Backup.ImportAsync(await _phone.Backup.ExportAsync());

        Assert.Equal(11m, (await _desktop.Receipts.GetReceiptAsync(receipt.Id))!.Total.Amount);
    }

    [Fact]
    public async Task An_older_version_in_the_file_leaves_the_local_one_alone()
    {
        var receipt = await AddReceiptAsync(_phone, 10m);
        var oldBackup = await _phone.Backup.ExportAsync();
        Later();
        await UpdateTotalAsync(_phone, receipt, 11m);

        var summary = await _phone.Backup.ImportAsync(oldBackup);

        Assert.True(summary.NothingChanged);
        Assert.Equal(11m, (await _phone.Receipts.GetReceiptAsync(receipt.Id))!.Total.Amount);
    }

    [Fact]
    public async Task A_deletion_in_the_file_deletes_an_older_local_record()
    {
        var receipt = await AddReceiptAsync(_phone, 10m);
        await _desktop.Backup.ImportAsync(await _phone.Backup.ExportAsync());
        Later();
        await _phone.Receipts.DeleteReceiptAsync(receipt.Id);

        await _desktop.Backup.ImportAsync(await _phone.Backup.ExportAsync());

        Assert.Null(await _desktop.Receipts.GetReceiptAsync(receipt.Id));
    }

    [Fact]
    public async Task A_deletion_in_the_file_does_not_delete_a_record_edited_since()
    {
        var receipt = await AddReceiptAsync(_phone, 10m);
        await _desktop.Backup.ImportAsync(await _phone.Backup.ExportAsync());
        Later();
        await _phone.Receipts.DeleteReceiptAsync(receipt.Id);
        Later();
        await UpdateTotalAsync(_desktop, receipt, 11m);

        await _desktop.Backup.ImportAsync(await _phone.Backup.ExportAsync());

        Assert.Equal(11m, (await _desktop.Receipts.GetReceiptAsync(receipt.Id))!.Total.Amount);
    }

    [Fact]
    public async Task A_deletion_only_in_the_file_is_kept_so_it_still_syncs_but_is_not_reported()
    {
        var receipt = await AddReceiptAsync(_phone, 10m);
        await _phone.Receipts.DeleteReceiptAsync(receipt.Id);

        var summary = await _desktop.Backup.ImportAsync(await _phone.Backup.ExportAsync());

        Assert.True(summary.NothingChanged);
        Assert.Equal(await _phone.Backup.ExportAsync(), await _desktop.Backup.ExportAsync());
    }

    [Fact]
    public async Task The_summary_counts_what_was_added_updated_and_deleted_for_each_record_type()
    {
        var kept = await AddReceiptAsync(_phone, 10m);
        var gone = await AddReceiptAsync(_phone, 20m);
        await _desktop.Backup.ImportAsync(await _phone.Backup.ExportAsync());
        Later();
        await UpdateTotalAsync(_phone, kept, 11m);
        await _phone.Receipts.DeleteReceiptAsync(gone.Id);
        await AddReceiptAsync(_phone, 30m);
        await _phone.Settings.SetAppearanceAsync(Appearance.Dark);

        var summary = await _desktop.Backup.ImportAsync(await _phone.Backup.ExportAsync());

        Assert.False(summary.NothingChanged);
        Assert.Equal(
            [new RecordTypeChanges("Receipts", Added: 1, Updated: 1, Deleted: 1), new RecordTypeChanges("Settings", Added: 1, Updated: 0, Deleted: 0)],
            summary.Changes);
    }

    [Fact]
    public async Task A_file_with_an_unknown_format_version_is_refused_and_changes_nothing()
    {
        await AddReceiptAsync(_phone, 10m);
        var file = JsonNode.Parse(await _phone.Backup.ExportAsync())!;
        file["formatVersion"] = SyncFile.FormatVersion + 1;
        await AddReceiptAsync(_desktop, 20m);
        var before = await _desktop.Backup.ExportAsync();

        var refusal = await Assert.ThrowsAsync<RuleViolationException>(() => _desktop.Backup.ImportAsync(file.ToJsonString()));

        Assert.Contains("version", refusal.Message);
        Assert.Equal(before, await _desktop.Backup.ExportAsync());
    }

    [Theory]
    [InlineData("not json at all")]
    [InlineData("""{"name": "something else"}""")]
    [InlineData("""{"formatVersion": 1, "records": {"receipts": [{"date": "2026-11-09"}]}}""")]
    public async Task A_file_that_is_not_an_export_is_refused_and_changes_nothing(string text)
    {
        await AddReceiptAsync(_desktop, 20m);
        var before = await _desktop.Backup.ExportAsync();

        var refusal = await Assert.ThrowsAsync<RuleViolationException>(() => _desktop.Backup.ImportAsync(text));

        Assert.Contains("export", refusal.Message);
        Assert.Equal(before, await _desktop.Backup.ExportAsync());
    }
}
