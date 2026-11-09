# 10: Fill-ups, Vehicles and Fuel Economy

**What to build:** Gas at the pump. A "Fill-up" button on the home screen opens a form with Store (last gas station used), fuel grade (default from the last Fill-up), and volume, price per unit and total, where entering any two fills in the third (total to cents, price per unit to 3 decimals). The volume unit and currency default from the Store's country. An optional Vehicle (default: the last used) has an odometer field, plus "filled to full", which is on by default. Settings → Vehicles allows adding, renaming and retiring Vehicles, each with a distance unit (mi or km). Fuel Economy is worked out between consecutive full Fill-ups with odometer readings, adding up the volume of partial Fill-ups between them, and shown on each Fill-up as MPG or L/100 km. After saving, offer "add a Receipt at this Store too". Fill-ups count as Gas spending. See the spec's behaviour rules.

**Blocked by:** 03

**Status:** ready-for-agent

- [ ] Core: Fill-ups, Vehicles, any-two-of-three arithmetic, Fuel Economy; table-driven tests cover partial fills, a missing odometer reading breaking the chain, an odometer lower than the last one (rejected with a reason), and a Canadian Fill-up in litres for a mi Vehicle.
- [ ] The Fill-up form saves with three numbers and one tap.
- [ ] Fill-ups appear in the home list beside Receipts, with volume and price per unit.
- [ ] New tables are added to the sync engine's table list.

## Comments
