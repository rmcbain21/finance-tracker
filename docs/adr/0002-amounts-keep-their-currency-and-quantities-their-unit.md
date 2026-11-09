# Amounts keep their currency and quantities keep their unit

Every amount is stored with the currency it was paid in, and every quantity (fuel volume, pack size) is stored as entered with its unit. A Store's country decides the currency and the default fuel unit. The app reports in a Home Currency (USD) and does no exchange-rate conversion: spending in another currency (such as CAD on a cross-border trip) is shown separately or left out of totals. Units are converted only when Unit Prices are compared, so a 4 L jug and a 1 gal jug can still be compared.

## Considered Options

- **Normalize everything to one unit system, as Backpacking Tracker does (metric)**: rejected. Receipts and pumps state their own units, and showing the number from the receipt matters more than easy totals.
- **Convert foreign amounts to the Home Currency at the rate on the day**: deferred. It needs a rate source, and a converted number never matches the card statement.

## Consequences

- Charts that add up money must group by currency or filter to one currency.
- Don't "fix" stored litres to gallons or CAD to USD.
