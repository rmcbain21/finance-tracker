using System.Text.Json;
using System.Text.Json.Nodes;
using FinanceTracker.Core.Ports;
using FinanceTracker.Core.Preferences;
using FinanceTracker.Core.Receipts;

namespace FinanceTracker.Core.Sync;

/// <summary>
/// One table of stored records, with the record type it holds, so whole-database operations (export, import
/// and sync) can read and write every table without knowing each type.
/// </summary>
internal abstract class RecordTable
{
    // The same JSON shape the record store uses, so a record looks the same in the file as on the device.
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Every synced table, in the order they appear in the file and in import summaries. This is the one list
    /// of synced tables: a new record type is added here, and sync, export and import all pick it up (give it
    /// lines in <see cref="ConflictDescriber"/> too). Device-only tables, such as the sync bookkeeping, stay out.
    /// </summary>
    public static readonly IReadOnlyList<RecordTable> All =
    [
        new RecordTable<Receipt>(ReceiptCatalogue.Table, "Receipt", "Receipts"),
        new RecordTable<Settings>(UserSettings.Table, "Settings", "Settings"),
    ];

    protected RecordTable(string name, string recordName, string recordTypeName)
    {
        Name = name;
        RecordName = recordName;
        RecordTypeName = recordTypeName;
    }

    public static RecordTable Named(string name) =>
        All.FirstOrDefault(table => table.Name == name) ?? throw new InvalidOperationException($"No table named {name}.");

    /// <summary>The table's name in the record store and the file, such as "receipts".</summary>
    public string Name { get; }

    /// <summary>What the Owner calls one of these records, such as "Receipt".</summary>
    public string RecordName { get; }

    /// <summary>What the Owner calls these records, such as "Receipts".</summary>
    public string RecordTypeName { get; }

    /// <summary>Every record in the table, tombstones included.</summary>
    public abstract Task<IReadOnlyList<StoredRecord>> ListAsync(IRecordStore store);

    /// <summary>One record, tombstone or not; null if the table has no record with that id.</summary>
    public abstract Task<StoredRecord?> LoadAsync(IRecordStore store, string id);

    /// <summary>Reads one record of this table's type; throws <see cref="JsonException"/> if it isn't one.</summary>
    public abstract StoredRecord Read(JsonNode node);

    public JsonNode Write(StoredRecord record) => JsonSerializer.SerializeToNode(record, record.GetType(), JsonOptions)!;

    /// <summary>
    /// Whether two versions of a record hold the same contents, ignoring when they were changed. Compared as
    /// JSON because a record's own equality compares its lists (such as Receipt Lines, later) by reference.
    /// </summary>
    public bool SameContents(StoredRecord first, StoredRecord second)
    {
        var (a, b) = ((JsonObject)Write(first), (JsonObject)Write(second));
        a.Remove("updatedAt");
        b.Remove("updatedAt");
        return JsonNode.DeepEquals(a, b);
    }
}

internal sealed class RecordTable<T> : RecordTable where T : StoredRecord
{
    public RecordTable(string name, string recordName, string recordTypeName) : base(name, recordName, recordTypeName)
    {
    }

    public override async Task<IReadOnlyList<StoredRecord>> ListAsync(IRecordStore store) => await store.ListAsync<T>(Name);

    public override async Task<StoredRecord?> LoadAsync(IRecordStore store, string id) => await store.LoadAsync<T>(Name, id);

    public override StoredRecord Read(JsonNode node) =>
        node.Deserialize<T>(JsonOptions) is { Id: not null } record
            ? record
            : throw new JsonException($"A record in {Name} has no id.");
}
