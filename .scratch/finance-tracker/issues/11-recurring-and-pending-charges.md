# 11: Recurring Charges and Pending Charges

**What to build:** Subscriptions and bills. A Recurring page lets me add a Recurring Charge: payee, Category (Subscriptions or Bills by default), amount and currency, Fixed or Variable, schedule (monthly on day N, yearly on a date, every N weeks from a start date), and start date. On app start and after every sync, the core generates waiting Pending Charges for every due date up to today, with ids derived from (Recurring Charge id, due date) so two devices never duplicate a period. Waiting Pending Charges show at the top of the home screen, oldest first: confirm (amount editable, pre-filled with the expected amount, which for Variable is the last confirmed amount) or skip. "Confirm all Fixed" confirms every waiting Fixed one. Changing the amount records a new amount from the next period on. Ending a Recurring Charge sets an end date and stops generation; it keeps its history. See ADR 0003.

**Blocked by:** 03

**Status:** ready-for-agent

- [ ] Core: Recurring Charges, schedules, amount history, generation, confirm, skip, Confirm all Fixed, end date; table-driven schedule tests (day 31 in February and in leap years, yearly on Feb 29, every 2 weeks across a year end).
- [ ] Tests cover catching up 3 missed months, an amount change mid-way, an end date, and two devices generating the same period (one record after sync).
- [ ] Confirmed Pending Charges count toward spending on their confirmed date under their Recurring Charge's Category; waiting and skipped ones don't.
- [ ] New tables are added to the sync engine's table list.

## Comments
