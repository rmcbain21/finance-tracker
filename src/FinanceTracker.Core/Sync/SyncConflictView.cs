using System.Globalization;
using FinanceTracker.Core.Preferences;
using FinanceTracker.Core.Receipts;

namespace FinanceTracker.Core.Sync;

/// <summary>Which version settles a Sync Conflict.</summary>
public enum SyncConflictChoice
{
    ThisDevice,
    OtherDevice,
}

/// <summary>
/// A Sync Conflict as the Owner sees it: the record, and its two versions side by side.
/// Small records list every line; large ones (Receipts with many Receipt Lines, later) may list only the
/// lines that differ, with <see cref="SameCount"/> saying how many others are the same.
/// </summary>
public sealed record SyncConflictView(string Id, string RecordType, string Name, IReadOnlyList<ConflictLine> Lines, int SameCount);

/// <summary>One detail of a record, such as "Total", in this device's version and the other device's.</summary>
public sealed record ConflictLine(string Label, string ThisDevice, string OtherDevice)
{
    public bool Differs => ThisDevice != OtherDevice;
}

/// <summary>Turns the two versions of a record into <see cref="ConflictLine"/>s.</summary>
internal static class ConflictDescriber
{
    private const string Empty = "—";
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public static SyncConflictView Describe(string id, RecordTable table, StoredRecord mine, StoredRecord theirs)
    {
        var myLines = LinesOf(mine);
        var theirLines = LinesOf(theirs);
        var lines = myLines.Keys.Union(theirLines.Keys)
            .Select(label => new ConflictLine(label, myLines.GetValueOrDefault(label, Empty), theirLines.GetValueOrDefault(label, Empty)))
            .ToList();
        if (mine.Deleted || theirs.Deleted)
        {
            lines.Insert(0, new ConflictLine("Deleted", YesNo(mine.Deleted), YesNo(theirs.Deleted)));
        }

        return new SyncConflictView(id, table.RecordName, NameOf(mine.Deleted ? theirs : mine), lines, SameCount: 0);
    }

    private static string NameOf(StoredRecord record) => record switch
    {
        Receipt receipt => $"{Date(receipt.Date)}, {receipt.Total}",
        _ => "Settings",
    };

    private static Lines LinesOf(StoredRecord record)
    {
        var lines = new Lines();
        switch (record)
        {
            case Receipt receipt:
                lines.Put("Date", Date(receipt.Date));
                lines.Put("Total", receipt.Total.ToString());
                break;
            case Settings settings:
                lines.Put("Appearance", settings.Appearance == Appearance.MatchDevice ? "Match my device" : settings.Appearance.ToString());
                lines.Put("Home Currency", settings.HomeCurrency);
                break;
        }

        return lines;
    }

    private static string Date(DateOnly date) => date.ToString("ddd, MMM d, yyyy", Culture);

    private static string YesNo(bool value) => value ? "Yes" : "No";

    /// <summary>A record's details by label, in order. Blank values show as a dash; a repeated label gets a number.</summary>
    private sealed class Lines : OrderedDictionary<string, string>
    {
        public void Put(string label, string? value)
        {
            var unique = label;
            for (var n = 2; ContainsKey(unique); n++)
            {
                unique = $"{label} ({n})";
            }

            Add(unique, string.IsNullOrWhiteSpace(value) ? Empty : value);
        }
    }
}
