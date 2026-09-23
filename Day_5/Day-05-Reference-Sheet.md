# Day 5 Reference Sheet -- Fluxor: Effects and Async Operations

**Course:** SDEV 2351 | **Module:** 1 | **CO:** CO2
**Estimated Reading Time:** ~5 minutes

---

<img src="../assets/day-05-effects-triple-action.svg" alt="Flowchart of the Fluxor triple-action pattern for async data loading on the Product catalog demo. Stage 1 Component: Products.razor dispatches LoadProductsAction from OnInitialized. Stage 2 Trigger Action: LoadProductsAction is handled twice — a pure reducer sets IsLoading=true, and an effect begins an async service call. Stage 3 Effect: ProductEffects uses [EffectMethod] with constructor-injected IProductService inside a try/catch; the try branch dispatches LoadProductsSuccessAction(list), the catch branch dispatches LoadProductsFailureAction(ex.Message). Stages 4a/4b Result Actions: Success carries List&lt;Product&gt;, Failure carries a string error. Stages 5a/5b Reducers: success reducer returns a new ProductState with Items filled and IsLoading=false, failure reducer returns a new ProductState with ErrorMessage set. A state-evolution band shows ProductState across all three reducers — initial, trigger (IsLoading=true), success (Items filled), failure (ErrorMessage set). Footer rules: effects are async instance methods with DI, reducers stay pure, and the try/catch branch lives in the effect — never in a reducer." />

**How to read this diagram:** Follow the solid arrows left-to-right — that's the path a single user action takes from the component to the store. The flow **splits in the middle** at the Effect box: the `try` branch (green) produces a Success action; the `catch` branch (red) produces a Failure action. Both branches are dispatched and handled by their own reducer, and each reducer returns a NEW `ProductState` record. The table below the main flow shows how `IsLoading`, `Items`, and `ErrorMessage` change across those three reducers — use it to check your own reducer code when a practice asks you to write one. The **one-line rule for the whole pattern:** async work always lives in the effect; reducers only map actions to new state.

---

> **Companion build guide.** Today you add the **effect** for async loading. The "Now make it wait" half of <strong><a href="Fluxor-Store-Anatomy.html?isCourseFile=true" target="_blank" rel="noopener">Anatomy of a Fluxor Store</a></strong> walks this exact Product-catalog pattern part by part — the three actions, the status flags, and the effect as the only part allowed to wait.

> **Why this needs an effect — the design decision.** Today's effect exists because of one whiteboard question: *does this action have to wait?* <strong><a href="Whiteboarding-A-Ride-Share-Trip.html?isCourseFile=true" target="_blank" rel="noopener">Whiteboarding a Ride-Share Trip</a></strong> makes that exact call while designing an async store from requirements — which actions get an effect (they wait on a service) and which stay a plain reducer (instant). It's the design reasoning behind the triple-action effect you're writing.

> **Unfamiliar C# syntax today?** <strong><a href="/d2l/le/lessons/184456/units/6193672" target="_blank" rel="noopener">Modern C# You'll Meet Here</a></strong> explains the shapes this day introduces -- primary constructors -- the parameters that sit right on the class line (Section 3.1).

## Key Concepts

- **Effects handle side effects** -- reducers are pure (they cannot use async, DI, or API calls). Effects solve this problem. An effect listens for an action, calls a service, and dispatches a new action with the result.
- **Triple-action pattern** (also called the async action pattern or Load/Success/Failure pattern in Redux documentation) -- every async operation uses three actions: (1) trigger (component dispatches), (2) success (effect dispatches with data), (3) failure (effect dispatches with error message).
- **DI in effects** -- effect classes are instances (not static like reducers) with constructors that accept injected services. Fluxor resolves them from the DI container.
- **Loading and error state** -- state records include `bool IsLoading` and `string? ErrorMessage` properties. Reducers set these based on which of the three actions fires.

## Syntax Reference: Triple-Action Pattern Template

**Actions:**
```csharp
public record LoadProductsAction;
public record LoadProductsSuccessAction(List<Services.Product> Products);
public record LoadProductsFailureAction(string Error);
```

**State:**
```csharp
public record ProductState
{
    public List<Services.Product> Items { get; init; } = [];   // Services. qualifier required -- the store namespace contains "Product"
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
}
```

**Reducers:**
```csharp
public static class ProductReducers
{
    [ReducerMethod]
    public static ProductState ReduceLoadProductsAction(ProductState state, LoadProductsAction action)
        => state with { IsLoading = true, ErrorMessage = null };

    [ReducerMethod]
    public static ProductState ReduceLoadProductsSuccessAction(ProductState state, LoadProductsSuccessAction action)
        => state with { Items = action.Products, IsLoading = false };

    [ReducerMethod]
    public static ProductState ReduceLoadProductsFailureAction(ProductState state, LoadProductsFailureAction action)
        => state with { ErrorMessage = action.Error, IsLoading = false };
}
```

**Effect:**
```csharp
// Primary constructor -- parameter available to all methods
public class ProductEffects(IProductService productService)
{
    [EffectMethod]
    public async Task HandleLoadProductsAction(LoadProductsAction action, IDispatcher dispatcher)
    {
        try
        {
            var products = await productService.GetAllAsync();
            dispatcher.Dispatch(new LoadProductsSuccessAction(products));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadProductsFailureAction(ex.Message));
        }
    }
}
```

**Component (trigger the load):** dispatch the trigger action from `OnInitialized` so the load starts the moment the component mounts. The dispatch is synchronous -- it just pushes the action to the store; the async work happens in the effect.
```razor
@inherits FluxorComponent
@inject IState<ProductState> ProductState
@inject IDispatcher Dispatcher

@code {
    protected override void OnInitialized()
    {
        base.OnInitialized();           // required -- FluxorComponent sets up its state subscription here
        Dispatcher.Dispatch(new LoadProductsAction());
    }
}
```
Use `OnInitialized`, not `OnInitializedAsync` -- dispatching is synchronous, so there is nothing to await at the component level.

## Common Mistakes

| Mistake | Symptom | Fix |
|---------|---------|-----|
| Effect method not `async Task` | Effect never runs | Change return type to `async Task` |
| Dispatching success from component | Service call in component, not effect | Move service call to effect class |
| Forgot `Feature<T>` class | State always has default values | Create feature class with `GetInitialState()` |
| Calling `StateHasChanged()` manually | Unnecessary; may cause double render | Remove it -- `FluxorComponent` handles re-renders |

## Quick Check

1. **Why can't reducers call APIs?**
   <details><summary>Answer</summary>Reducers must be pure -- static, synchronous, no side effects. They take state + action and return new state. API calls are async side effects, which belong in effects.</details>

2. **What two parameters does an [EffectMethod] receive?**
   <details><summary>Answer</summary>The action that triggered it and an IDispatcher to dispatch new actions (success or failure). Note: the <code>IDispatcher</code> is provided automatically by Fluxor as a method parameter — you do not add it to the class constructor.</details>

3. **In the triple-action pattern, who dispatches the success action?**
   <details><summary>Answer</summary>The effect dispatches it after the service call succeeds. The component only dispatches the trigger action.</details>

---

## Pre-Class Reading for Day 6

| Reading | Time | Location |
|---------|------|----------|
| **BYSResult Pattern Guide** | ~20 min | Linked from the Day 6 page in Brightspace (Pre-Class Preparation section) |

**Focus:** Core Concepts (sections 1-4), Basic Pattern (section 5), Integration with Fluxor (section 9).

## Watch a Fluxor effect load async data in action

**Where does the async work happen if reducers are pure?** A 12-second screen recording of the Day 5 demo — navigate to /products, watch the reducer flip IsLoading=true (spinner), then see the effect's await complete and dispatch LoadProductsSuccess so the reducer fills in the product cards. The triple-action pattern (trigger, success, failure) in one shot.

<video controls preload="metadata" width="720" style="max-width:100%;height:auto;border:1px solid #DEE2E6;">
  <source src="../demo-videos/day-05-fluxor-effect-loading.mp4" type="video/mp4" />
  <track kind="captions" src="../demo-videos/day-05-fluxor-effect-loading.vtt" srclang="en" label="English" default />
  Your browser does not support embedded video. <a href="../demo-videos/day-05-fluxor-effect-loading.mp4">Download the MP4 (12 s, silent, captioned)</a>.
</video>

- Video file: [`day-05-fluxor-effect-loading.mp4`](../demo-videos/day-05-fluxor-effect-loading.mp4) (12 s, silent, 1280×720, H.264)
- Caption track: [`day-05-fluxor-effect-loading.vtt`](../demo-videos/day-05-fluxor-effect-loading.vtt) (WebVTT sidecar — WCAG 1.2.2)
- Full text transcript (screen-reader friendly): [`day-05-fluxor-effect-loading-transcript.md`](../demo-videos/day-05-fluxor-effect-loading-transcript.md) (WCAG 1.2.1)

The video has **no audio**. All narration is in the on-screen captions and the transcript. Nothing in Day 5 assessments requires watching the video — the transcript is a complete alternative.

---

*See also: [Fluxor Docs](https://github.com/mrpmorris/Fluxor/blob/master/Docs/README.md) | [MS Learn: Blazor State Management](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/) | Fluxor Quick Reference Guide (Day 3 Guide folder) | Day 5 practices in Code-Examples/*
