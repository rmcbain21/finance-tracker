using FinanceTracker.Core.Receipts;
using FinanceTracker.Core.Tests.Fakes;

namespace FinanceTracker.Core.Tests;

public class ReceiptTests
{
    private static readonly DateOnly Today = new(2026, 11, 9);

    private readonly InMemoryRecordStore _store = new();
    private readonly FakeClock _clock = new(new DateTimeOffset(2026, 11, 9, 18, 0, 0, TimeSpan.Zero));

    private AppCore CreateApp() => new(_store, _clock);

    [Fact]
    public async Task There_are_no_receipts_at_first()
    {
        Assert.Empty(await CreateApp().Receipts.ListReceiptsAsync());
    }

    [Fact]
    public async Task An_added_receipt_is_remembered_after_the_app_is_reopened()
    {
        var added = await CreateApp().Receipts.AddReceiptAsync(Today, 84.37m);

        var receipt = Assert.Single(await CreateApp().Receipts.ListReceiptsAsync());

        Assert.Equal(added.Id, receipt.Id);
        Assert.Equal(Today, receipt.Date);
        Assert.Equal(new Money(84.37m, "USD"), receipt.Total);
    }

    [Fact]
    public async Task A_new_receipt_gets_a_guid_id_the_clock_time_and_is_not_deleted()
    {
        var receipt = await CreateApp().Receipts.AddReceiptAsync(Today, 12m);

        Assert.True(Guid.TryParse(receipt.Id, out _));
        Assert.Equal(_clock.UtcNow, receipt.UpdatedAt);
        Assert.False(receipt.Deleted);
    }

    [Fact]
    public async Task Each_receipt_gets_its_own_id()
    {
        var app = CreateApp();

        var first = await app.Receipts.AddReceiptAsync(Today, 12m);
        var second = await app.Receipts.AddReceiptAsync(Today, 12m);

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public async Task Receipts_are_listed_newest_date_first_then_last_added_first()
    {
        var app = CreateApp();
        await app.Receipts.AddReceiptAsync(Today.AddDays(-3), 1m);
        await app.Receipts.AddReceiptAsync(Today, 2m);
        _clock.UtcNow += TimeSpan.FromMinutes(1);
        await app.Receipts.AddReceiptAsync(Today, 3m);
        await app.Receipts.AddReceiptAsync(Today.AddDays(-1), 4m);

        var totals = (await app.Receipts.ListReceiptsAsync()).Select(r => r.Total.Amount);

        Assert.Equal([3m, 2m, 4m, 1m], totals);
    }

    [Fact]
    public async Task Deleted_receipts_are_not_listed()
    {
        var app = CreateApp();
        var receipt = await app.Receipts.AddReceiptAsync(Today, 5m);
        await _store.SaveAsync("receipts", receipt with { Deleted = true });

        Assert.Empty(await app.Receipts.ListReceiptsAsync());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-4.50)]
    public async Task A_receipt_needs_a_total_above_zero(decimal total)
    {
        await Assert.ThrowsAsync<RuleViolationException>(() => CreateApp().Receipts.AddReceiptAsync(Today, total));
    }

    [Fact]
    public async Task A_total_cannot_have_fractions_of_a_cent()
    {
        await Assert.ThrowsAsync<RuleViolationException>(() => CreateApp().Receipts.AddReceiptAsync(Today, 3.459m));
    }

    [Fact]
    public async Task A_receipt_cannot_be_dated_in_the_future()
    {
        var app = CreateApp();

        await Assert.ThrowsAsync<RuleViolationException>(() => app.Receipts.AddReceiptAsync(Today.AddDays(1), 5m));
        Assert.Empty(await app.Receipts.ListReceiptsAsync());
    }

    [Fact]
    public async Task Receipts_are_in_us_dollars_until_stores_arrive()
    {
        var receipt = await CreateApp().Receipts.AddReceiptAsync(Today, 5m);

        Assert.Equal(Money.Usd, receipt.Total.Currency);
    }
}
