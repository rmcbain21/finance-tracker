# Pending Charges wait for the Owner to confirm, even fixed ones

A Recurring Charge never records spending by itself. Each period it creates a Pending Charge that waits until the Owner confirms or skips it, even when the amount is fixed. Missed periods pile up oldest first, and a single "confirm all fixed" action clears the routine ones. Changing a Recurring Charge's amount applies only from the next period on, so confirmed charges show its real price history.

## Considered Options

- **Auto-confirm fixed charges on their date**: rejected. A subscription cancelled without updating the app would keep quietly adding spending that never happened.
- **Reminders only, with every charge entered by hand**: rejected. It's too much typing for charges that rarely change.

## Consequences

- Monthly totals can be incomplete while Pending Charges are waiting, so charts should show how many are waiting.
