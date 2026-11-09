# 07: Refunds

**What to build:** Recording a return. "Refund" on the Add Receipt form (or "Refund from this Receipt" on an existing one) creates a Receipt with the Refund flag and a negative total at a Store. It can have Receipt Lines pointing at the returned Store Items with negative prices, or no lines and its own Category. Refunds reduce spending in the month they happen and are never used for Unit Price or price history. The home list shows them clearly (for example, a green "−$40.00 Refund").

**Blocked by:** 06

**Status:** ready-for-agent

- [ ] Core: Refund flag, negative totals and line prices allowed only on Refunds; tests cover a Refund being left out of price history queries.
- [ ] "Refund from this Receipt" pre-fills the Store and lets me tick which lines were returned.

## Comments
