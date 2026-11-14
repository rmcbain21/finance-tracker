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
        var receipt = new Receipt
        {
            Id = Guid.NewGuid().ToString(),
            UpdatedAt = _clock.UtcNow,
            Date = date,
            Total = Checked(date, total),
        };
        await _store.SaveAsync(Table, receipt);
        return receipt;
    }

    /// <summary>The Receipt, or null if there's none with that id or it was deleted.</summary>
    public async Task<Receipt?> GetReceiptAsync(string id) =>
        await _store.LoadAsync<Receipt>(Table, id) is { Deleted: false } receipt ? receipt : null;

    /// <summary>Changes a Receipt's date and total, under the same rules as adding one.</summary>
    /// <exception cref="RuleViolationException">The Receipt is gone, or the date or total breaks a rule.</exception>
    public async Task<Receipt> UpdateReceiptAsync(string id, DateOnly date, decimal total)
    {
        var receipt = await GetReceiptAsync(id) ?? throw Gone();
        var updated = receipt with { Date = date, Total = Checked(date, total), UpdatedAt = _clock.UtcNow };
        await _store.SaveAsync(Table, updated);
        return updated;
    }

    /// <summary>Deletes a Receipt. It's kept as a tombstone, so the deletion syncs to other devices.</summary>
    /// <exception cref="RuleViolationException">The Receipt is already gone.</exception>
    public async Task DeleteReceiptAsync(string id)
    {
        var receipt = await GetReceiptAsync(id) ?? throw Gone();
        await _store.SaveAsync(Table, receipt with { Deleted = true, UpdatedAt = _clock.UtcNow });
    }

    /// <summary>Every Receipt that isn't deleted, newest date first; on the same date, the one added last first.</summary>
    public async Task<IReadOnlyList<Receipt>> ListReceiptsAsync() =>
        (await _store.ListAsync<Receipt>(Table))
            .Where(receipt => !receipt.Deleted)
            .OrderByDescending(receipt => receipt.Date)
            .ThenByDescending(receipt => receipt.UpdatedAt)
            .ToList();

    private Money Checked(DateOnly date, decimal total)
    {
        if (total <= 0)
        {
            throw new RuleViolationException("Enter the total paid, more than zero.");
        }

        if (date > _clock.Today)
        {
            throw new RuleViolationException("A Receipt can't be dated in the future.");
        }

        return Money.Of(total, Money.Usd);
    }

    private static RuleViolationException Gone() => new("That Receipt has been deleted.");
}
