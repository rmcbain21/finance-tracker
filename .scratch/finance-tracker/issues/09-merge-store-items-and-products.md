# 09: Merge Store Items and Products

**What to build:** Folding duplicates together. On a Store Item I can Merge it into another Store Item of the same Chain: every Receipt Line is repointed, and its SKU is kept as an extra SKU on the survivor so future lookups find it. On a Product I can Merge it into another Product: every linked Store Item is repointed. Merge follows Backpacking Tracker ADR 0009 and Recipe Tracker ticket 13: the merged record's tombstone records what it was merged into, and after every sync or JSON import, any reference still pointing at it is repointed, following chains. Merging Products with incompatible Comparison Units is refused.

**Blocked by:** 02, 08

**Status:** ready-for-agent

- [ ] Core: Merge for Store Items (same Chain only, extra SKUs) and Products, "merged into" on tombstones, repointing after sync and import; tests cover a chain (A→B, then B→C) and a Receipt Line added on another device before the sync.
- [ ] A record in an unsettled Sync Conflict can't be merged.
- [ ] Merge shows a preview ("14 lines on 9 Receipts will use *KS 2% Milk*") before applying.

## Comments
