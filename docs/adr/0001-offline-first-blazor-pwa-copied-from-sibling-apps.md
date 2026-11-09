# Offline-first Blazor PWA with no server, copied from the sibling apps

Finance Tracker is built the same way as Backpacking Tracker and Recipe Tracker (their ADR 0001). It is a Blazor WebAssembly PWA served as static files from GitHub Pages, keeps each device's data in IndexedDB, and syncs the phone and desktop through one data file in Google Drive's app-data folder. It is a separate app with its own data. Code and patterns are copied, not shared, and words such as Recipe Tracker's "Ingredient" are not reused for Products.

Receipt Photos do not go in the sync file. Each one is compressed on capture (about 1600 px wide, JPEG) and stored as its own file in the Drive app-data folder, referenced from its Receipt.

## Considered Options

- **A backend with its own database**: rejected; hosting, auth and maintenance cost for one user.
- **A small proxy (such as a Cloudflare Worker) for LLM receipt extraction**: deferred. A static PWA can't hold an API key, so automatic extraction waits for a later phase. In v1 the photo is attached and the Receipt Lines are typed in.

## Consequences

- Receipt extraction, when it comes, sits behind an interface and only pre-fills a draft Receipt for review. Adding a proxy for it would be the app's only server part and should revise this ADR.
- Photos sync separately from the data file, so a Receipt can arrive on a device before its photo does.
