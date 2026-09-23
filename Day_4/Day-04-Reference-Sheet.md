# Day 4 Reference Sheet -- Fluxor: Actions and Reducers

**Course:** SDEV 2351 | **Module:** 1 | **CO:** CO2
**Estimated Reading Time:** ~5 minutes

---

<img src="../assets/day-04-fluxor-cycle.svg" alt="Horizontal diagram of the Fluxor cycle traced through a single Counter click — five numbered stages from Component dispatch through Action, Reducer, New State, and Store, with a dashed return loop for FluxorComponent auto-rendering. Below the cycle, a comparison contrasts a mutating C# event handler against a Fluxor reducer that returns a replacement record using the with expression." />

**How to read this diagram.** Work left to right through stages 1–5: (1) your component injects `IDispatcher` and dispatches a new action object, (2) the action is a plain record describing what happened, (3) the reducer is a pure static method marked `[ReducerMethod]` that returns *new* state using the `with` expression, (4) the new state replaces the old one (the old record is garbage-collected, untouched), (5) the Fluxor Store holds the new value and your `FluxorComponent`-derived component re-renders automatically via `IState<T>`. The dashed green loop back to the component is the subscribe half — you never call `StateHasChanged()` yourself. The comparison band below is the **single most important thing** to internalize today: C# event handlers mutate in place (`count++`); Fluxor reducers return a replacement (`s with { Count = s.Count + 1 }`).

---

> **Companion build guide.** As you build the Counter store today, keep <strong><a href="Fluxor-Store-Anatomy.html?isCourseFile=true" target="_blank" rel="noopener">Anatomy of a Fluxor Store</a></strong> open beside this sheet. It walks the same parts you're building — action → state → reducer → feature — as working code, using this exact Counter as its running example.

> **See where today's Counter came from.** <strong><a href="Whiteboarding-A-Turnstile-Counter.html?isCourseFile=true" target="_blank" rel="noopener">Whiteboarding a Turnstile Counter</a></strong> designs this exact store from a real-world scene — a lobby with IN/OUT turnstiles and an occupancy sign: turnstile IN → `IncrementCounterAction`, OUT → `DecrementCounterAction`, the sign → `CounterState.Count`. It also shows *why* the Counter needs no effect (a turnstile turns instantly — nothing waits). Read it to see the design decision behind the code you're typing.

> **Unfamiliar C# syntax today?** <strong><a href="/d2l/le/lessons/184456/units/6193672" target="_blank" rel="noopener">Modern C# You'll Meet Here</a></strong> explains the shapes this day introduces -- collection expressions and the spread `..` (Section 3.2), and `init`-only properties (Section 3.3).

## Key Concepts



- **Fluxor setup** -- Install `Fluxor.Blazor.Web` from NuGet. Register with `AddFluxor` in Program.cs, and call `o.UseReduxDevTools()` inside it to enable the debugger. Add `@using` directives in _Imports.razor. Place `<Fluxor.Blazor.Web.StoreInitializer @rendermode="RenderMode.InteractiveServer" />` in App.razor (it starts the store -- see **What `StoreInitializer` does** below).
- **Immutable state** -- state is a `record` type with `init` properties. Reducers return *new* state using `with` expressions. The original state object is never modified.
- **Actions** -- `record` types that describe what happened. Use parameterless records for signals (example: `IncrementCounterAction`). Use records with parameters for data carriers (example: `SetCounterAction(int NewCount)`).
- **Reducers** -- `static` methods marked with `[ReducerMethod]`. Each reducer takes the current state and an action, then returns new state. Reducers are pure functions: no side effects, no async, no DI.

## Fluxor Setup Checklist

| Step | File | Code |
|------|------|------|
| 1. NuGet | Terminal | `dotnet add package Fluxor.Blazor.Web` (+ `Fluxor.Blazor.Web.ReduxDevTools` for the debugger) |
| 2. Register | Program.cs | `builder.Services.AddFluxor(o => { o.ScanAssemblies(typeof(Program).Assembly); o.UseReduxDevTools(); });` -- the `UseReduxDevTools()` call needs `using Fluxor.Blazor.Web.ReduxDevTools;` and the package; the package reference alone does **not** enable DevTools |
| 3. Usings | _Imports.razor | `@using Fluxor` and `@using Fluxor.Blazor.Web.Components` |
| 4. Initialize | App.razor | `<Fluxor.Blazor.Web.StoreInitializer @rendermode="RenderMode.InteractiveServer" />` (before `<Routes>`) |

### What `StoreInitializer` does (and why it needs `@rendermode`)

`<StoreInitializer />` is the component that actually **starts the store** -- it calls Fluxor's `InitializeAsync()` and turns on the effects pipeline. `AddFluxor(...)` in `Program.cs` only *registers* Fluxor; without `StoreInitializer` rendered in `App.razor`, the store is never initialized, so dispatched actions never reach your reducers and **every value stays at its starting default (a counter stuck at 0, a list that stays empty)** -- with no error and a clean build. If a whole Fluxor feature looks dead, check `App.razor` first.

It also needs its **own** `@rendermode`. It sits *outside* `<Routes>`, so the render mode on `<Routes>` does not cover it -- give it `@rendermode="RenderMode.InteractiveServer"` too. (More: **Blazor Gotchas Section 3.4**.)

## Code Pattern -- Counter Store

```csharp
// CounterState.cs -- immutable state record
public record CounterState
{
    public int Count { get; init; }
}

// CounterActions.cs -- what happened
public record IncrementCounterAction;
public record SetCounterAction(int NewCount);

// CounterReducers.cs -- how state changes
public static class CounterReducers
{
    [ReducerMethod]
    public static CounterState ReduceIncrementCounterAction(
        CounterState state, IncrementCounterAction action)
    {
        return state with { Count = state.Count + 1 };
    }

    [ReducerMethod]
    public static CounterState ReduceSetCounterAction(
        CounterState state, SetCounterAction action)
    {
        return state with { Count = action.NewCount };
    }
}

// CounterFeature.cs -- register with Fluxor
public class CounterFeature : Feature<CounterState>
{
    public override string GetName() => "Counter";
    protected override CounterState GetInitialState()
        => new CounterState { Count = 0 };
}
```

> **Naming convention — `Reduce<ActionName>`:** A reducer method is named by putting `Reduce` in front of the *full* action name — `IncrementCounterAction` → `ReduceIncrementCounterAction`, `OrderPlacedAction` → `ReduceOrderPlacedAction`. This is the standard Fluxor convention (from Mark Morris's official Fluxor docs), and it's the shape you'll see in every store this course builds. The `Reduce` prefix tells a reader "this method is a reducer," and keeping the action's full name (including its `Action` suffix) tells them exactly which action it handles. Don't shorten it to a business verb like `NewOrder` — that hides that the method is a reducer.

**Component wiring** (Counter.razor):
```razor
@inherits FluxorComponent
@inject IState<CounterState> CounterState
@inject IDispatcher Dispatcher

<p>Count: @CounterState.Value.Count</p>
<button @onclick="() => Dispatcher.Dispatch(new IncrementCounterAction())">+1</button>
```

## Testing Reducers

Reducers are pure static functions -- test them directly without Fluxor:

```csharp
using Xunit;

public class CounterReducerTests
{
    [Fact]
    public void ReduceIncrementCounterAction_WithCountOf5_ReturnsCountOf6()
    {
        // Arrange
        var initialState = new CounterState { Count = 5 };
        var action = new IncrementCounterAction();

        // Act
        var newState = CounterReducers.ReduceIncrementCounterAction(
            initialState, action);

        // Assert
        Assert.Equal(6, newState.Count);
    }
}
```

**Key insight:** No Fluxor setup needed. Call the static method, check the return value.

**Naming convention:** `MethodName_Scenario_ExpectedResult`

**Run tests:** Test Explorer in Visual Studio (`Test > Test Explorer`, or `Ctrl+E, T`).

## .NET Naming Conventions

| Rule | Good | Bad |
|------|------|-----|
| PascalCase — public types | `CounterState` | `counterState`, `counter_state` |
| PascalCase — public properties | `public int Count { get; init; }` | `public int count` |
| PascalCase — methods | `GetInitialState()` | `getInitialState()` |
| camelCase — private fields | `private int _count` | `private int Count` |
| Interface prefix `I` | `IProductService` | `ProductService` (as interface name) |
| One type per file | `CounterState.cs` | `StoreTypes.cs` (multiple types) |
| File name = type name | `CounterReducers.cs` | `Reducers.cs` |

**Fluxor-specific patterns:**

| Element | Convention | Example |
|---------|-----------|---------|
| State record | `FeatureNameState` | `CounterState`, `CartState` |
| Action record | `VerbNounAction` | `IncrementCounterAction`, `AddToCartAction` |
| Reducer class | `FeatureNameReducers` | `CounterReducers`, `CartReducers` |
| Effect class *(Day 5)* | `FeatureNameEffects` | `CounterEffects`, `CartEffects` |
| Feature class | `FeatureNameFeature` | `CounterFeature`, `CartFeature` |
| Public properties | PascalCase | `Count`, `Items`, `IsLoading` |
| Private fields | _camelCase | `_logger`, `_httpClient` |
| Method parameters | camelCase | `state`, `action`, `dispatcher` |
| Test method | `Method_Scenario_ExpectedResult` | `ReduceIncrementCounterAction_WithCountOf5_ReturnsCountOf6` |

## Common Mistakes

| Mistake | Symptom | Fix |
|---------|---------|-----|
| Missing `@inherits FluxorComponent` | Component shows initial state but never updates | Add `@inherits FluxorComponent` to the component |
| Using `set` instead of `init` on state | State can be mutated directly, breaking Flux contract | Use `init` on all state properties |
| Missing `[ReducerMethod]` attribute | Action dispatches but state does not change | Add `[ReducerMethod]` to every reducer method |
| Mutating state instead of using `with` | Unpredictable behavior; Redux DevTools shows no diff | Return `state with { ... }` to create a new record. The `with` keyword creates a copy of the record with the specified properties changed -- the original is untouched. |
| No **Redux** tab in browser F12 | The Redux tab never appears in developer tools | Install the free **Redux DevTools** browser extension (Edge: Microsoft Edge Add-ons or the Chrome Web Store; also Chrome and Firefox). F12 is the **browser's** developer tools, not Visual Studio. |

## Quick Check

1. **What are the two parameters a reducer method takes?**
   <details><summary>Answer</summary>The current state (e.g., CounterState) and the action (e.g., IncrementCounterAction). The method returns the new state.</details>

2. **Why must reducers be static with no injected services?**
   <details><summary>Answer</summary>Reducers must be pure functions -- same inputs always produce the same output, with no side effects. Service calls (API, database) belong in effects (Day 5), not reducers.</details>

3. **How does a component read state from the Fluxor store?**
   <details><summary>Answer</summary>Inject IState&lt;T&gt; (e.g., @inject IState&lt;CounterState&gt; CounterState) and access values through .Value (e.g., CounterState.Value.Count). The component must inherit FluxorComponent to auto-subscribe to changes.</details>

## Watch a Fluxor counter store in action

**Where does the counter's value actually live?** A 17-second screen recording of the Day 4 demo — click Increment three times, watch the count tick to 3, click Set to 10 and see the state jump, then Decrement twice to 8. The component itself owns no counter field. Every change flows through dispatch → reducer → new state → re-render.

<video controls preload="metadata" width="720" style="max-width:100%;height:auto;border:1px solid #DEE2E6;">
  <source src="../demo-videos/day-04-fluxor-counter.mp4" type="video/mp4" />
  <track kind="captions" src="../demo-videos/day-04-fluxor-counter.vtt" srclang="en" label="English" default />
  Your browser does not support embedded video. <a href="../demo-videos/day-04-fluxor-counter.mp4">Download the MP4 (17 s, silent, captioned)</a>.
</video>

- Video file: [`day-04-fluxor-counter.mp4`](../demo-videos/day-04-fluxor-counter.mp4) (17 s, silent, 1280×720, H.264)
- Caption track: [`day-04-fluxor-counter.vtt`](../demo-videos/day-04-fluxor-counter.vtt) (WebVTT sidecar — WCAG 1.2.2)
- Full text transcript (screen-reader friendly): [`day-04-fluxor-counter-transcript.md`](../demo-videos/day-04-fluxor-counter-transcript.md) (WCAG 1.2.1)

The video has **no audio**. All narration is in the on-screen captions and the transcript. Nothing in Day 4 assessments requires watching the video — the transcript is a complete alternative.

---

*See also: [Fluxor Docs](https://github.com/mrpmorris/Fluxor/blob/master/Docs/README.md) | [MS Learn: Blazor State Management](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/) | Fluxor Quick Reference Guide (Day 3 Guide folder) | [Fluxor GitHub](https://github.com/mrpmorris/Fluxor) | Day 4 practice projects in Code-Examples/*
