# Store Items belong to the Chain, not the Store

A Store is one location, and it belongs to a Chain. Store Items (SKUs) belong to the Chain, so the same SKU bought at two Walmart locations is one Store Item with one price history. Each Receipt Line still records which Store it was bought at, through its Receipt, so prices can be split by location when they differ.

## Considered Options

- **Store Items per Store location**: rejected. The same SKU at two locations would have two separate histories, and every SKU would be entered twice.
- **No locations, only Chains**: rejected. Country, currency and fuel unit belong to a location (a cross-border trip to a Canadian Walmart), and gas prices vary by station.

## Consequences

- Duplicate Store Items and Products are expected with SKU-first entry, so both can be merged. Merging leaves a tombstone so sync doesn't bring the merged record back (the pattern from Backpacking Tracker ADR 0009).
