# 12: Chart: monthly spending by Category

**What to build:** A Spending page with stacked monthly bars by Category for the last 12 months, for one currency at a time (Home Currency by default, with a switch when other currencies have spending). It uses the spec's spending rules: line prices plus a proportional share of tax and receipt discount, the rest of a Receipt's total as Uncategorized, lineless Receipts under their own Category, Refunds negative, Fill-ups as Gas, confirmed Pending Charges under their Category. Each month shows how many Draft Receipts and waiting Pending Charges it has, so I know when a month isn't complete. Tapping a bar segment lists what's in it. Read the `dataviz` skill before writing the chart; use MudBlazor charts unless they can't do this, in which case note the alternative here.

**Blocked by:** 06, 07, 10, 11

**Status:** ready-for-agent

- [ ] Core: a spending query (month range, currency → per month, per Category totals, plus counts of Drafts and waiting Pending Charges); table-driven tests for the allocation of tax and discount, skipped lines, Drafts and Refunds.
- [ ] Totals for a month add up exactly to the sum of Receipt totals + Fill-up totals + confirmed Pending Charges in that currency.
- [ ] Works in light and dark mode and at phone width.

## Comments
