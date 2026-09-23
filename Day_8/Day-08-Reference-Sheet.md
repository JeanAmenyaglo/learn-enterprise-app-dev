# Day 8 Reference Sheet -- State Persistence Patterns

**Course:** SDEV 2351 | **Module:** 1 | **CO:** CO2
**Estimated Reading Time:** ~5 minutes

---

> **Companion build guide.** Today you save the cart to the browser and load it back on startup. The **"Make it remember — saving the cart (Day 8)"** part of <strong><a href="Fluxor-Store-Anatomy.html?isCourseFile=true" target="_blank" rel="noopener">Anatomy of a Fluxor Store</a></strong> walks this exact pattern — the persistence effect that writes to localStorage, and the `OnAfterRenderAsync(firstRender)` trigger that hydrates the store (not `OnInitialized`).

## Key Concepts

- **SPA state is volatile** -- Fluxor state lives in memory. Page refresh resets the store to its initial state. Persistence saves state to the browser so it survives refreshes.
- **localStorage vs. sessionStorage** -- localStorage persists until explicitly cleared (survives browser close). sessionStorage is cleared when the tab closes. Choose based on data lifetime needs.
- **Blazored.LocalStorage** -- a NuGet package that provides `ILocalStorageService` via DI. Handles JSON serialization automatically. No JavaScript required.
- **Hydrate / rehydrate** -- refill an empty store with previously saved data. On app startup, read saved state from storage and dispatch a hydration action to restore the Fluxor store (restoring saved state on startup). You'll meet the same word in industry tools like redux-persist and in Blazor/React server-side rendering.

## Syntax Reference

### Blazored.LocalStorage API

| Method | Purpose | Example |
|--------|---------|---------|
| `SetItemAsync<T>(key, value)` | Save data to localStorage | `await localStorage.SetItemAsync("cart", items)` |
| `GetItemAsync<T>(key)` | Load data from localStorage | `var items = await localStorage.GetItemAsync<List<CartItem>>("cart")` |
| `RemoveItemAsync(key)` | Delete data from localStorage | `await localStorage.RemoveItemAsync("cart")` |
| `ISessionStorageService` | Same three methods, from the separate `Blazored.SessionStorage` package | Nothing on Day 8 uses it -- this is what "choose sessionStorage" means in code |

### localStorage vs. sessionStorage

| Feature | localStorage | sessionStorage |
|---------|-------------|----------------|
| Lifetime | Until explicitly cleared | Until tab closes |
| Scope | All tabs, same origin | Single tab |
| Use for | Cart, preferences, filters | Session tokens, temp data |
| Never store | Passwords, API keys, credit cards | Passwords, API keys, credit cards |

## Code Pattern

*This is the cart pattern from the demonstration. In Practice 1 you apply the **same** save-and-hydrate shape to a different feature -- a "Recently Viewed" list -- so read this as the template, then transfer it.*

**Renaming map -- transfer the cart pattern to your feature:**

| Demo (cart) | Your practice (Recently Viewed) |
|---|---|
| `CartState` / `CartItem` | `RecentlyViewedState` / `RecentlyViewedItem` |
| `"cart"` (storage key) | `"recentlyViewed"` |
| `CartPersistenceEffects` | `RecentlyViewedEffects` |
| `HandleAddToCartAction` | `HandleViewProductAction` (save effect) |
| `HydrateCartRequestAction` | `HydrateRecentlyViewedRequestAction` |
| `HydrateCartAction` | `HydrateRecentlyViewedAction` |

The code block below stays cart-named -- the table is your translation key. You still write the shape yourself.

```csharp
// Persistence effect -- save cart after every change
public class CartPersistenceEffects(
    ILocalStorageService localStorage,
    IState<CartState> cartState)
{
    [EffectMethod]
    public async Task HandleAddToCartAction(AddToCartAction action, IDispatcher dispatcher)
    {
        // Effect runs AFTER reducer -- state is already updated
        await localStorage.SetItemAsync("cart", cartState.Value.Items);
    }

    // Rehydration -- restore state from storage on startup
    [EffectMethod]
    public async Task HandleHydrateCartRequestAction(
        HydrateCartRequestAction action,
        IDispatcher dispatcher)
    {
        try
        {
            var items = await localStorage.GetItemAsync<List<CartItem>>("cart");
            if (items is not null && items.Count > 0)
                dispatcher.Dispatch(new HydrateCartAction(items));
        }
        catch
        {
            await localStorage.RemoveItemAsync("cart"); // Handle corrupt data
        }
    }
}
```

**Component (trigger hydration on startup):** dispatch the rehydrate action once from `MainLayout`'s `OnAfterRenderAsync(firstRender)` so the saved cart restores as soon as the app can reach localStorage.
```razor
@inherits LayoutComponentBase
@implements IDisposable
@inject IState<CartState> CartState
@inject IDispatcher Dispatcher

@code {
    // Pre-built plumbing: a layout (LayoutComponentBase) does NOT auto-subscribe to Fluxor
    // state the way a FluxorComponent does, so subscribe manually to keep the navbar cart
    // count live as CartState changes (Add/Remove/Clear and the hydrate below).
    protected override void OnInitialized() =>
        CartState.StateChanged += OnCartStateChanged;

    private void OnCartStateChanged(object? sender, EventArgs e) =>
        InvokeAsync(StateHasChanged);

    public void Dispose() =>
        CartState.StateChanged -= OnCartStateChanged;

    // The day's NEW step -- trigger hydration once, after the circuit exists
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Dispatcher.Dispatch(new HydrateCartRequestAction());
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}
```
Use `OnAfterRenderAsync(firstRender)`, **not** `OnInitialized`. With prerendering, a component is created **twice** -- once on the server for the prerender pass, where there is no browser (so no JS interop and no localStorage), and again once the circuit is live. `OnInitialized` runs in **both** passes; `OnAfterRenderAsync` runs only in the live one, and `firstRender` makes your startup code fire exactly once. Reading localStorage **directly** from `OnInitialized` throws `InvalidOperationException` on the prerender pass.

Why the manual `StateChanged` subscription? A layout must inherit `LayoutComponentBase` (for `@Body`), so it can't also inherit `FluxorComponent` -- which means it won't auto-re-render when the store changes. Without the subscription the navbar count would stay frozen until the next navigation. See **Blazor Gotchas Section 3.5**.

## Common Mistakes

| Mistake | Symptom | Fix |
|---------|---------|-----|
| Saving the action's single item instead of the full state | Only the last added item appears after refresh | Save `cartState.Value.Items` (the full list), not `action.Item` |
| Not handling null from `GetItemAsync` on first visit | `NullReferenceException` during hydration | Check `items is not null` before dispatching |
| Missing try/catch around `GetItemAsync` | App crashes if localStorage data is corrupt | Wrap in try/catch; remove corrupt key on failure |
| Persisting sensitive data (passwords, tokens) | Security vulnerability via XSS | Never store credentials in browser storage |

## Quick Check

1. **A shopping cart should survive closing the browser. localStorage or sessionStorage?**
   <details><summary>Answer</summary>localStorage -- persists until explicitly cleared, survives browser close.</details>

2. **When does a Fluxor effect run relative to the reducer?**
   <details><summary>Answer</summary>After the reducer. So <code>IState&lt;T&gt;.Value</code> in an effect reflects the already-updated state.</details>

3. **Why wrap `GetItemAsync` in try/catch during rehydration?**
   <details><summary>Answer</summary>If the stored JSON is corrupt or doesn't match the expected type, deserialization throws an exception. The catch block removes the corrupt key and lets the app start with fresh state.</details>

## Watch a cart survive a browser refresh in action

**Does Fluxor state survive a page refresh?** A 16-second screen recording of the Day 8 demo — add three items to the cart (effects write each to localStorage), hit refresh to destroy the Blazor circuit, and watch the hydration effect re-read localStorage and dispatch HydrateCartAction so the cart restores to 3 items without a rebuild. Persistence is added through an effect, not a built-in Fluxor feature.

<video controls preload="metadata" width="720" style="max-width:100%;height:auto;border:1px solid #DEE2E6;">
  <source src="../demo-videos/day-08-cart-persistence.mp4" type="video/mp4" />
  <track kind="captions" src="../demo-videos/day-08-cart-persistence.vtt" srclang="en" label="English" default />
  Your browser does not support embedded video. <a href="../demo-videos/day-08-cart-persistence.mp4">Download the MP4 (16 s, silent, captioned)</a>.
</video>

- Video file: [`day-08-cart-persistence.mp4`](../demo-videos/day-08-cart-persistence.mp4) (16 s, silent, 1280×720, H.264)
- Caption track: [`day-08-cart-persistence.vtt`](../demo-videos/day-08-cart-persistence.vtt) (WebVTT sidecar — WCAG 1.2.2)
- Full text transcript (screen-reader friendly): [`day-08-cart-persistence-transcript.md`](../demo-videos/day-08-cart-persistence-transcript.md) (WCAG 1.2.1)

The video has **no audio**. All narration is in the on-screen captions and the transcript. Nothing in Day 8 assessments requires watching the video — the transcript is a complete alternative.

---

*See also: [MS Learn: Blazor State Management](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/) | [Fluxor Docs](https://github.com/mrpmorris/Fluxor/blob/master/Docs/README.md) | Day 8 demonstration + practice in Code-Examples/*
