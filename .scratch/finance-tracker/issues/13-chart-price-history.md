# 13: Chart: price history

**What to build:** On a Store Item or a Product, a price history chart: Unit Price (in the Product's Comparison Unit, or the Store Item's own unit) over time, one line per Store, one currency at a time. Refunds are left out. Store Items with no pack size chart price paid instead, labelled as such. Below the chart: lowest, highest and most recent Unit Price, with Store and date. Read the `dataviz` skill first.

**Blocked by:** 08

**Status:** ready-for-agent

- [ ] Core: a price history query per Store Item and per Product (points: date, Store, Unit Price); tests cover several pack sizes linked to one Product and a Refund being left out.
- [ ] The chart is readable with a single data point and with several Stores.

## Comments
