# 08: Products, Comparison Units and Unit Prices

**What to build:** Comparing prices across pack sizes and Stores. Unit conversion within mass (g, kg, oz, lb) and volume (mL, L, fl oz, gal) lives in the core; `each` converts to nothing. A Receipt Line's Unit Price is price paid ÷ (quantity × pack size), shown with the line when its Store Item has a pack size. A Products page lets me create a Product and link Store Items to it from either side. The first Store Item linked sets the Product's Comparison Unit, which I can change; linking a Store Item whose unit can't convert is refused with a reason. Each Product shows its linked Store Items with each one's last Unit Price and Store, and the cheapest one is highlighted. See the spec's behaviour rules and ADR 0002.

**Blocked by:** 06

**Status:** ready-for-agent

- [ ] Core: unit conversion, Unit Price, Products, Comparison Unit, linking rules; table-driven tests for conversions and Unit Prices (4 L vs 1 gal, 18-pack eggs vs dozen, kg vs lb).
- [ ] A stub with no pack size has no Unit Price and can't be linked until it has one.
- [ ] Product page shows "cheapest recently" across Stores in the Comparison Unit, per currency (USD and CAD never mixed).

## Comments
