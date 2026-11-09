# 02: Google Drive sync, JSON backup and Settings

**What to build:** Copy Recipe Tracker's sync, backup and settings and rename them: the sync engine (merge per record, most recent edit wins, Sync Conflicts settled by the Owner), the Google Drive adapter (`googleDrive.js`, `appDataFolder`, `drive.appdata` scope), auto-sync, the sync status icon and banner, the Sync Conflicts page, JSON export/import, and a Settings page with Appearance (light, dark or match device) and Home Currency (USD default, CAD). Sync Conflicts need a side-by-side view per record type; each can start as a summary with "keep this device / keep other device".

**Blocked by:** 01

**Status:** ready-for-human (one manual step first), then ready-for-agent

- [ ] **Human:** create a **new** Google Cloud OAuth client (web) for Finance Tracker with authorised JavaScript origins `https://rmcbain21.github.io` and a new `http://localhost:<port>` not used by the sibling apps, and give its client id for `wwwroot/appsettings.json`. Don't reuse a sibling's client, because Drive's `appDataFolder` belongs to the OAuth client.
- [ ] The sync engine and its tests come over with names changed; the sync file is `finance-tracker.json`.
- [ ] Records added on the phone show up on the desktop after a sync, and deletions sync as tombstones.
- [ ] A conflicting edit shows on the Sync Conflicts page with a badge on Settings, and the Owner picks a version.
- [ ] Export downloads all records as JSON, and importing that file into an empty app restores them.
- [ ] Appearance and Home Currency sync through the Settings record (fixed id), and the top bar has a Settings icon.
- [ ] The sync engine's table list is kept in one place, so later tickets add their tables there.

## Comments
