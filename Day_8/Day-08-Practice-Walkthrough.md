# Practice Walkthrough -- Day 8: State Persistence Patterns

**What this is:** A worked-launch companion for your Day 8 practice. Practice 1 opens with a worked launch -- the instructor walks the first step on screen as one worked example -- then continues as independent work; the steps here give direction without giving away every line. The classification activity is a no-code challenge with a partner.

Day 8 has one coding practice and one no-code partner activity -- this is the intended shape, not a partial list.

**Your project:** Open the `Day08Practice` starter project in Visual Studio. The project has a working **Recently Viewed** feature. The Shop page shows six products, each with a "View" button. A store keeps the five products you viewed most recently, newest first, with no duplicates. That feature works in memory right now -- view a few products and the "Recently Viewed" list updates. But refresh the page and the list is empty again. **Your practice makes that list survive a refresh**, using the same persistence-and-hydration pattern you watched the instructor build for the cart in the demonstration.

**How to find what needs changing:** Search for `TODO` in the project (Ctrl+Shift+F in Visual Studio). Every place you need to write code is marked with a `TODO` comment.

---

## Background: What's pre-built (review, do not edit)

The **Recently Viewed feature itself** is already wired so you can focus on persistence:

- `Features/RecentlyViewed/Store/RecentlyViewedState.cs` -- holds `IReadOnlyList<RecentlyViewedItem> Items`.
- `RecentlyViewedActions.cs` -- `ViewProductAction(RecentlyViewedItem Item)` is dispatched by the Shop page when you click "View".
- `RecentlyViewedReducers.cs` -- `ReduceViewProductAction` prepends the viewed product, removes any earlier duplicate, and caps the list at five. It is a pure function (no storage).
- `Components/Pages/Shop.razor` -- the "View" buttons and the "Recently Viewed" display list are built for you.
- `Components/Layout/MainLayout.razor` -- the navbar "Recently viewed" count and its manual `StateChanged` subscription are built for you.

Run the app, open Shop, and click "View" on a few products: the list updates live. Now refresh -- the list empties. Nothing saves it yet. **That is exactly the Day 7 → Day 8 problem, on a new feature.** Fixing it is Practice 1.

What is **not** built yet is the persistence layer. `RecentlyViewedEffects.cs` ships as a shell -- it has the `ILocalStorageService` and `IState<RecentlyViewedState>` injected and the storage-key constant defined, but no effect methods. You write them.

---

## Practice 1: Persist and Rehydrate "Recently Viewed" (35 min)

> **How this practice runs.** This is a *worked launch*, then independent work. You already saw the **whole** persistence-and-hydration pattern built on screen in the demonstration -- but on the cart, not on this feature. So this practice is **transfer**, not reproduction: you apply a pattern you have seen to a feature you have not wired. Your instructor runs the first step as the worked launch -- they write the **save effect** (`HandleViewProductAction`) on screen so the shape is anchored. After that you continue **on your own**: the two hydration actions, the hydration reducer, the hydration effect with try/catch, and the `OnAfterRenderAsync(firstRender)` trigger in `MainLayout.razor`. Step 1 below is the worked launch; Steps 2-5 are independent.

**This is the same persistence-and-hydration pattern the demonstration just built for the cart -- now you apply it to a Recently Viewed list.** The demo saved the cart on every change and restored it on startup. You do the same two things for Recently Viewed: save the list whenever it changes, and load it back when the app starts.

Refresh-resistance is a UX expectation. Real e-commerce sites remember what you were looking at. "Recently viewed" is a low-stakes, high-value example: losing it is not a disaster, but keeping it makes the app feel like it remembers you. The same pattern shows up in carts, form drafts, paused videos, and dozens of other situations where the app should remember the user's place.

The pattern has two halves that run at different times. **Save** runs on *every* state change -- an effect mirrors `recentState.Value.Items` to localStorage after each view. **Hydrate** runs *once* on startup -- an effect reads localStorage, dispatches `HydrateRecentlyViewedAction` if there is data, and the reducer restores state. The startup trigger fires from `OnAfterRenderAsync(firstRender)` in `MainLayout`, not `OnInitialized`, because JavaScript interop is not available until after first render.

You will write the **whole** pattern for this feature -- save effect, the two hydration actions, the hydration reducer, the hydration effect with try/catch, and the MainLayout trigger -- because you have seen it demonstrated only on the cart. The feature's own store (state, the `ViewProductAction` and its reducer) and the UI are pre-built.

You will know it works when viewing products and refreshing restores the list, a first visit with no data starts empty, and manually corrupting the JSON in DevTools causes a silent reset instead of a crash.

Four terms underpin the rest of this Practice. You met all four in the demonstration; the definitions are repeated here so the card is self-contained. Read them first; the Practice steps then put each term to work in order.

You need to know what **localStorage** is. localStorage is a small key-value store the browser keeps on disk for each website. Data written to localStorage survives when the user closes the tab, closes the browser, or restarts the machine. It is not the same as cookies -- it is bigger, and the browser does not send it to the server with every request. You read and write to it with JavaScript or, in Blazor, through a library that calls JavaScript for you.

You also need to know what **Blazored.LocalStorage** is. Blazored.LocalStorage is a NuGet package that wraps the browser's localStorage in a C#-friendly interface. The starter project already has it installed and registered in `Program.cs`. You inject `ILocalStorageService` into an effect or component, then call `SetItemAsync`, `GetItemAsync<T>`, or `RemoveItemAsync`. The library handles the JavaScript interop call for you.

You also need to know what a **circuit** is. In Blazor Server, a **circuit** is the live connection between the browser and the server. The server holds the state of the page; the browser sends events through the circuit and receives UI updates back. The circuit does not exist until the page has rendered at least once. This is why hydration runs in `OnAfterRenderAsync(firstRender)`, not `OnInitialized`.

You also need to know how `OnAfterRenderAsync(firstRender)` differs from `OnInitialized`. Because of prerendering, your component is created **twice**: once on the server for the prerender pass, and again once the circuit is live. **`OnInitialized`** runs in *both* of those passes -- and in the first one there is no browser yet, so JavaScript interop calls, like reading from localStorage, will fail there. **`OnAfterRenderAsync(firstRender)`** runs only after the component has actually rendered in the browser, which means the circuit is up and interop is safe. The `firstRender` parameter is `true` only on the very first render; the method runs again on every later render, so checking `firstRender` lets you run startup code exactly once. This is the right place to load saved state.

### What's already built

- The full Recently Viewed store (state, `ViewProductAction`, `ReduceViewProductAction` with prepend/dedupe/cap-at-5)
- The Shop page "View" buttons and the "Recently Viewed" display list
- The navbar count and its `StateChanged` subscription in `MainLayout.razor`
- `RecentlyViewedEffects.cs` shell: DI wiring + the `RecentlyViewedStorageKey` constant
- Blazored.LocalStorage installed and registered in `Program.cs`

### What you need to add

Five pieces -- the instructor walks the first as a worked launch, then you complete the rest independently.

**Worked launch (the instructor walks this one):**

1. **The save effect** in `RecentlyViewedEffects.cs` -- an `[EffectMethod]` for `ViewProductAction` that calls `localStorage.SetItemAsync(RecentlyViewedStorageKey, recentState.Value.Items)`. Save the list from state, not `action.Item` -- the effect runs after the reducer, so `recentState.Value.Items` is already the updated, capped list (the **Effect Timing Rule** from the demo).

**Independent remainder (on your own):**

2. **Two hydration action records** in `RecentlyViewedActions.cs` -- `HydrateRecentlyViewedRequestAction` (the trigger, carries no data) and `HydrateRecentlyViewedAction(IReadOnlyList<RecentlyViewedItem> Items)` (carries the loaded items). Same trigger-then-result shape the cart used.

3. **A reducer for `HydrateRecentlyViewedAction`** in `RecentlyViewedReducers.cs` -- sets `Items` to the items from the action. Use the `state with { }` pattern.

4. **A hydration effect** in `RecentlyViewedEffects.cs` -- responds to `HydrateRecentlyViewedRequestAction`. It should:
   - Try to load `List<RecentlyViewedItem>` from localStorage using the storage key
   - Check the loaded data is not null and has items
   - If data exists, dispatch `HydrateRecentlyViewedAction` with the loaded items
   - If `GetItemAsync` throws (corrupt data), remove the key and let the list stay empty

5. **The startup wiring** in `Components/Layout/MainLayout.razor` -- override `OnAfterRenderAsync(bool firstRender)` and dispatch `HydrateRecentlyViewedRequestAction` when `firstRender` is true.

### Hints

- The storage key constant `RecentlyViewedStorageKey` is already defined in `RecentlyViewedEffects` -- use it
- `GetItemAsync<T>(key)` returns `null` if the key doesn't exist (first visit)
- Wrap the load in `try/catch` -- if the JSON in localStorage doesn't match `List<RecentlyViewedItem>`, deserialization throws
- In the catch block, call `RemoveItemAsync` to clean up the corrupt data
- The effect method signatures follow the demo's pattern: `public async Task HandleViewProductAction(ViewProductAction action, IDispatcher dispatcher)` and `public async Task HandleHydrateRecentlyViewedRequestAction(HydrateRecentlyViewedRequestAction action, IDispatcher dispatcher)`
- Hydration must run after the circuit exists -- that is why the dispatch goes in `OnAfterRenderAsync(firstRender)`, not `OnInitialized`
- Use `IReadOnlyList<RecentlyViewedItem>` for the `Items` parameter -- it matches `RecentlyViewedState.Items`'s declared type. You load into `List<RecentlyViewedItem>` because `GetItemAsync<T>` needs a concrete type to deserialize into; the action then carries that list as `IReadOnlyList<RecentlyViewedItem>`.

### Success criteria

- View 3 products, refresh page -- the list is restored, newest first
- View a 4th, 5th, and 6th product -- the list stays capped at 5, and refreshing keeps that capped list
- First visit (no localStorage data) -- app starts with an empty list, no errors
- Manually corrupt the localStorage data (edit the JSON in DevTools to invalid text) -- app starts with an empty list, no crash. In DevTools the key is `recentlyViewed`, not `cart` -- each feature uses its own storage key.
- Step 5 is the one piece you **cannot** verify by running the app -- both placements look identical in the browser. Read your `MainLayout` against the reference card before you call Practice 1 done.

### Troubleshooting

| Symptom | Likely Cause | Fix |
|---------|-------------|-----|
| List not restored after refresh | Save effect missing `[EffectMethod]`, or the hydration trigger not dispatched | Add `[EffectMethod]`; ensure `MainLayout` dispatches `HydrateRecentlyViewedRequestAction` on first render |
| `NullReferenceException` during hydration | Dispatching `HydrateRecentlyViewedAction` without checking for null | Add `if (items is not null && items.Count > 0)` before dispatching |
| App crashes on refresh with corrupt data | Missing try/catch around `GetItemAsync` | Wrap the load call in try/catch; remove corrupt key in catch |
| Saved list is always one view behind | Saving `action.Item` instead of `recentState.Value.Items` | Save the list from state -- the effect runs after the reducer, so state is already updated |
| *No visible symptom -- the app appears to work* | Hydration dispatched from `OnInitialized` instead of `OnAfterRenderAsync(firstRender)` | The prerender dispatch is silently discarded, and `OnInitialized`'s second pass happens to succeed -- so it is right by accident. Move the dispatch to `OnAfterRenderAsync(firstRender)`. *(Calling `ILocalStorageService` **directly** from `OnInitialized` is the case that throws `InvalidOperationException`.)* |

---

## Persistence Strategy Classification -- Pair Work (15 min)

Real apps make persistence decisions constantly — and making the wrong persistence decision has real consequences. Storing a credit card in localStorage will eventually cause a security breach; *not* storing a multi-step form's progress will eventually frustrate the user. Every piece of data has a "right" storage tier based on three questions: what does the user expect, what happens if this data is exposed, and whether anything outside the component needs it at all.

The key insight here: developers reach for localStorage by default because it is the easiest API, but the default choice is often wrong. The decision framework is three questions deep — *lifetime* (should this survive browser close?), *sensitivity* (would an XSS reader make this a breach?), and *scope* (does anything outside this one component need it?). Once you can answer all three, the persistence tier picks itself.

**The demo defaulted cart items to localStorage** — and that was the right call: low sensitivity, the user expects the cart to survive a refresh. The same code shape applied to credit card numbers would be a serious vulnerability. This activity asks you to make the same kind of judgment for eight more scenarios, including ones with conflicting signals.

You'll know it worked when you and your partner can defend each classification with a one-sentence justification that names the *real* reason. If you disagree on a scenario, one of you is missing a constraint the other sees — and that disagreement is where the learning happens.

No coding. Work with a partner to classify 8 data scenarios.

Two new terms appear in the instructions and decision framework below. Read these definitions before you start.

You need to know what **sessionStorage** is. sessionStorage works like localStorage, with one difference. localStorage data survives until the user clears it or the website removes it. sessionStorage data is cleared the moment the tab closes. Both use the same kind of key-value API. The difference is **lifetime**: localStorage is "remember this until I say otherwise," sessionStorage is "remember this just for this visit."

You also need to know what an **XSS attack** is. **XSS** stands for **cross-site scripting**. An XSS attack is a security flaw where an attacker manages to run their own JavaScript inside your website's pages — usually because the site renders user input without escaping it correctly. If an attacker's script runs on your page, anything that script can read becomes leaked data. The browser does not protect localStorage or sessionStorage from a script running on the same page. So the "what if XSS reads this" question in the decision framework below is the same question as "if my site has a script-injection bug, is the leak a security incident or an inconvenience?"

### Instructions

For each scenario below, decide:
- **localStorage** -- data should persist across sessions (survives browser close)
- **sessionStorage** -- data should persist within the session only (cleared on tab close)
- **Do not persist** -- data should not be stored in browser storage at all

**The answer alone is not enough.** Write one sentence justifying each choice. The "why" matters more than the "what."

### Scenarios

| # | Scenario | Your Answer | Your Justification |
|---|----------|-------------|-------------------|
| 1 | Shopping cart items | | |
| 2 | User's preferred language (en/fr) | | |
| 3 | OAuth access token | | |
| 4 | "Show advanced options" UI toggle | | |
| 5 | Search filter settings | | |
| 6 | Credit card number | | |
| 7 | Multi-step form progress | | |
| 8 | Dark/light theme preference | | |

### Decision framework

Ask yourself three questions for each scenario:
1. **What does the user expect?** If they close the browser and come back tomorrow, should this data still be there?
2. **What happens if this data is exposed?** If an XSS attack reads this from storage, is it a security breach or just an inconvenience?
3. **Does anything outside this one component need this value?** If only one component reads it, it is component state -- keep it there and do not persist it. Persisting it makes the app remember something the user never asked it to remember.

---

## What You Learned

After completing this practice, you can:

- **Persist state on change** with an effect that mirrors Fluxor state to localStorage after every update (Effect Timing Rule -- effects run after reducers, so you save the already-updated list)
- **Restore state on startup** with a hydration action/reducer/effect pattern triggered from `OnAfterRenderAsync(firstRender)`
- **Transfer a known pattern to a new feature** -- you applied the cart's persistence-and-hydration shape to Recently Viewed from scratch
- **Handle edge cases** -- first visit (null data), corrupt data (try/catch), cleanup (RemoveItemAsync)
- **Choose the right persistence strategy** for different types of data based on lifetime, sensitivity, and scope

### Next day preview

Day 9 covers **advanced routing patterns** -- fallback routes for 404 handling and catch-all parameters. Then **Lab 1 is formally assigned** in the last 30 minutes. This is the final Module 1 instruction day before review and lab work begins.
