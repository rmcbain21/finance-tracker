using FinanceTracker.Core.Ports;

namespace FinanceTracker.Core.Receipts;

/// <summary>The Owner's Receipts.</summary>
public sealed class ReceiptCatalogue
{
    internal const string Table = "receipts";

    private readonly IRecordStore _store;
    private readonly IClock _clock;

    public ReceiptCatalogue(IRecordStore store, IClock clock)
    {
        _store = store;
        _clock = clock;
    }

    /// <summary>
    /// Adds a Receipt for the total paid on a date. Until Stores arrive (ticket 03), every
    /// Receipt is in US dollars.
    /// </summary>
    /// <exception cref="RuleViolationException">The total isn't more than zero, has fractions of a cent, or the date is in the future.</exception>
    public async Task<Receipt> AddReceiptAsync(DateOnly date, decimal total)
    {
        if (total <= 0)
        {
            throw new RuleViolationException("Enter the total paid, more than zero.");
        }

        if (date > _clock.Today)
        {
            throw new RuleViolationException("A Receipt can't be dated in the future.");
        }

        var receipt = new Receipt
        {
            Id = Guid.NewGuid().ToString(),
            UpdatedAt = _clock.UtcNow,
            Date = date,
            Total = Money.Of(total, Money.Usd),
        };
        await _store.SaveAsync(Table, receipt);
        return receipt;
    }

    /// <summary>Every Receipt that isn't deleted, newest date first; on the same date, the one added last first.</summary>
    public async Task<IReadOnlyList<Receipt>> ListReceiptsAsync() =>
        (await _store.ListAsync<Receipt>(Table))
            .Where(receipt => !receipt.Deleted)
            .OrderByDescending(receipt => receipt.Date)
            .ThenByDescending(receipt => receipt.UpdatedAt)
            .ToList();
}
