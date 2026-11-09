# 01: Walking skeleton: add and list Receipts

**What to build:** The first end-to-end path, with the shape every later ticket builds on. I can open the app on my phone, add a Receipt with just a date and a total (Money, in USD for now), and see it on the home screen. It's still there after closing and reopening the app with no signal. Copy the plumbing from Recipe Tracker and rename it: the solution layout (`FinanceTracker.slnx`, `src/FinanceTracker.Core`, `src/FinanceTracker.Web`, `tests/FinanceTracker.Core.Tests`), the app core's public API (`AppCore`), the record store and clock ports, the Dexie record store adapter (vendored `wwwroot/lib/dexie`, `recordStore.js`, database `finance-tracker`), the test project's in-memory fakes, and the GitHub Pages workflow. See the spec's Implementation Decisions and ADR 0001.

**Blocked by:** None (can start immediately)

**Status:** ready-for-human (Android install check)

- [x] The core depends only on the record store and clock ports; tests use an in-memory record store and a controllable clock.
- [x] Every record has a GUID `id`, a UTC `updatedAt` from the clock and a `deleted` tombstone flag.
- [x] A `Money` value type (decimal amount + ISO currency code) exists from the start; no amount is ever a bare number (ADR 0002).
- [x] Through the core API I can add a Receipt (date, total) and list Receipts newest first; tests cover adding, listing and persistence.
- [x] The Dexie adapter stores records in one `records` table keyed by `[table+id]`; data survives a reload.
- [x] The home page lists Receipts (date, total) and has a "＋" button that adds one.
- [ ] A Release publish installs from Chrome on Android and works offline after the first visit.

## Comments

**2026-11-09 (agent):** Implemented. `AppCore` exposes `Receipts` (`ReceiptCatalogue`: `AddReceiptAsync(date, total)`, `ListReceiptsAsync()`), depending only on `IRecordStore` and `IClock`. `Money` (`FinanceTracker.Core.Money`, root namespace) is a record struct of amount + currency, built with `Money.Of`, which refuses unknown currencies (only USD and CAD) and fractions of a cent; it formats as `$84.37`, `CA$84.37` or `−$40.00`. Rules on adding: the total must be above zero (Refunds come in 07) and the date can't be after today. Every Receipt is USD until Stores arrive in 03. 20 core tests pass (`ReceiptTests`, `MoneyTests`).

Copied from Recipe Tracker and renamed: `StoredRecord`, `RuleViolationException`, the ports, the fakes, `DexieRecordStore`, `SystemClock`, `recordStore.js` (database `finance-tracker`), vendored Dexie, both service workers, and `deploy.yml` (base path `/finance-tracker/`). Not copied yet: palettes, `ThemeState`, navigation and Settings. The layout has one green theme that follows the device's dark mode until 02 adds Appearance. New: favicon and PNG icons (a receipt with a rising line), dev ports 5320/7120.

Checked in a headless browser against a Release publish (phone-sized viewport): empty state, adding a Receipt, the zero-total rule message, the Receipt surviving a reload, and the app loading offline with the Receipt still there once the service worker has cached it. No console errors.

Still to check by hand: installing from Chrome on Android and working offline. That needs the GitHub repo and Pages set up (Settings → Pages → Source: GitHub Actions), since there's no remote yet.
