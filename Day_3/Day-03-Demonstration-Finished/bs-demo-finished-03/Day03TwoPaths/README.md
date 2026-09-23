# Day 3 — Two Paths (State Loss vs. a Store)

A standalone Blazor Web App that runs the same three-step order wizard twice: once with
no store, once with a Fluxor store. Both paths collect the same three fields and let you
edit the **quantity** on every step. The only difference is where the data lives between
pages.

Built on the Day 2 checkout wizard (`Day-02/Code-Examples/demo/finished/Day02Demo`) —
same `NavigationManager` navigation, same Bootstrap card layout.

## Running it

```
dotnet run --project Day03TwoPaths.csproj
```

Then open the main menu at `/`. Two cards: **Path 1 — No Store** and **Path 2 — Flux Store**.

## Path 1 — No Store (`/classic/step1..3`)

There is nowhere to put an object between two Blazor pages, so `OrderDraft` is flattened
into the query string by `ToQueryString()` and rebuilt by `FromQuery()` on arrival. The
address bar is the transport. Each page keeps the user's edits in local fields, and **only
a wizard button writes them into the URL**.

Every page carries a *Where the data actually is* panel showing the two copies side by
side — the one in the URL and the one the user typed — and warns when they disagree.

## Path 2 — Flux Store (`/flux/step1..3`)

`OrderState` lives in the Fluxor store, registered once in `Program.cs`. The URLs carry
nothing. Each page injects `IState<OrderState>`, renders from it, and dispatches
`SetQuantityAction` and friends on every keystroke. No page holds a copy of the order.

## The demo sequence

Run the same five steps on each path and compare.

| Step | Path 1 (No Store) | Path 2 (Flux Store) |
|---|---|---|
| 1. Set quantity to **3** on step 1 | Total $74.97 | Total $74.97 |
| 2. Click the wizard **Next** button | URL becomes `?name=…&qty=3`; step 2 shows 3 | URL stays `/flux/step2`; step 2 shows 3 |
| 3. Change quantity to **5** on step 2 | Panel warns: URL still says 3, page says 5 | Store says 5 — there is no second copy |
| 4. Press the browser **Back** button | Step 1 shows **1** — neither edit survived | Step 1 shows **5** |
| 5. Press the browser **Forward** button | Step 2 shows **3** — the 5 is gone | Step 2 shows **5** |

Two distinct failures show up on Path 1, and it is worth naming both:

- **Drift** — Back and Forward replay an older URL, so each history entry holds its own
  frozen copy of the order. The data is not erased, it is *stale*, which is the
  cross-component synchronisation problem in miniature.
- **Loss** — an edit made but not yet passed through a wizard button never reached the URL
  at all, so no history entry has it and nothing can bring it back.

## The one limit on Path 2

The store lives in server memory for the current Blazor circuit. Back and Forward are safe;
**F5 resets the store to its initial values** (verified, not assumed). This is stated on the
student-facing panel on every `/flux` page — it is the honest boundary of what a store buys
you, and the hook into persistence later in the course.

## Layout

```
Models/OrderDraft.cs                    the object Path 1 hand-carries
Features/Order/Store/                   OrderState, Actions, Reducers, Feature
Components/Pages/Home.razor             main menu — two cards
Components/Pages/Classic/               ClassicStep1..3
Components/Pages/Flux/                  FluxStep1..3
Components/Shared/UrlTransportPanel.razor     Path 1 explanation panel
Components/Shared/StoreTransportPanel.razor   Path 2 explanation panel
```

This project sits beside `Day03Demo` in `Code-Examples/demo/finished/`, so it ships inside
`Day-03-Demonstration-Finished.zip` and reaches both students and the instructor. It is
**reference only** — Activity 1 uses `Day03Demo`, and the Fluxor code here is taught on Day 4.
