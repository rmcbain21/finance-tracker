# 02: Google Drive sync, JSON backup and Settings

**What to build:** Copy Recipe Tracker's sync, backup and settings and rename them: the sync engine (merge per record, most recent edit wins, Sync Conflicts settled by the Owner), the Google Drive adapter (`googleDrive.js`, `appDataFolder`, `drive.appdata` scope), auto-sync, the sync status icon and banner, the Sync Conflicts page, JSON export/import, and a Settings page with Appearance (light, dark or match device) and Home Currency (USD default, CAD). Sync Conflicts need a side-by-side view per record type; each can start as a summary with "keep this device / keep other device".

**Blocked by:** 01

**Status:** ready-for-human (two-device check)

- [x] **Human:** create a **new** Google Cloud OAuth client (web) for Finance Tracker with authorised JavaScript origins `https://rmcbain21.github.io` and a new `http://localhost:<port>` not used by the sibling apps, and give its client id for `wwwroot/appsettings.json`. Don't reuse a sibling's client, because Drive's `appDataFolder` belongs to the OAuth client.
- [x] The sync engine and its tests come over with names changed; the sync file is `finance-tracker.json`.
- [ ] Records added on the phone show up on the desktop after a sync, and deletions sync as tombstones.
- [ ] A conflicting edit shows on the Sync Conflicts page with a badge on Settings, and the Owner picks a version.
- [x] Export downloads all records as JSON, and importing that file into an empty app restores them.
- [x] Appearance and Home Currency sync through the Settings record (fixed id), and the top bar has a Settings icon.
- [x] The sync engine's table list is kept in one place, so later tickets add their tables there.

## Comments

**2026-11-14 (agent):** Implemented, except for the OAuth client id.

Recipe Tracker's ticket 02 isn't done yet, so the sync engine, Drive adapter, auto-sync, Sync Conflicts page and backup were copied from **Backpacking Tracker** and renamed (Owner, Receipts). Settings and `ThemeState` follow Recipe Tracker's shape, but only with Appearance and Home Currency (no palettes or text size).

- Core `Sync/`: `CloudSync`, `AutoSync`, `Backup`, `RecordCombine`, `SyncFile`, `SyncState`, `ChangeWatchingRecordStore`, `ConflictDescriber`. **The one list of synced tables is `RecordTable.All`** (`receipts`, `settings` for now). A new record type goes there, plus a `case` in `ConflictDescriber.LinesOf` (and `NameOf`) for its side-by-side view. `CloudSync` and `Backup` take an `arrived` callback, which is a no-op in `AppCore` for now. Ticket 09 (repointing Merges) and ticket 11 (generating Pending Charges after sync) hook in there.
- Core `Preferences/`: `Settings` (Appearance, HomeCurrency as a `Money.Currencies` code) with the fixed id `00000000-0000-0000-0000-000000000001`; `UserSettings.SetAppearanceAsync` and `SetHomeCurrencyAsync`.
- Pulled forward from 03, core only: `ReceiptCatalogue.GetReceiptAsync`, `UpdateReceiptAsync(id, date, total)` and `DeleteReceiptAsync(id)` (tombstone). Sync's edit, delete and conflict tests need them. There's no edit or delete UI yet; 03 adds it.
- Web: `GoogleDriveFileStore` + `wwwroot/js/googleDrive.js` (`drive.appdata`, file `finance-tracker.json`), `network.js`, `AutoSyncRunner`, `PendingConflicts`, `RefreshAfterSync` (on Home), `Pages/SettingsPage.razor`, `Pages/SyncConflicts.razor`, and in `MainLayout` the sync status icon, Settings icon with a conflict badge, and the "Sync is paused" banner.
- Tests: 91 pass (`SyncTests`, `SyncConflictTests`, `AutoSyncTests`, `BackupTests`, `SettingsTests`, plus edit and delete in `ReceiptTests`). Conflicts cover Receipts and Settings side by side.
- Headless check against a Release publish (phone and desktop viewports): adding a Receipt, Settings → Dark and CAD surviving a reload, Export (`finance-tracker-YYYY-MM-DD.json`, no `syncState`), importing it into an empty app (summary "Receipts: 1 added · Settings: 1 added", Dark applied, Receipt on Home), and an empty Sync Conflicts page. No console errors.

Still to do by hand:

1. Create the OAuth client (Google Cloud Console → APIs & Services → Credentials → Create credentials → OAuth client ID → Web application; enable the Google Drive API; authorised JavaScript origins `https://rmcbain21.github.io` and `http://localhost:5320`). Put its client id in `src/FinanceTracker.Web/wwwroot/appsettings.json` in place of the `REPLACE-WITH-…` placeholder. Until then, Connect Google Drive fails at Google's sign-in.
2. Two-device check: a Receipt added on the phone shows on the desktop after a sync. For a conflict, change Appearance differently on each device before syncing. A conflict should show on Sync Conflicts with a badge on the Settings icon. Deletions can only be checked by tests until 03 adds Receipt delete to the UI.

**2026-11-14 (agent):** OAuth client created (own Google Cloud project, Testing mode, Owner as test user). Its client id is in `wwwroot/appsettings.json`. Build is clean and all 91 tests pass. Remaining: deploy and run the two-device check.
