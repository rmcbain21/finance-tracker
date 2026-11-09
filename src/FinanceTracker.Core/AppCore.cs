using FinanceTracker.Core.Ports;
using FinanceTracker.Core.Receipts;

namespace FinanceTracker.Core;

/// <summary>
/// The application-level API the UI uses. Each area of the app hangs off it.
/// It depends only on the ports, so tests run it against in-memory fakes.
/// </summary>
public sealed class AppCore
{
    public AppCore(IRecordStore store, IClock clock)
    {
        Receipts = new ReceiptCatalogue(store, clock);
    }

    public ReceiptCatalogue Receipts { get; }
}
