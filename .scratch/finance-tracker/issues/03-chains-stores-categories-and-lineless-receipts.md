# 03: Chains, Stores, Categories and Receipts without lines

**What to build:** A Receipt now belongs to a Store, which belongs to a Chain, and a Receipt with no lines has a Category. Adding a Receipt asks for Store (defaulting to the one used last), date, total and Category, and I can add a Chain and Store inline without leaving the form. A Store has a country (US or CA), which sets the Receipt's currency (USD or CAD) and later the default fuel unit. Categories are an editable list (Settings → Categories) seeded with Groceries, Gas, Subscriptions, Bills, Household, Misc and Uncategorized, using fixed ids and an epoch `updatedAt` so every device creates identical starters. Gas and Uncategorized can be renamed but not deleted. I can edit and delete Receipts (with confirmation). See the spec's Records and schema, and ADR 0002.

**Blocked by:** 02

**Status:** ready-for-agent

- [ ] Core: Chains, Stores (Chain, label, country), Categories (rename, reorder, add, delete when unused or after reassigning to another), Receipt with Store and Category; tests cover currency coming from the Store's country and deleting an in-use Category being blocked.
- [ ] The Add Receipt form defaults the Store to the last one used, with inline "new Chain / new Store".
- [ ] The home list shows Store, date, total with currency, and Category.
- [ ] Settings → Stores lists Chains with their Stores and allows rename and country change (with a warning that existing Receipts keep their currency).
- [ ] New tables are added to the sync engine's table list.

## Comments

**2026-11-14 (agent, from 02):** The core already has `ReceiptCatalogue.GetReceiptAsync`, `UpdateReceiptAsync(id, date, total)` and `DeleteReceiptAsync(id)` (tombstone), which sync's tests needed. Extend them for Store and Category rather than adding new ones. Add the new tables (`chains`, `stores`, `categories`) to `RecordTable.All` in `Core/Sync/RecordTable.cs`, and give each a `case` in `ConflictDescriber` (`Core/Sync/SyncConflictView.cs`). The starter Categories' epoch `updatedAt` is already handled by `RecordCombine` ("never edited" means no conflict on a new device's first sync). Back in Backpacking Tracker there's a sync test for this (`A_new_devices_untouched_default_categories_do_not_undo_edits_made_elsewhere`) worth copying.
