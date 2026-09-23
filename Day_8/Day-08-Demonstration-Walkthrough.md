# Demo Walkthrough -- Day 8: State Persistence Patterns -- Cart Persistence with Blazored.LocalStorage

**Purpose:** A comprehensive, step-by-step reference for the Day 8 demo. Use this to prepare before class or to follow along while coding.

**Starting point:** The `Code-Examples/demo/starter/Day08Demo` project -- a Blazor Web App with the full Day 7 cart store (CartState, actions, reducers, feature, effects), checkout guard, and Blazored.LocalStorage installed and registered. `CartPersistenceEffects.cs` exists with DI wiring but no effect methods. Hydration actions and reducer do not exist yet.

**End state:** A working cart that persists to browser localStorage on every change and rehydrates on app startup. Add items, refresh, cart is restored. Clear cart, localStorage key is removed. Corrupt localStorage data is handled gracefully.

**During class:** Open the starter project and follow along — the instructor codes each step live. Your role is to observe and match each step to the TODO markers in the starter. You do not type the effects yourself during the demo; that is the job of Practice 1.

---

## Step 1: Open the Starter Project and Show What's Pre-Built (3 min)

Open `Code-Examples/demo/starter/Day08Demo` in Visual Studio. Press F5 to run.

**What you'll see:**
- Navbar reads "Day 8 Demonstration"
- Shop page with 6 products and Add to Cart buttons
- Cart counter in the navbar updates as you add items
- Navigate to Checkout -- guard passes, items display
- **Refresh the page** -- cart is empty, guard redirects to Shop

**Before coding, open each file briefly to see the pre-built infrastructure:**

1. **`Program.cs`** -- `builder.Services.AddBlazoredLocalStorage()`. The Blazored.LocalStorage package is already installed and registered, so `ILocalStorageService` is available via DI.

2. **`Features/Cart/Store/CartPersistenceEffects.cs`** -- A class with constructor DI already injects `ILocalStorageService` and `IState<CartState>`. The constructor wiring is done -- the task in this demo is to add the effect methods that actually save and load data.

3. **`Features/Cart/Store/CartActions.cs`** -- Three existing actions: Add, Remove, Clear -- the actions that change cart state. Hydration actions get added shortly.

4. **`Features/Cart/Store/CartReducers.cs`** -- Three existing reducers handle state transitions for Add, Remove, and Clear. A hydration reducer gets added later in the demo.

5. **`Components/Pages/Checkout.razor`** -- Contains the Day 7 guard in `OnInitialized`. If the cart is empty, it redirects to Shop. Once the cart is persisted, this guard passes when you reach Checkout by navigating from Shop -- the cart is already restored by then. Refreshing while you are *on* Checkout is a different case, covered at the end of the demonstration.

   > **Recall from Day 6 -- Result\<T\> in the Cart Store:** The cart effects use the `Result<T>` pattern (BYSResult) from Day 6. `MockCartService` returns `Result<List<CartItem>>` instead of throwing exceptions. Effects check `.IsSuccess` before dispatching. Today's persistence effects follow the same effect pattern but interact with `ILocalStorageService` instead of a mock service. The `Result<T>` pattern returns in Module 2 (Day 20+) and Module 3 (Day 29+).

> **Why start here:** The concrete problem is visible first -- refresh kills state, guard fails. The demo solves this problem step by step. The infrastructure (NuGet package, DI registration, class shell) is pre-built so class time focuses on the persistence pattern, not setup.

---

## Step 2: Add Persistence Effects to CartPersistenceEffects.cs (4 min)

**Action:** Open `Features/Cart/Store/CartPersistenceEffects.cs` and add three effect methods inside the class body — the `CartStorageKey` constant is already defined above the comment block.

Replace the comment block with:

```csharp
[EffectMethod]
public async Task HandleAddToCartAction(AddToCartAction action, IDispatcher dispatcher)
{
    // Effect runs after reducer -- state is already updated
    await localStorage.SetItemAsync(CartStorageKey, cartState.Value.Items);
}

[EffectMethod]
public async Task HandleRemoveFromCartAction(RemoveFromCartAction action, IDispatcher dispatcher)
{
    await localStorage.SetItemAsync(CartStorageKey, cartState.Value.Items);
}

[EffectMethod]
public async Task HandleClearCartAction(ClearCartAction action, IDispatcher dispatcher)
{
    await localStorage.RemoveItemAsync(CartStorageKey);
}
```

**Why three effects:**

> Three effects, one per cart-changing action. When an item is added or removed, the effect saves the entire cart list to localStorage. When the cart is cleared, the key is removed entirely.
>
> Note that the code saves `cartState.Value.Items` -- the full list from the Fluxor state -- not `action.Item` from the action. The action carries one item. The state carries the complete, updated list.
>
> The key insight is the **Effect Timing Rule**: Fluxor effects run AFTER reducers. By the time this code executes, the reducer has already updated the state. So `cartState.Value.Items` reflects the new cart, not the old one.
>
> A constant `CartStorageKey` is used for the storage key. In a real app, this avoids typos -- saving with `'cart'` and loading with `'Cart'` would fail because localStorage keys are case-sensitive.

**FAQ -- "Why not save in the reducer instead of an effect?"**

> Reducers must be pure -- same inputs, same outputs, no side effects. Saving to localStorage is a side effect (it talks to the browser). Effects are the designated place for side effects in Fluxor. This separation is fundamental to the Flux pattern.

---

## Step 3: Add Hydration Actions (1 min)

**Action:** Open `Features/Cart/Store/CartActions.cs` and add two action records after the existing actions:

```csharp
public record HydrateCartAction(IReadOnlyList<CartItem> Items);
public record HydrateCartRequestAction;
```

**Why two actions:**

> Two new actions for the hydration flow. `HydrateCartRequestAction` is the trigger -- like `LoadProductsAction` from Day 5, it is a signal with no data. It says "go check localStorage."
>
> `HydrateCartAction` carries the loaded items. If localStorage has cart data, the effect dispatches this action with the loaded items. The reducer then sets the state.
>
> This is the same trigger-then-result pattern as the triple-action pattern from Day 5. Request action triggers the effect. Success action carries the result. The failure action is skipped here because a missing cart is not an error -- it is just a first visit.

---

## Step 4: Add Hydration Reducer (1 min)

**Action:** Open `Features/Cart/Store/CartReducers.cs` and add a reducer method after the existing reducers:

```csharp
[ReducerMethod]
public static CartState ReduceHydrateCartAction(CartState state, HydrateCartAction action)
{
    return state with { Items = action.Items };
}
```

**Why one line is enough:**

> The hydration reducer simply replaces the empty initial cart with the items loaded from localStorage. One line of logic: `state with { Items = action.Items }`.
>
> After this reducer runs, every component subscribed to `IState<CartState>` re-renders with the restored cart. The Checkout guard sees items and allows navigation. The cart counter in the navbar updates. One state change ripples through the entire app.

**FAQ -- "What if the reducer receives null items?"**

> Good defensive thinking. The effect checks for null before dispatching `HydrateCartAction`, so the reducer should never receive null items. For extra safety, `action.Items ?? []` could be added here.

---

## Step 5: Add Hydration Effect (3 min)

**Action:** Open `Features/Cart/Store/CartPersistenceEffects.cs` and add the hydration effect method after the persistence effects:

```csharp
[EffectMethod]
public async Task HandleHydrateCartRequestAction(HydrateCartRequestAction action, IDispatcher dispatcher)
{
    try
    {
        var items = await localStorage.GetItemAsync<List<CartItem>>(CartStorageKey);
        if (items is not null && items.Count > 0)
        {
            dispatcher.Dispatch(new HydrateCartAction(items));
        }
    }
    catch
    {
        // Corrupt data in localStorage -- remove and start fresh
        await localStorage.RemoveItemAsync(CartStorageKey);
    }
}
```

**Step-by-step breakdown:**

> This is the rehydration logic. When `HydrateCartRequestAction` is dispatched -- typically on app startup -- this effect runs.
>
> **Step 1:** Try to load the cart from localStorage. `GetItemAsync<List<CartItem>>` deserializes the JSON back into a C# list. Blazored.LocalStorage handles the JSON conversion automatically.
>
> **Step 2:** Check whether data came back. On a first visit, there is nothing in localStorage, so `items` is null. The null-and-empty check means "no data means first visit, just let the app start with an empty cart."
>
> **Step 3:** If data exists, dispatch `HydrateCartAction` with the loaded items. The reducer sets the state, components re-render, and the cart is restored.
>
> **Step 4: The try/catch.** What if the data in localStorage is corrupt? Maybe the `CartItem` class changed since the data was saved. `GetItemAsync` tries to deserialize the old JSON into the new class structure and throws a `JsonException` if it cannot. The catch block removes the corrupt key and lets the app start fresh. This is graceful degradation -- a real production pattern.

**FAQ -- "Can the localStorage data be versioned?"**

> Absolutely -- that is an enterprise pattern. A payload like `{ version: 2, items: [...] }` could be stored and the version checked before deserializing. If the version does not match, discard the old data. This demo keeps it simple, but versioning is how production apps handle schema migrations in client-side storage.

---

## Step 6: Quick Test of Persistence (1 min)

At this point, run the app to test persistence only (save on change):

1. Navigate to Shop, add 2-3 items.
2. Open DevTools > Application > Local Storage > site URL.
3. The `cart` key is now visible with JSON array data.
4. Add another item -- the JSON updates in real time.
5. Clear cart -- the key disappears.

> Persistence is working. Every cart change saves to localStorage. Next, connect the hydration trigger so the cart loads on startup.

---

## Step 7: Trigger Hydration on Startup (CODE LIVE, 1 min)

**Action:** Open `Components/Layout/MainLayout.razor`. The demo starter has a `CODE LIVE` TODO here -- the instructor types this live. `MainLayout` wraps every page, so an override here fires once on app startup.

Override `OnAfterRenderAsync` and dispatch the hydration request on first render:

```csharp
@code {
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

**Why timing matters:**

> Hydration must happen after the circuit is established. In Blazor Server, that means `OnAfterRenderAsync` with the `firstRender` guard -- not `OnInitialized`, which runs during prerender when JavaScript interop is not yet available.

> Once this is wired, `HydrateCartRequestAction` is dispatched from the layout's first render. The effect loads localStorage, dispatches the hydration action, and the cart is restored before the user interacts.

> Always call `await base.OnAfterRenderAsync(firstRender)` when overriding lifecycle methods — this lets the base class complete its own lifecycle work, even if it appears to do nothing here.

---

## Step 8: Full Integration Test (2 min)

1. Run the app. Navigate to Shop. Add 3 items.
2. Check DevTools -- the `cart` key has 3 items in JSON.
3. **Refresh the page.** Cart counter shows 3. Navigate to Checkout -- guard passes, items display.
4. Remove an item -- DevTools shows 2 items.
5. Close the browser tab. Reopen the app URL. Cart still has 2 items.
6. Clear the cart. DevTools shows the key removed.

> State survived the refresh. State survived closing the tab. One honest caveat: the guard runs in `OnInitialized`, and hydration runs in `OnAfterRenderAsync` -- which is later. So if you hard-refresh while sitting on `/checkout`, the guard still sees an empty cart and still sends you to Shop, where the cart then reappears fully restored. Persistence works; the guard just asks its question before the answer has loaded.

Open DevTools > Application > Local Storage one more time and inspect the data.

> This is where the data lives -- JSON in the browser's localStorage, scoped to this origin (this website's address), accessible to any JavaScript on this page. That is why passwords should never be stored here. Cart contents? Fine. User preferences? Fine. Credentials? Never.

---

## Transition to Practice

> The full persistence + hydration pattern is now in place. Open the student practice starter project and search for `TODO`.
>
> **Practice 1:** The starter ships a working **Recently Viewed** list that loses itself on refresh -- and nothing is pre-wired. You build the whole persistence layer, applying the cart pattern from the demonstration to a new feature: the save effect (the instructor walks that first one as a worked launch), the two hydration actions, the reducer, the load effect, and the `MainLayout` wiring. This is transfer, not reproduction -- and you still have to handle null data and corrupt data.
>
> **Classification activity:** No coding -- pair up and classify 8 scenarios by persistence strategy: localStorage, sessionStorage, or do not persist.
