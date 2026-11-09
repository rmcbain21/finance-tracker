# 16: CSV export

**What to build:** Settings → Export gains "CSV" beside the JSON backup. It downloads a zip of CSV files: `receipts.csv` (one row per Receipt: date, Chain, Store, country, currency, total, tax, discount, Draft, Refund, Category if lineless), `receipt-lines.csv` (Receipt id, date, Store, SKU, receipt name, Product, quantity, pack size, unit, price paid, Unit Price, Category), `fill-ups.csv` and `recurring-charges.csv` (confirmed Pending Charges only). Dates are ISO, amounts use a period decimal separator with the currency code in its own column, and every row has the record's id so it can be joined.

**Blocked by:** 06, 07, 10, 11

**Status:** ready-for-agent

- [ ] Core: CSV writers with proper quoting (commas and quotes in receipt names); tests cover quoting and a Refund row.
- [ ] Opens cleanly in Excel and Google Sheets.

## Comments
