using FinanceTracker.Core.Ports;
using FinanceTracker.Core.Preferences;
using FinanceTracker.Core.Receipts;
using FinanceTracker.Core.Sync;

namespace FinanceTracker.Core;

/// <summary>
/// The application-level API the UI uses. Each area of the app hangs off it.
/// It depends only on the ports, so tests run it against in-memory fakes.
/// </summary>
public sealed class AppCore
{
    /// <param name="cloud">The cloud drive to sync through; null where there is none, such as tests that don't sync.</param>
    public AppCore(IRecordStore store, IClock clock, ICloudFileStore? cloud = null)
    {
        // The areas save through a store that tells automatic sync about each change; sync uses the plain one.
        // (AutoSync is set at the end of this constructor, before anything can save, hence the "!"s.)
        var watched = new ChangeWatchingRecordStore(store, () => AutoSync!.LocalChanged());
        Receipts = new ReceiptCatalogue(watched, clock);
        Settings = new UserSettings(watched, clock);

        // Later tickets hook in here what must happen after records arrive (repointing Merges, Pending Charges).
        Backup = new Backup(watched, arrived: _ => Task.CompletedTask);
        Sync = new CloudSync(store, clock, cloud, settled: () => AutoSync!.LocalChanged(), arrived: _ => Task.CompletedTask);
        AutoSync = new AutoSync(Sync, clock);
    }

    public ReceiptCatalogue Receipts { get; }

    public UserSettings Settings { get; }

    public Backup Backup { get; }

    public CloudSync Sync { get; }

    public AutoSync AutoSync { get; }
}
