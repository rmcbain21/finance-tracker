# 01: Walking skeleton: add and list Receipts

**What to build:** The first end-to-end path, with the shape every later ticket builds on. I can open the app on my phone, add a Receipt with just a date and a total (Money, in USD for now), and see it on the home screen. It's still there after closing and reopening the app with no signal. Copy the plumbing from Recipe Tracker and rename it: the solution layout (`FinanceTracker.slnx`, `src/FinanceTracker.Core`, `src/FinanceTracker.Web`, `tests/FinanceTracker.Core.Tests`), the app core's public API (`AppCore`), the record store and clock ports, the Dexie record store adapter (vendored `wwwroot/lib/dexie`, `recordStore.js`, database `finance-tracker`), the test project's in-memory fakes, and the GitHub Pages workflow. See the spec's Implementation Decisions and ADR 0001.

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

- [ ] The core depends only on the record store and clock ports; tests use an in-memory record store and a controllable clock.
- [ ] Every record has a GUID `id`, a UTC `updatedAt` from the clock and a `deleted` tombstone flag.
- [ ] A `Money` value type (decimal amount + ISO currency code) exists from the start; no amount is ever a bare number (ADR 0002).
- [ ] Through the core API I can add a Receipt (date, total) and list Receipts newest first; tests cover adding, listing and persistence.
- [ ] The Dexie adapter stores records in one `records` table keyed by `[table+id]`; data survives a reload.
- [ ] The home page lists Receipts (date, total) and has a "＋" button that adds one.
- [ ] A Release publish installs from Chrome on Android and works offline after the first visit.

## Comments
