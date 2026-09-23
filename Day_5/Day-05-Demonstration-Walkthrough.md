# Demo Walkthrough -- Day 5: Fluxor: Effects and Async Operations -- Product Loading Effect

**Purpose:** A comprehensive, step-by-step reference for the Day 5 demo. Use this to prepare before class, to follow along while coding, or as a detailed guide when your instructor's abbreviated demo script needs more context.

**Starting point:** The `Code-Examples/demo/starter/Day05Demo` project -- a standalone Blazor Web App with Fluxor configured, `IProductService`/`MockProductService` registered, and a Products page with loading/error/data UI pre-built. The five `Features/Product/Store/` files already compile without errors. **Three of them -- `ProductActions.cs`, `ProductState.cs` and `ProductFeature.cs` -- are already written for you**; your instructor walks through them rather than typing them. The reducer bodies in `ProductReducers.cs`, the constructor and try/catch in `ProductEffects.cs`, and the `Products.razor` wiring are typed in during the demonstration.

**End state:** A working product catalog with async loading via effects. Loading spinner appears for 1 second, then products display. Error state tested by making the mock service throw. Redux DevTools shows the full triple-action flow.

---

## Step 1: Open the Starter Project and Show What's Pre-Built (1 min)

Open `Code-Examples/demo/starter/Day05Demo` in Visual Studio. Press F5 to run.

**What you'll see:**
- The Products page loads with a placeholder message: "Wire the Fluxor store to see products here."
- The loading spinner and error display are in the markup but gated behind placeholder `isLoading` / `errorMessage` fields
- The `@code` block has a TODO comment -- no Fluxor wiring yet

**Before coding, open each file briefly to see the pre-built infrastructure:**

1. **`Services/IProductService.cs`** -- `Task<List<Product>> GetAllAsync()`. One method -- returns a list of products asynchronously. This is what the effect will call.

2. **`Services/MockProductService.cs`** -- `Task.Delay(1000)` simulates a 1-second network delay. In a real app, this would be an HTTP call or database query. The delay exists so the loading state is visible.

3. **`Services/Product.cs`** -- `record Product(int Id, string Name, decimal Price, string Category)`. A simple data record with positional parameters -- concise syntax.

4. **`Program.cs`** -- `AddFluxor(options => options.ScanAssemblies(...))` and `AddScoped<IProductService, MockProductService>()`. Fluxor is registered. The product service is registered. Infrastructure is done.

   > **Service Lifetimes -- Why `AddScoped`?** When you register a service, you choose how long it lives:
   > - **`AddScoped`** -- one instance per HTTP request (or Blazor circuit). Most services use this. Each user gets their own instance.
   > - **`AddTransient`** -- new instance every time it's requested. Use for lightweight, stateless helpers.
   > - **`AddSingleton`** -- one instance shared by ALL users for the entire app lifetime. Use for configuration, caches, or thread-safe utilities.
   >
   > `MockProductService` is `AddScoped` because each user's request should get its own service instance. If it were `AddSingleton`, all users would share the same instance -- fine here (stateless mock), but dangerous for services with user-specific state like a database context. You'll see `AddScoped` throughout this course -- it's the safe default for most services.

5. **`Products.razor`** -- Three UI sections are pre-built: a loading spinner, an error alert, and product cards. TODO comments mark where wiring goes. The UI handles three states -- loading, error, and data -- but right now everything is hardcoded to show the placeholder. The goal of this demo: build the store so this page displays real products.

> **Why start here:** This split makes it easy to see the separation between infrastructure (pre-built, one-time setup) and the pattern that repeats for every feature: state, actions, reducers, effects, feature class, component wiring.

---

## Step 2: ProductActions.cs -- already written; your instructor narrates it (2 min)

**Action:** Open `Features/Product/Store/ProductActions.cs` -- the three action records are **already written for you**. Read them as your instructor walks through each one:

```csharp
namespace Day05Demo.Features.Product.Store;

// Trigger -- dispatched by the component to start loading
public record LoadProductsAction;

// Success -- dispatched by the effect when data arrives
public record LoadProductsSuccessAction(List<Services.Product> Products);

// Failure -- dispatched by the effect when the service call fails
public record LoadProductsFailureAction(string Error);
```

**Why three records:**

> The triple-action pattern. Three records, each serving a distinct role.
>
> `LoadProductsAction` is the trigger -- the component dispatches this to say "I need products." No data, just a signal.
>
> `LoadProductsSuccessAction` carries the result -- the effect dispatches this when the service call succeeds. Note the `List<Services.Product>` qualifier: the `Services.` prefix is required because the current namespace already contains a `Product` type.
>
> `LoadProductsFailureAction` carries the error message -- the effect dispatches this when the service call throws.
>
> This three-action template is the template for every async data load in Fluxor. The names change, the structure stays the same.

---

## Step 3: ProductState.cs -- already written; your instructor narrates it (1 min)

**Action:** Open `Features/Product/Store/ProductState.cs` -- the state properties are **already written for you**. Read them as your instructor compares them to Day 4's state record:

```csharp
namespace Day05Demo.Features.Product.Store;

public record ProductState
{
    public List<Services.Product> Items { get; init; } = [];
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
}
```

**Why three properties:**

> Same record pattern as Day 4's `CounterState`, but with three properties instead of one.
>
> `Items` -- the product list, defaults to empty. `IsLoading` -- tracks whether a service call is in flight. `ErrorMessage` -- nullable string, set when the service call fails.
>
> These three properties model the lifecycle of an async operation: not started, loading, succeeded (has data), or failed (has error). The UI uses these to decide what to show.

---

## Step 4: Fill in ProductReducers.cs (2 min)

**Action:** Open the `TODO`-stubbed `Features/Product/Store/ProductReducers.cs` and replace the placeholder `=> state;` bodies with the real reducer expressions:

```csharp
using Fluxor;

namespace Day05Demo.Features.Product.Store;

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

**Reducer-by-reducer breakdown:**

> **ReduceLoadProductsAction:** When the trigger action fires, set `IsLoading = true` and clear any previous error. This is the "starting" state.
>
> **ReduceLoadProductsSuccessAction:** When the effect reports success, store the products and turn off loading. The component re-renders and shows the product cards.
>
> **ReduceLoadProductsFailureAction:** When the effect reports failure, store the error message and turn off loading. The component re-renders and shows the error alert.
>
> Notice that all three reducers are static, synchronous, and pure. They never call services or do async work. They just take state + action and return new state with `with`. This is exactly the same pattern as Day 4 -- the reducers do not know or care that an effect exists.

---

## Step 5: Fill in ProductEffects.cs -- THE NEW PIECE (3 min)

**Action:** Open the `TODO`-stubbed `Features/Product/Store/ProductEffects.cs`. Add the `(IProductService productService)` primary constructor and replace the placeholder `await Task.CompletedTask;` body with the try/catch service call:

```csharp
using Fluxor;
using Day05Demo.Services;

namespace Day05Demo.Features.Product.Store;

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

**Why this is the key teaching moment — three things to notice:**

> This is the new piece. Everything else has been Day 4 patterns with more properties. The effect class is new.
>
> The class declaration `public class ProductEffects(IProductService productService)` uses a primary constructor -- the parameter is available to all methods. Fluxor creates this class and injects `IProductService` from the DI container, just like any other service.
>
> **`[EffectMethod]`:** This attribute tells Fluxor "when `LoadProductsAction` is dispatched, call this method." Fluxor figures out which action to listen for from the first parameter's type.
>
> **`async Task`:** Effects return `async Task`. They CAN await. This is the opposite of reducers, which are synchronous and static.
>
> **Two parameters:** The action that triggered this effect, and an `IDispatcher` used to dispatch new actions. The effect does not return state -- it dispatches actions, and the reducers handle the state update.
>
> **The try/catch:** Try: call the service, get the products, dispatch success with the data. Catch: dispatch failure with the error message. The effect decides which path based on whether the service throws.
>
> The effect is the intermediary between the component and the service. The component says "I want products" (trigger action). The effect does the async work. The effect tells the store what happened (success or failure action). The reducer updates state. The component re-renders. The effect never touches state directly.

**FAQ -- "Why not just call the service from the component?"**

> You could -- and that is exactly how this worked in SDEV 2301. But then the component contains both UI logic and data-fetching logic. The triple-action pattern separates them: the component dispatches an intent, the effect does the work, and the reducer updates state. Each piece is testable independently.

**FAQ -- primary constructor syntax**

> The `(IProductService productService)` after the class name is a primary constructor. It defines a constructor parameter available to all methods without writing a separate private field and constructor body. Same syntax that appears on `record` types. Both approaches work; primary constructors are used here for brevity.

---

## Step 6: ProductFeature.cs -- already written; your instructor narrates it (1 min)

**Action:** Open `Features/Product/Store/ProductFeature.cs` -- it already shows the `Feature<ProductState>` pattern; your instructor narrates it while confirming each line:

```csharp
using Fluxor;

namespace Day05Demo.Features.Product.Store;

public class ProductFeature : Feature<ProductState>
{
    public override string GetName() => "Product";

    protected override ProductState GetInitialState()
        => new() { Items = [], IsLoading = false, ErrorMessage = null };
}
```

**Why this class exists:**

> Same Feature pattern as Day 4. It registers the state with Fluxor and provides the initial values. Without this class, Fluxor will not know about `ProductState` -- the app will compile but state will always be default.

---

## Step 7: Wire Products.razor (2 min)

**Action:** Open `Components/Pages/Products.razor` and replace the TODO stubs.

Add at the top of the file (after `@page "/products"`):

```razor
@inherits FluxorComponent
@inject IState<ProductState> ProductState
@inject IDispatcher Dispatcher
```

Replace the loading conditional:
```razor
@if (ProductState.Value.IsLoading)
```

Replace the error conditional:
```razor
else if (ProductState.Value.ErrorMessage is not null)
```

Replace the error message display:
```razor
<p>@ProductState.Value.ErrorMessage</p>
```

> **Only the `<p>` tag changes** — the `<h5 class="alert-heading">Error Loading Products</h5>` heading and the surrounding `<div class="alert">` wrapper are pre-built and stay as-is.

Replace the data section:
```razor
@foreach (var product in ProductState.Value.Items)
{
    <div class="col-md-4 mb-3">
        <div class="card">
            <div class="card-body">
                <h5 class="card-title">@product.Name</h5>
                <p class="card-text">@product.Price.ToString("C")</p>
                <span class="badge bg-secondary">@product.Category</span>
            </div>
        </div>
    </div>
}
```

Add to the `@code` block:

```csharp
protected override void OnInitialized()
{
    base.OnInitialized();
    Dispatcher.Dispatch(new LoadProductsAction());
}
```

**Wiring notes:**

> Same wiring pattern as Day 4: inherit from `FluxorComponent`, inject state and dispatcher.
>
> The new part: in `OnInitialized`, dispatch `LoadProductsAction` to kick off the data load. The dispatch itself is synchronous -- it just pushes an action object to the store. The async work happens in the effect, not here.
>
> Use `OnInitialized` (not `OnInitializedAsync`) because dispatching is synchronous. The effect runs the async operation on its own.
>
> `base.OnInitialized()` is required -- that is where `FluxorComponent` sets up its state subscription.

---

## Step 8: Run and Demonstrate (2 min)

Run the app. Navigate to Products.

**Happy path (1 min):**

1. The loading spinner shows for about 1 second (the `Task.Delay(1000)` in MockProductService).
2. Products appear in cards: Wireless Mouse, USB-C Hub, Mechanical Keyboard, 27" Monitor, Webcam HD.
3. Open F12 -> Redux tab (browser DevTools) and observe the action sequence:
   - `LoadProductsAction` (dispatched by component on init)
   - `LoadProductsSuccessAction` (dispatched by effect after service returned data)
4. Clicking `LoadProductsAction` shows the state snapshot: `IsLoading: true, Items: [], ErrorMessage: null`.
5. Clicking `LoadProductsSuccessAction` shows the state snapshot: `IsLoading: false, Items: [5 products], ErrorMessage: null`.

**Error path (1 min):**

Modify `MockProductService.GetAllAsync()` -- add before the `return`:
```csharp
throw new InvalidOperationException("Database connection failed");
```

Run again:
1. Loading spinner appears briefly.
2. Error alert displays: "Database connection failed."
3. Redux DevTools shows: `LoadProductsAction` → `LoadProductsFailureAction`.
4. State snapshot: `IsLoading: false, Items: [], ErrorMessage: "Database connection failed"`.

Remove the `throw new InvalidOperationException(...)` line from `Services/MockProductService.cs` to restore the happy-path demo state.

> Effects handle the async complexity. Reducers stay pure. The component dispatches a trigger, and the effect and reducers handle the rest. This same pattern works for any async operation -- loading users, saving forms, deleting records.

---

## Transition to Practice

> That is the complete triple-action pattern: three actions, three reducers, one effect, one feature class, and component wiring. Six files, one repeatable pattern. Practice 1 is a worked launch -- the instructor walks Task 1 on screen, then you recall the pattern and complete the rest independently. Practice 2 is the full product catalog -- the same pattern you just watched -- and Practice 3 adds retry and empty-state handling on top.
