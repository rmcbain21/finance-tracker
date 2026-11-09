namespace FinanceTracker.Core.Receipts;

/// <summary>
/// One purchase on a date, with the total actually paid. The total is what counts toward spending.
/// Stores, Receipt Lines, Categories and Receipt Photos arrive in later tickets.
/// </summary>
public sealed record Receipt : StoredRecord
{
    /// <summary>The day of the purchase, where the Owner was.</summary>
    public required DateOnly Date { get; init; }

    /// <summary>What was actually paid, in the currency it was paid in.</summary>
    public required Money Total { get; init; }
}
