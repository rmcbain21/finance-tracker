# 06: Receipt Lines and Store Items

**What to build:** Filling in a Receipt line by line. On a Receipt, "Add line" asks for SKU, quantity (default 1) and price paid. A SKU is looked up by (Chain, normalized SKU: trimmed, leading zeros dropped). A match fills in the Store Item's receipt name and default Category; no match creates a Store Item stub holding only the SKU. Lines can be edited, reordered and deleted, and each has a Category defaulted from its Store Item. The Receipt also gets optional tax and receipt-wide discount. A total check compares lines + tax − discount with the total and warns (never blocks) when they differ by more than $0.01. A Store Items page (per Chain) lists stubs first, so I can fill in receipt name, pack size (Quantity with unit) and default Category later. Once a Receipt has lines, its own Category is no longer used. See ADR 0004.

**Blocked by:** 03

**Status:** ready-for-agent

- [ ] Core: Receipt Lines nested in the Receipt, Store Items per Chain, SKU normalization and lookup, stubs, tax and discount, total check; tests cover a stub created on first use and found on the next Receipt at a different Store of the same Chain.
- [ ] A `Quantity` value type (decimal + unit code from the spec's unit list), with `oz` meaning weight.
- [ ] Entering a line works with the keyboard alone (SKU → Enter → price → Enter → next line).
- [ ] The total check shows the difference ("$4.12 not in lines") without blocking save.
- [ ] Store Items page with a "stubs only" filter.

## Comments
