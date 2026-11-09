# Spec: Finance Tracker v1

Status: ready-for-agent

## Problem Statement

I don't know what things actually cost me over time. I can see my card statement total, but not whether milk went up at Walmart, whether Costco is really cheaper per gallon, what I pay for gas per gallon and per mile, or how much my subscriptions and bills add up to each month. Budgeting apps track spending by category from bank feeds but know nothing about individual products or SKUs. Typing a whole receipt in at the checkout is too slow, so whatever I use has to let me capture it quickly and fill in details later.

## Solution

A personal app that installs from the browser on my phone, works offline, and also runs in a desktop browser for reading charts. At the checkout I save a Draft Receipt in seconds: pick the Store, take a Receipt Photo, type the total. Later I add Receipt Lines by typing each SKU and the price paid. A known SKU fills in its Store Item, and an unknown one creates a stub I can finish whenever. Linking Store Items to Products lets me compare Unit Prices across Stores. At the pump I record a Fill-up, and with a Vehicle and odometer reading the app tracks Fuel Economy. Recurring Charges such as streaming services and utility bills create Pending Charges each period, which I confirm or skip. Charts show monthly spending by Category, price history, gas prices and Fuel Economy, and recurring totals. Data lives on each device and syncs through my own Google Drive, with no server.

Architecture: ADR 0001. Currency and units: ADR 0002. Pending Charges: ADR 0003. Store Items per Chain: ADR 0004. Terms: `GLOSSARY.md`.

## User Stories

### Quick capture

1. As the Owner, I want to save a Draft Receipt with only a Store, a total and a Receipt Photo, so that capturing at the checkout takes seconds.
2. As the Owner, I want the Store to default to the one I used last, so that repeat trips are one tap.
3. As the Owner, I want to take the Receipt Photo with my phone camera or pick an existing image, so that I can capture now or later.
4. As the Owner, I want Draft Receipts listed at the top of the home screen until they're finished, so that I don't forget them.
5. As the Owner, I want a Draft Receipt's total to count toward spending as Uncategorized until it's finished, so that monthly totals are never missing money.

### Receipts

6. As the Owner, I want to add Receipt Lines by typing a SKU and the price paid, with a known SKU filling in its Store Item, so that entering a long receipt is fast.
7. As the Owner, I want an unknown SKU to create a Store Item stub that I can fill in later (receipt name, pack size, unit, default Category), so that entry never stops to ask for details.
8. As the Owner, I want each Receipt Line to have a quantity, the price actually paid after any line discount, and a Category defaulted from its Store Item, so that spending splits correctly (a Costco trip with food and a TV).
9. As the Owner, I want to record receipt-wide tax and discount, so that the Receipt matches the paper.
10. As the Owner, I want a warning, never a block, when the Receipt Lines plus tax minus discount don't add up to the total, so that I can deliberately skip lines I don't care about.
11. As the Owner, I want a Receipt with no lines (a haircut, a parking meter) that carries its own Category, so that one-off expenses are a three-field form.
12. As the Owner, I want to record a Refund as a Receipt with a negative total at the Store, optionally pointing at the Store Items returned, so that returns reduce spending in the month they happen without affecting price history.
13. As the Owner, I want to edit or delete any Receipt after confirming, so that I can fix mistakes.

### Stores and Categories

14. As the Owner, I want Stores to belong to a Chain, with the country (US or Canada) set on the Store, so that a Canadian Walmart records CAD and litres while sharing SKUs with every other Walmart.
15. As the Owner, I want to add a Chain and Store inline while entering a Receipt, so that a new place never interrupts capture.
16. As the Owner, I want an editable list of Categories seeded with Groceries, Gas, Subscriptions, Bills, Household, Misc and Uncategorized, so that spending groups the way I think.

### Products and prices

17. As the Owner, I want to link Store Items to a Product such as "2% Milk", so that I can compare its price across Stores and Chains.
18. As the Owner, I want each Product to have a Comparison Unit (such as $/gal, $/lb or $/each), set from the first Store Item linked and changeable, with links refused when units can't convert, so that comparisons are always valid.
19. As the Owner, I want Unit Prices worked out from pack size, quantity and price paid, so that a 4 L jug and a 1 gal jug can be compared.
20. As the Owner, I want to merge duplicate Store Items and Products, with all history repointed, so that "2% Milk" and "Milk 2%" become one.

### Gas

21. As the Owner, I want to record a Fill-up with Store, fuel grade, volume, price per unit and total (entering any two of the last three fills in the third), so that it takes seconds at the pump.
22. As the Owner, I want the volume unit to default from the Store's country (gal in the US, L in Canada), so that the numbers match the pump.
23. As the Owner, I want to optionally pick a Vehicle and enter its odometer reading, with "filled to full" on by default, so that Fuel Economy can be tracked.
24. As the Owner, I want to be offered "add a Receipt here too" after saving a Fill-up, so that the coffee I bought at the station is captured separately.

### Recurring

25. As the Owner, I want to set up a Recurring Charge with a payee, Category, amount, currency, Fixed or Variable, and a schedule (monthly on day N, yearly, or every N weeks), so that subscriptions and bills are tracked without retyping.
26. As the Owner, I want each period to create a Pending Charge that waits for me to confirm (editing the amount if needed) or skip, so that nothing is recorded that I didn't actually pay (ADR 0003).
27. As the Owner, I want missed Pending Charges to pile up oldest first, with one action to confirm every Fixed one, so that catching up after weeks away is quick.
28. As the Owner, I want changing a Recurring Charge's amount to apply only from the next period on, so that its price history shows real increases.
29. As the Owner, I want to end a Recurring Charge on a date without deleting it, so that cancelled subscriptions keep their history.

### Charts

30. As the Owner, I want a chart of monthly spending by Category, per currency, with a count of waiting Pending Charges and Draft Receipts, so that I see where money goes and how complete the month is.
31. As the Owner, I want a price history chart for a Store Item or Product, with one line per Store, in Unit Price, so that I can see what I pay over time and where it's cheapest.
32. As the Owner, I want a chart of gas price per unit over time, and Fuel Economy and cost per distance for each Vehicle, so that I can track both.
33. As the Owner, I want a Recurring page showing the monthly total of active Recurring Charges, their price history and what's coming up, so that I see what I'm committed to.

### Devices and data

34. As the Owner, I want the app to install to my home screen and work fully offline, so that it works in a store with no signal.
35. As the Owner, I want my data and Receipt Photos synced between devices through my Google Drive, with conflicts shown for me to settle, so that my phone and desktop agree.
36. As the Owner, I want to export Receipts, Receipt Lines, Fill-ups and confirmed Pending Charges to CSV, and everything to a JSON backup that I can import, so that the data is mine.
37. As the Owner, I want light, dark or match-device appearance, so that it's readable anywhere.

## Implementation Decisions

### Modules

- **App core**: a plain C# class library (`FinanceTracker.Core`, no Blazor or JavaScript) that holds the domain model and all behaviour, and exposes one application API (`AppCore`) used by the UI. Sub-areas:
  - **Places**: Chains, Stores, country and its currency and default fuel unit.
  - **Receipts**: Receipts, Receipt Lines, Draft Receipts, Refunds, tax/discount, total check.
  - **Catalogue**: Store Items, Products, Comparison Units, SKU lookup, Merge (ADR 0004).
  - **Money and Quantities**: amounts with currency, quantities with unit, unit conversion, Unit Price (ADR 0002).
  - **Gas**: Fill-ups, Vehicles, Fuel Economy.
  - **Recurring**: Recurring Charges, schedules, Pending Charge generation, confirm/skip (ADR 0003).
  - **Spending**: Categories, and the queries the charts use (spending by month and Category, price history, gas series, recurring totals).
  - **Photos**: Receipt Photo metadata and the photo sync queue.
  - **Sync engine, backup, Settings and CSV export**: sync, backup and Settings copied from Recipe Tracker and renamed; CSV export is new.
- **Ports** (copied from Recipe Tracker): record store (IndexedDB via Dexie, one `records` table keyed by `[table+id]`), cloud file store (Google Drive `appDataFolder`, `drive.appdata` scope only), clock. New: a **photo store** port (local blobs in IndexedDB, plus upload/download to Drive as separate files).
- **Web app**: a Blazor WebAssembly PWA with MudBlazor, on GitHub Pages. Pages are thin; logic lives in the core.
- **Receipt extraction** (phase 2, not v1): reserve an `IReceiptExtractor` interface that turns a Receipt Photo into a proposed Draft Receipt for review. Don't implement it.

### Records and schema

- Every record has a GUID `id`, a UTC `updatedAt` and a `deleted` tombstone flag from slice 1, so that sync needs no migration.
- Top-level record types: Chain, Store, Category, Receipt, Store Item, Product, Vehicle, Fill-up, Recurring Charge, Pending Charge, Receipt Photo, Settings.
- **Money**: decimal amount plus ISO currency code (`USD`, `CAD`). Never a bare number. Never converted.
- **Quantity**: decimal value plus unit code. Units: `each`, `g`, `kg`, `oz` (weight), `lb`, `mL`, `L`, `fl oz` (US), `gal` (US). Conversions only within mass or volume; `each` converts to nothing. `oz` always means weight.
- **Chain**: name. **Store**: Chain id, name or address label, country (`US` or `CA`). Currency and default fuel unit come from the country.
- **Category**: name, order. The starter Categories have fixed ids and an `updatedAt` of the Unix epoch, as in Recipe Tracker's starter Tag Groups. Uncategorized and Gas are fixed and can't be deleted.
- **Receipt**: Store id, date, total (Money), tax (Money, optional), receipt discount (Money, optional), Draft flag, Refund flag, Category id (used only when it has no lines), Receipt Photo ids, note, and nested Receipt Lines (id, order, Store Item id, quantity, price paid, Category id).
- **Store Item**: Chain id, SKU, receipt name, pack size (Quantity, optional on a stub), default Category id, Product id or none, and on a tombstone an optional "merged into" id.
- **Product**: name, Comparison Unit, and on a tombstone an optional "merged into" id.
- **Vehicle**: name, distance unit (`mi` or `km`), active flag.
- **Fill-up**: Store id, date, fuel grade (Regular, Mid-grade, Premium, Diesel), volume (Quantity), price per unit (Money), total (Money), filled-to-full flag, Vehicle id and odometer reading (both optional).
- **Recurring Charge**: payee, Category id, Fixed or Variable, amount (Money: the set price for Fixed, the last confirmed amount for Variable), schedule (kind: monthly day N, yearly month+day, every N weeks from a start date), start date, end date or none, and amount history (effective date, amount).
- **Pending Charge**: Recurring Charge id, due date, expected amount, status (waiting, confirmed, skipped), confirmed amount and date. Ids are deterministic from (Recurring Charge id, due date), so two devices generating the same period produce one record.
- **Receipt Photo**: Receipt id, Drive file id or none, local blob key, width, height, byte size, captured-at. The image bytes are never in the sync file (ADR 0001).

### Behaviour rules

- **Total check**: expected = sum of line prices + tax − receipt discount. Warn when it differs from the total by more than $0.01. Never block.
- **Spending by Category** (for a month and currency): each Receipt Line contributes its price plus a share of tax and receipt discount in proportion to its price. Whatever is left of the total (skipped lines, Draft Receipts) goes to Uncategorized. A Receipt with no lines contributes its total to its own Category. Refunds are negative. Fill-ups count as Gas. Confirmed Pending Charges count under their Recurring Charge's Category on their confirmed date. Waiting and skipped ones don't count.
- **Month** is the Receipt, Fill-up or confirmed Pending Charge date in local time.
- **Unit Price**: price paid ÷ (quantity × pack size), shown in the Product's Comparison Unit when linked, or else the Store Item's own unit. A stub with no pack size has no Unit Price, only price paid. Refunds are left out.
- **Comparison Unit**: linking a Store Item whose unit can't convert to the Product's Comparison Unit is refused with a reason.
- **SKU lookup**: by (Chain, SKU), with SKUs normalized by trimming spaces and leading zeros. No match creates a stub.
- **Fill-up arithmetic**: any two of volume, price per unit and total fill in the third, rounded to cents for total and to 3 decimals for price per unit (pumps show $3.459).
- **Fuel Economy**: for consecutive full Fill-ups A and B of a Vehicle that both have odometer readings, economy = (odometer B − odometer A) ÷ (volume of B plus every partial Fill-up between them). A missing odometer reading breaks the chain, and the chart shows a gap. Shown as MPG for `mi` vehicles and L/100 km for `km`, converting volume if the station used the other unit.
- **Pending Charge generation**: on app start and after sync, for every active Recurring Charge, create waiting Pending Charges for every due date from the start date (or the last one generated) up to today. Monthly day 29–31 falls on the last day of a shorter month. The expected amount is the amount effective on the due date.
- **Confirm all Fixed**: confirms every waiting Pending Charge from a Fixed Recurring Charge at its expected amount.
- **Merge** (Store Items, Products): follow Backpacking Tracker ADR 0009 and Recipe Tracker ticket 13. The tombstone records "merged into"; after sync or import, references to merged records are repointed, following chains. Store Items can only merge within one Chain.
- **Receipt Photo capture**: `<input type="file" accept="image/*" capture="environment">`, resized in JS to 1600 px on the long side, JPEG quality 0.8, stored locally first, uploaded to Drive by a queue when signed in and online.

### Build order

Slices are in `issues/`, each one usable on its own. 01–02 lay the foundation, 03–07 cover receipts, 08–09 cover prices, 10 is gas, 11 is recurring, 12–15 are charts and 16 is export.

## Testing Decisions

- Test external behaviour through the core's application API with an in-memory record store and a controllable clock, the same as Recipe Tracker. Don't test private helpers.
- Table-driven tests for the arithmetic: the total check, spending allocation of tax and discount, Unit Price and unit conversion, Fill-up any-two-of-three, Fuel Economy with partial Fill-ups and missing readings, and schedule dates (day 31 in February, leap years, every N weeks across year ends).
- Pending Charge generation tests cover catching up several missed periods, a change of amount mid-way, an end date, and two devices generating the same period.
- The sync engine and its tests come over from Recipe Tracker unchanged apart from names.
- UI is checked by hand, plus a headless browser smoke test for offline install as in Recipe Tracker ticket 01.

## Out of Scope

Not in v1, in rough order of what comes next: receipt extraction from the Receipt Photo (needs a proxy, ADR 0001), payment method on entries, budgets, year-over-year comparisons, a "nearby Store" default from location. Deferred with no plan yet: exchange-rate conversion (ADR 0002), bank or statement import, sharing with other people, countries other than the US and Canada.

## Further Notes

- Use a new Google OAuth client for Drive, not either sibling app's. All three apps are served from `rmcbain21.github.io`, and Drive's `appDataFolder` belongs to the OAuth client.
- Copy plumbing from Recipe Tracker (the newer of the two), not Backpacking Tracker, where they differ.
