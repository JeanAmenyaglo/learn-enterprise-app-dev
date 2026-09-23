# Demo Walkthrough -- Day 6: Centralized State, Component Integration, and the BYSResult Pattern -- Dashboard Multi-Component + Result Refactor

**Purpose:** A comprehensive, step-by-step reference for the Day 6 demo. Use this to prepare before class, to follow along while coding, or as a detailed guide for reviewing the demo afterward.

**Starting point:** The `demo/starter/Day06Demo` project -- a standalone Blazor Web App with Fluxor configured, a complete Dashboard store (state, actions, reducers, effects, feature), three component `.razor` files with UI markup but no Fluxor wiring, and a `MockDashboardService` that returns raw `DashboardData` (not `Result<T>`). This is the demonstration project your instructor live-codes in class; you receive it as `Day-06-Demonstration-Starter.zip` so you can code along.

**End state:** A working dashboard with three child components all subscribing to the same `DashboardState`. The service returns `Result<DashboardData>` and the effect uses `result.IsSuccess` / `result.IsFailure` instead of try/catch. One action dispatch from the parent page updates all three components simultaneously.

---

## Step 1: Open the Starter Project and Show What's Pre-Built (2 min)

Open `Code-Examples/demo/starter/Day06Demo` in Visual Studio. Press F5 to run.

**What you'll see:**
- The Dashboard page loads with three card panels showing `--` placeholders
- The store infrastructure is complete but the components are not wired to it
- The `Dashboard.razor` parent dispatches `LoadDashboardAction` in `OnInitialized`

**Before coding, open each file briefly to see the pre-built infrastructure:**

1. **`Features/Dashboard/Store/DashboardState.cs`** -- `NotificationCount`, `ProductCount`, `UserName`, `IsLoading`, `ErrorMessage`. Same state record pattern as Day 5. Five properties tracking the dashboard data and async lifecycle.

2. **`Features/Dashboard/Store/DashboardActions.cs`** -- Three action records: Load, LoadSuccess, LoadFailure. Triple-action pattern from Day 5.

3. **`Features/Dashboard/Store/DashboardReducers.cs`** -- Three reducer methods, one per action. Same pure function pattern.

4. **`Features/Dashboard/Store/DashboardEffects.cs`** -- The current effect calls the service with a try/catch. This gets refactored to use `Result<T>` later in the demo.

5. **`Services/MockDashboardService.cs`** -- Uses `Task.Delay(300)` and returns raw `DashboardData`. No Result wrapper yet.

6. **`Components/Pages/Dashboard.razor`** -- Renders three child components and dispatches `LoadDashboardAction` in `OnInitialized`. The children are not yet wired to the store.

7. **`Components/Pages/NotificationPanel.razor`** -- UI markup is complete. The TODO comment marks where Fluxor wiring will be added.

> **Why start here:** All the Fluxor infrastructure (state, actions, reducers, effects, feature) is identical to Day 5. The new concept is *multiple components subscribing to the same state* -- and that requires only three lines per component.

---

## Step 2: Wire NotificationPanel.razor (2 min)

**Action:** Open `Components/Pages/NotificationPanel.razor`. Add at the very top of the file (before the `<div>`):

```razor
@using Fluxor
@inherits Fluxor.Blazor.Web.Components.FluxorComponent
@inject IState<DashboardState> DashboardState
```

Then replace the placeholder text:

```razor
<!-- Replace this: -->
<!-- TODO: [CODE LIVE] Replace with @DashboardState.Value.NotificationCount -->
--

<!-- With this: -->
@DashboardState.Value.NotificationCount
```

**Why three lines:**

> Three lines at the top. First, `@using Fluxor` makes `IState<T>` available. Second, `@inherits FluxorComponent` is the base class that subscribes to state changes -- without it, the component renders once and never updates. Third, `@inject IState<DashboardState>` gives the component access to the store slice.
>
> Then the data gets bound. `@DashboardState.Value.NotificationCount` reads the current notification count from the store. When the store updates, `FluxorComponent` triggers a re-render and this value changes.

**FAQ -- "Why `FluxorComponent` and not just `ComponentBase`?"**
> `ComponentBase` does not know about Fluxor. `FluxorComponent` subscribes to `IState<T>` changes in the background and calls `StateHasChanged()` when the store updates. Without it, the subscription would have to be wired up manually.

---

## Step 3: Wire ProductSummary.razor (2 min)

**Action:** Open `Components/Pages/ProductSummary.razor`. Add the same three directives at the top:

```razor
@using Fluxor
@inherits Fluxor.Blazor.Web.Components.FluxorComponent
@inject IState<DashboardState> DashboardState
```

Replace the placeholder with:

```razor
@DashboardState.Value.ProductCount
```

**Why the same three lines:**

> Same three lines. Same state type -- `IState<DashboardState>`. Different property -- `ProductCount` instead of `NotificationCount`. Both components subscribe to the same store slice.

---

## Step 4: Wire UserProfile.razor (1 min)

**Action:** Open `Components/Pages/UserProfile.razor`. Same three directives. Replace the placeholder with:

```razor
@DashboardState.Value.UserName
```

**Why the pattern is the same:**

> Third component, same pattern. Three components, one shared store, three different pieces of data. The state record holds all of it.

---

## Step 4b: Add the Loading/Error Conditional Block (2 min)

**Action:** The bare `@DashboardState.Value.NotificationCount` binding shows the count, but `DashboardState` also carries `IsLoading` and `ErrorMessage` -- the async-lifecycle properties from Day 5. With only the bare binding, the component shows a raw `0` during the 300ms service delay and a stale `0` if the service ever fails. Wrap the card body content in a three-state conditional.

In `Components/Pages/NotificationPanel.razor`, replace the bare binding so the card body reads:

```razor
@if (DashboardState.Value.IsLoading)
{
    <p class="text-muted">Loading...</p>
}
else if (DashboardState.Value.ErrorMessage is not null)
{
    <p class="text-danger">@DashboardState.Value.ErrorMessage</p>
}
else
{
    <p class="display-4 text-primary">@DashboardState.Value.NotificationCount</p>
    <p class="text-muted">unread notifications</p>
}
```

For `ProductSummary.razor`, the adapted block (substituting `text-success`, `ProductCount`, and caption "products in catalog"):

```razor
@if (DashboardState.Value.IsLoading)
{
    <p class="text-muted">Loading...</p>
}
else if (DashboardState.Value.ErrorMessage is not null)
{
    <p class="text-danger">@DashboardState.Value.ErrorMessage</p>
}
else
{
    <p class="display-4 text-success">@DashboardState.Value.ProductCount</p>
    <p class="text-muted">products in catalog</p>
}
```

For `UserProfile.razor`, substitute `text-info`, `UserName`, and caption "logged in as" — the shape is identical. Note: `UserName` is a `string` and `NotificationCount` is an `int`, but Razor renders both the same way with `@DashboardState.Value.UserName`. No type-specific handling is needed.

**Why the conditional block matters:**

> The state record carries more than the value. `IsLoading` and `ErrorMessage` are the async-lifecycle properties from Day 5. A component subscribed to shared state should render all three states -- loading, error, and value -- not just the value.
>
> This is the multi-component payoff again: one store update drives the loading spinner, the error message, and the value across all three components at once. They do not each manage their own loading flag -- they read one shared lifecycle.

**FAQ -- "Why not just show the number and skip the loading state?"**
> During the 300ms service delay the number would render as a raw `0`, which looks like a real value of zero. The `IsLoading` branch makes the async gap explicit. The `ErrorMessage` branch does the same for failures -- without it a failed load would silently show stale data.

This is the shape the finished reference ships, and it is the same conditional block used in Practice 1.

---

## Step 5: Run and Demonstrate Multi-Component Update (2 min)

Run the project. Navigate to `/dashboard`.

**What you'll see:**
- Brief loading state (300ms delay from MockDashboardService)
- All three cards populate simultaneously: NotificationPanel shows "5", ProductSummary shows "128", UserProfile shows "Alice Johnson"

**Why this is the payoff:**

> One action dispatched from `Dashboard.razor`. The effect called the service. The reducer updated the state. All three child components re-rendered with the new data.
>
> No parameters passed between components. No events. No callbacks. The parent does not even know which data each child needs. Each component subscribes to the state and picks what it wants.
>
> This is the payoff of centralized state. When multiple components need the same data, the store is the single source of truth.

**FAQ -- "What if only one component needs to update?"**
> If the data is only used by one component, it probably does not belong in the store. Apply the decision framework: is it shared? Does it survive navigation? If not, keep it local.

---

## Step 6: Refactor Service to Result<T> (2 min)

**Action:** Open `Services/IDashboardService.cs`. Change the return type and add the using:

```csharp
using BYSResults;

namespace Day06Demo.Services;

public record DashboardData(int NotificationCount, int ProductCount, string UserName);

public interface IDashboardService
{
    Task<Result<DashboardData>> GetDashboardDataAsync();
}
```

**Action:** Open `Services/MockDashboardService.cs`. Update the method:

```csharp
using BYSResults;

namespace Day06Demo.Services;

public class MockDashboardService : IDashboardService
{
    public async Task<Result<DashboardData>> GetDashboardDataAsync()
    {
        // Simulate network delay
        await Task.Delay(300);

        var result = new Result<DashboardData>();
        return result.WithValue(new DashboardData(
            NotificationCount: 5,
            ProductCount: 128,
            UserName: "Alice Johnson"));
    }
}
```

**What changed:**

> Two changes to the service. The interface return type goes from `Task<DashboardData>` to `Task<Result<DashboardData>>`. The implementation creates a `Result<DashboardData>` container and returns it with `.WithValue()`.
>
> The `using BYSResults;` import brings in `Result<T>` and `Error`. The NuGet package is already installed -- only the using directive is needed.

---

## Step 7: Refactor Effect to Use Result<T> (1 min)

**Action:** Open `Features/Dashboard/Store/DashboardEffects.cs`. Replace the try/catch block in `HandleLoadDashboardAction`:

```csharp
[EffectMethod]
public async Task HandleLoadDashboardAction(
    LoadDashboardAction action, IDispatcher dispatcher)
{
    var result = await dashboardService.GetDashboardDataAsync();

    if (result.IsSuccess)
    {
        var data = result.Value!;
        dispatcher.Dispatch(new LoadDashboardSuccessAction(
            data.NotificationCount, data.ProductCount, data.UserName));
    }
    else
    {
        var errors = result.Errors.Select(e => e.ToString()).ToList();
        dispatcher.Dispatch(new LoadDashboardFailureAction(
            string.Join("; ", errors)));
    }
}
```

**Why the refactor matters:**

> The try/catch is gone. Instead, call the service and check the result. `result.IsSuccess` means the service returned data -- extract it with `result.Value` and dispatch the success action. `result.IsFailure` means something went wrong -- extract the errors and dispatch the failure action.
>
> Behavior has not changed -- the dashboard still loads the same data. But the *contract* has changed. The service explicitly says "here is your data" or "here is what went wrong." No surprise exceptions for business logic failures.

Run the project again to verify everything still works.

**FAQ -- "Do we still need try/catch at all?"**
> Yes, but for different reasons. Try/catch handles *infrastructure* errors -- network timeouts, database connection failures. `Result<T>` handles *business logic* outcomes -- validation failures, not-found conditions. In a real app, the service call would be wrapped in try/catch for infrastructure errors, and the service would return `Result<T>` for business outcomes. They work together.

---

## Step 8: Multi-Error Accumulation Preview (1 min)

**Action:** Review the following pattern -- it is not coded during the demo, just shown for reference:

```csharp
public async Task<Result<Product>> AddAsync(Product product)
{
    var result = new Result<Product>();

    if (string.IsNullOrWhiteSpace(product.Name))
        result.AddError(new Error("Validation", "Product name is required"));
    if (product.Price <= 0)
        result.AddError(new Error("Validation", "Price must be greater than zero"));

    if (result.IsFailure)
        return result;

    // ... save to database ...
    return result.WithValue(product);
}
```

**Why this is different:**

> Notice the key difference. Two validations, two possible errors. Instead of throwing on the first one, the method *collects all the errors* before returning. The caller gets the full picture in one response.
>
> With try/catch, the first validation that fails throws an exception. The user fixes it, submits again, and hits the second validation. With `Result<T>`, both errors come back at once. This is the pattern used in Practice 2's `AddAsync` method.

---

## Transition to Practice

> Three components wired to shared state -- same three lines per component, same store, different data. Then the service layer refactored to use `Result<T>` for explicit success/failure handling.
>
> Practice 1 repeats the multi-component wiring pattern on the student practice project. Practice 2 is the service-to-`Result<T>` refactor with two methods: `GetByIdAsync` (straightforward) and `AddAsync` (multi-error accumulation).
