# 05: Receipt Photo sync through Drive

**What to build:** Receipt Photos sync between devices as separate files in the Drive `appDataFolder`, never inside the sync file (ADR 0001). An upload queue sends local photos with no Drive file id when signed in and online, then records the Drive file id on the Receipt Photo record, which syncs normally. On another device, a Receipt Photo with a Drive file id but no local blob is downloaded when opened (and, optionally, in the background on Wi-Fi). Deleting a Receipt Photo deletes its Drive file. Until a photo arrives, show a placeholder ("Photo still syncing").

**Blocked by:** 02, 04

**Status:** ready-for-agent

- [ ] The cloud file store port gains upload/download/delete for binary files, with a fake for tests.
- [ ] Tests cover queueing, retry after failure, a Receipt arriving before its photo, and deletion.
- [ ] Photos taken offline upload on the next sync.
- [ ] The sync status shows how many photos are waiting to upload.

## Comments
