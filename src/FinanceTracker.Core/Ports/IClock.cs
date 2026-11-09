namespace FinanceTracker.Core.Ports;

public interface IClock
{
    DateTimeOffset UtcNow { get; }

    /// <summary>Today's date where the Owner is, for dates such as a Receipt's.</summary>
    DateOnly Today { get; }
}
