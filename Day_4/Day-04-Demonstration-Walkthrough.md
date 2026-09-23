# Demo Walkthrough -- Day 4: Fluxor: Actions and Reducers -- Counter Store

**What this is:** A step-by-step companion for the instructor demo. Follow along as the instructor builds a complete Fluxor counter store from scratch -- state, actions, reducers, feature, and component wiring.

**What you'll see:** The instructor creates five Fluxor files that implement the Flux/Redux cycle you designed on paper in Day 3. By the end, a counter component reads state from the Fluxor store and dispatches actions to change it -- with every action visible in Redux DevTools.

> **One-time setup -- install Redux DevTools.** The **Redux** tab is not built into your browser. You install it once as a free browser extension. It is available for **Edge, Chrome, and Firefox**. In Edge, get it from the **Microsoft Edge Add-ons** site, or install it from the **Chrome Web Store** -- Edge can run Chrome extensions. After installing, run the app, press **F12** in the browser, and look for the **Redux** tab. If the browser window is narrow, the tab may be hidden under the `»` (more) menu. You only do this once; it works for every Fluxor day after this.

---

## The Fluxor File Pattern

Every Fluxor feature follows this five-file pattern. Today's demo builds all five for a counter:

| File | Purpose | Day 3 Equivalent |
|------|---------|-------------------|
| `CounterState.cs` | Immutable state record | Your "state shape" diagram |
| `CounterActions.cs` | Action record types | Your action names with data payloads |
| `CounterReducers.cs` | Pure static reducer methods | Your reducer pseudocode |
| `CounterFeature.cs` | Registers state with Fluxor | (Infrastructure -- no Day 3 equivalent) |
| `Counter.razor` | Component wiring | Your "component subscriptions" list |

---

## What's Pre-Built

The project infrastructure is already configured:

- **NuGet package:** `Fluxor.Blazor.Web` installed
- **Program.cs:** `AddFluxor` with assembly scanning and Redux DevTools
- **_Imports.razor:** Fluxor using directives
- **App.razor:** `<StoreInitializer />` component
- **Counter.razor:** UI with buttons, but no Fluxor wiring (TODO comments)

This setup happens once per project. The five-file pattern is what you repeat for every feature.

---

## Step 1: CounterState -- The State Record

```csharp
namespace Day04Demo.Features.Counter.Store;

public record CounterState
{
    public int Count { get; init; }
}
```

**Key concepts:**

- **`record`** instead of `class` -- records provide value equality and the `with` expression for creating modified copies. Two `CounterState` records with the same `Count` are considered equal.

- **`init`** instead of `set` -- the property can only be set during construction. Nobody can write `state.Count = 5` after the record is created. This enforces immutability.

- This is the **single source of truth** for the counter value. Every component reads from here.

---

## Step 2: CounterActions -- Action Records

```csharp
namespace Day04Demo.Features.Counter.Store;

public record IncrementCounterAction;
public record DecrementCounterAction;
public record SetCounterAction(int NewCount);
```

**Key concepts:**

- **Parameterless records** (`IncrementCounterAction`) are signals -- "something happened." The reducer knows how to respond.

- **Records with parameters** (`SetCounterAction(int NewCount)`) carry data -- "set the counter to this specific value."

- **Actions don't do anything.** They describe *what happened*, not *how to respond*. The reducer decides the response.

- **Naming convention:** Verb + Noun + "Action" (e.g., `IncrementCounterAction`, `SetCounterAction`).

---

## Step 3: CounterReducers -- Pure Static Methods

```csharp
namespace Day04Demo.Features.Counter.Store;

using Fluxor;

public static class CounterReducers
{
    [ReducerMethod]
    public static CounterState ReduceIncrementCounterAction(
        CounterState state,
        IncrementCounterAction action)
    {
        return state with { Count = state.Count + 1 };
    }

    [ReducerMethod]
    public static CounterState ReduceDecrementCounterAction(
        CounterState state,
        DecrementCounterAction action)
    {
        return state with { Count = state.Count - 1 };
    }

    [ReducerMethod]
    public static CounterState ReduceSetCounterAction(
        CounterState state,
        SetCounterAction action)
    {
        return state with { Count = action.NewCount };
    }
}
```

**Key concepts:**

- **`static` class and methods** -- reducers have no instance, no constructor, no injected services. They are pure functions.

- **`[ReducerMethod]`** -- tells Fluxor to discover this method. Without it, the reducer is invisible to the store.

- **Two parameters:** current state (first) and the action (second). Returns the new state.

- **`state with { ... }`** -- creates a *copy* of the record with the specified properties changed. The original `state` is untouched. This is immutability in action.

- **`action.NewCount`** -- the SetCounter reducer uses the action's data. The data was provided when someone dispatched `new SetCounterAction(10)`.

**The #1 rule:** Reducers return **new state**. They never modify the input state. `state with { Count = state.Count + 1 }` creates a new `CounterState`. It does NOT change the original.

---

## Step 4: CounterFeature -- Registration

```csharp
namespace Day04Demo.Features.Counter.Store;

using Fluxor;

public class CounterFeature : Feature<CounterState>
{
    public override string GetName() => "Counter";

    protected override CounterState GetInitialState()
    {
        return new CounterState { Count = 0 };
    }
}
```

**What is `Feature<T>`?** `Feature<T>` is a Fluxor base class that registers one state record with the application store. Fluxor scans your assembly at startup, finds every class that extends `Feature<T>`, and calls `GetInitialState()` to seed the store. The two methods you override give Fluxor (1) a display name for Redux DevTools (`GetName()`) and (2) the starting state when the app boots (`GetInitialState()`). Every state type in the app has exactly one `Feature<T>` — without it, any component that injects `IState<CounterState>` throws a "No feature found" exception at runtime.

**Key concepts:**

- **`GetName()`** -- label for Redux DevTools. Shows up in the state tree.

- **`GetInitialState()`** -- the starting state when the app loads. Counter begins at 0.

- **One feature per state type.** Each feature manages its own slice of application state.

---

## Step 5: Counter.razor -- Component Wiring

```razor
@page "/counter"
@inherits FluxorComponent
@inject IState<CounterState> CounterState
@inject IDispatcher Dispatcher

<p class="lead">Current count: <strong>@CounterState.Value.Count</strong></p>

<button class="btn btn-success" @onclick="Increment">+ Increment</button>

@code {
    private void Increment()
    {
        Dispatcher.Dispatch(new IncrementCounterAction());
    }
}
```

**Three lines wire the component to the store:**

| Line | Purpose |
|------|---------|
| `@inherits FluxorComponent` | Subscribes to state changes, triggers re-render automatically |
| `@inject IState<CounterState>` | Read access to current state via `.Value` |
| `@inject IDispatcher` | Sends actions to the store |

**Key concepts:**

- **`@CounterState.Value.Count`** -- reads the current count from the store. `.Value` gives you the state record.

- **`Dispatcher.Dispatch(new IncrementCounterAction())`** -- sends an action. Fluxor finds the matching reducer, the reducer returns new state, the store updates, and the component re-renders.

- **The component never modifies state directly.** It reads and dispatches. That's it.

---

## Step 6: Redux DevTools

After wiring everything, the instructor runs the app and opens F12 -> Redux tab:

- **Action log:** Every button click appears as an entry (IncrementCounterAction, DecrementCounterAction, etc.)
- **State diff:** Click any action to see the before/after state
- **Time travel:** Click older actions to view the state at that point in history

This is the debugging payoff of the Flux pattern: every state change is logged, visible, and reproducible.

---

## Connection to Day 3

Remember Day 3's shopping cart? Products had a `private List<CartItem>` that disappeared when navigating to Cart. Each component had independent state that didn't survive navigation.

Today's counter doesn't have that problem. The state lives in the Fluxor store, outside any component. If you added a second page that also injected `IState<CounterState>`, both pages would see the same count. Practice 3 today solves exactly that -- refactoring Day 3's cart to use Fluxor.

---

## Recovery Notes

**If you see `InvalidOperationException: No feature found for state type CounterState`:** You have not created `CounterFeature.cs` yet (Step 4). The app compiled — Fluxor discovers features at runtime, not compile time. Create `CounterFeature.cs` and hot-reload or restart.

---

## Key Takeaways

1. **Five files, one pattern:** State record, action records, reducer methods, feature class, component wiring. You repeat this for every feature.

2. **Records + `init` = immutability.** State can only be set during construction. Use `with` to create modified copies.

3. **Reducers are pure static functions.** Same input always produces the same output. No side effects, no DI, no async.

4. **Components read and dispatch.** `IState<T>` for reading, `IDispatcher` for dispatching. Never modify state directly.

5. **Redux DevTools shows everything.** Every action, every state change, full history with time travel.
