# 04: Draft Receipts and Receipt Photos (on this device)

**What to build:** The checkout quick-capture. From the home screen, a big "Quick receipt" button opens a form with Store (last used), total and a camera button, and saves a Draft Receipt. The camera uses `<input type="file" accept="image/*" capture="environment">` (picking an existing image also works). The image is resized in JS to 1600 px on the long side at JPEG quality 0.8 and stored locally through a new photo store port (blobs in IndexedDB). A Receipt can have several Receipt Photos; tapping one opens it full screen. Draft Receipts are pinned at the top of the home list until finished; finishing means marking it not-Draft once it has a Category or lines. A Draft Receipt's total counts as Uncategorized. Drive upload of photos is ticket 05.

**Blocked by:** 03

**Status:** ready-for-agent

- [ ] Core: Draft flag, Receipt Photo records (Receipt id, local blob key, size, dimensions, captured-at); deleting a Receipt deletes its photos.
- [ ] Photo store port with an IndexedDB adapter and an in-memory fake for tests.
- [ ] Quick receipt saves in three taps plus typing the total; the photo is optional.
- [ ] Compression keeps a typical receipt photo under ~400 KB.
- [ ] Draft Receipts are pinned at the top of the home list with a count badge.

## Comments
