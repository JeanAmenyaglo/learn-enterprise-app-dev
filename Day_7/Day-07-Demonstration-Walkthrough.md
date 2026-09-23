# Demo Walkthrough -- Day 7: Route Guards and Navigation with State -- Cart Checkout Guard

**Purpose:** A comprehensive, step-by-step reference for the Day 7 demo. Use this to prepare before class or to follow along while coding -- it is the detailed companion to the live demonstration.

**Starting point:** The `demo/starter/Day07Demo` project -- a standalone Blazor Web App with a complete Fluxor cart store (state, actions, reducers, feature), a `MockCartService` using `Result<T>`, a Shop page for adding items, and a Checkout page that displays cart contents but has no guard. TODO comments mark the live-code points.

**End state:** A working cart checkout guard that redirects to `/shop` when the cart is empty, using `OnInitialized` and `NavigationManager`. The guard decision logic is extracted into a static `CartGuard` class for testability.

---

## Step 1: Open the Starter Project and Show What's Pre-Built (3 min)

Open `Code-Examples/demo/starter/Day07Demo` in Visual Studio. Press F5 to run.

**What you'll see:**
- The Home page loads with a description of the demonstration and a prompt to try navigating to `/checkout` without adding items
- The Shop page shows 3 products with "Add to Cart" buttons
- The Checkout page displays a table of cart items

**Before coding, open each file briefly to see the pre-built infrastructure:**

1. **`Models/CartItem.cs`** -- `record CartItem(string Name, decimal Price, int Quantity)`. A simple positional record. This is the item type stored in the cart.

2. **`Features/Cart/Store/CartState.cs`** -- Three properties:

   ```csharp
   public record CartState
   {
       public IReadOnlyList<CartItem> Items { get; init; } = [];
       public bool IsLoading { get; init; }
       public string? ErrorMessage { get; init; }
   }
   ```

   The cart state tracks items, loading status, and errors. Same pattern as the `ProductState` from Day 5.

3. **`Features/Cart/Store/CartActions.cs`** -- Three actions: `AddToCart`, `RemoveFromCart`, `ClearCart`. Standard Fluxor action records.

4. **`Features/Cart/Store/CartReducers.cs`** -- The `AddToCart` reducer handles quantity updates for existing items and adds new items. All immutable -- the `with` expression creates a new state record.

5. **`Services/ICartService.cs` / `Services/MockCartService.cs`** -- `Task<Result<List<CartItem>>> GetCartItemsAsync()`. The return type wraps the data in a `Result<T>` -- the same BYSResult pattern as Day 6. No exceptions for expected failures. This is the service pattern used in Lab 1.

   > **Recall from Day 6 -- Result\<T\> Pattern:** The `Result<T>` wrapper replaces try/catch for expected failures. A service returns `Result<T>` instead of throwing exceptions. The caller checks `.IsSuccess` to get `.Value` (the data) or `.IsFailure` to read `.Errors` (the error list). You'll see this pattern again in Module 2 (gRPC error handling) and Module 3 (transaction error handling).

6. **`Components/Pages/Shop.razor`** -- Shows the product list and dispatches `AddToCartAction` when a button is clicked. Cart count appears at the bottom.

7. **`Components/Pages/Checkout.razor`** -- TODO comments mark the current behavior:

   ```razor
   @* ============================================================ *@
   @* DEMONSTRATION (CODE LIVE).                                    *@
   @*                                                               *@
   @* Step 1 — Add the guard to this page ...                       *@
   @* Step 5 — After creating Guards/CartGuard.cs, replace the      *@
   @*   inline check with CartGuard.ShouldRedirect(CartState.Value).*@
   @* ============================================================ *@
   ```

   The checkout page shows cart items, but it always renders -- even with no items. The task in this demo: add a guard that prevents this page from loading when the cart is empty.

> **Why start here:** The entire Fluxor store is pre-built. The only new code in this demo is the guard pattern -- a few lines in `OnInitialized`. This keeps cognitive load on the concept, not on setup.

---

## Step 2: Show the Problem (2 min)

Run the project to see the problem directly:

1. Navigate directly to `/checkout` by typing the URL. The page renders with the "Your cart is empty" warning. The user sees a checkout layout with nothing in it -- confusing UX.

2. Navigate to `/shop`. Add 2 items. Navigate to `/checkout`. Now the items display. The page works fine when items exist, but there is nothing preventing access when the cart is empty.

**Why this is a problem:**

> Right now, nothing stops a user from navigating to `/checkout` with an empty cart. The page renders with a warning message, but the user still sees the checkout UI. In enterprise apps, the correct pattern is to redirect instead -- the user never sees the page.

---

## Step 3: Add the Guard (CODE LIVE, 5 min)

Open `Components/Pages/Checkout.razor`.

**Action:** Add `NavigationManager` injection after the existing `@inject` directives:

```razor
@inject NavigationManager Navigation
```

**Why both injections:**

> `IState<CartState>` is already injected -- that provides the cart data. `NavigationManager` gets added for the redirect. These are the two ingredients every guard needs.

**Action:** Add the `OnInitialized` override in the `@code` block:

```csharp
protected override void OnInitialized()
{
    base.OnInitialized();

    if (CartState.Value.Items.Count == 0)
    {
        Navigation.NavigateTo("/shop");
        return;
    }
}
```

**Why three steps:**

> Three steps. First, call `base.OnInitialized()` -- `FluxorComponent.OnInitialized()` wires up state-change subscriptions; skipping it means the component never re-renders when state updates. Second, check the condition: is the cart empty? Third, if yes, redirect to `/shop` and return immediately.
>
> The `return` is required. Without it, the code that comes after the redirect will also run. The redirect does not stop execution. Accessing `CartState.Value.Items[0]` after the redirect would throw an `IndexOutOfRangeException` because the list is empty. Always return after `NavigateTo`.
>
> Why `OnInitialized` and not `OnAfterRender`? Because `OnInitialized` runs before the first render. The component never outputs any HTML. With `OnAfterRender`, the page would flash on screen for a moment before the redirect fires.

**FAQ -- "Is `@inherits FluxorComponent` required for the guard?"**

> `FluxorComponent` enables automatic re-rendering when state changes. The guard reads state once during initialization -- it does not need to react to changes. However, this page already has `@inherits FluxorComponent` because it displays cart items that change. The guard does not require it, but the display does.

---

## Step 4: Test the Guard (3 min)

Run the project and walk through these three tests:

1. **Empty cart test:** Navigate directly to `/checkout`. The app immediately redirects to `/shop`. The guard checked the cart, found zero items, and redirected before the page rendered.

2. **Full cart test:** Add 2 items on the shop page. Navigate to `/checkout`. The page renders with items. The guard checked the cart, found 2 items, and allowed the page to render normally.

3. **State change test:** Remove all items from the cart (click Remove buttons). Navigate back to `/checkout`. The app redirects again. The guard checks state every time the page loads -- state changed, guard responded.

**Summary:**

> This is the complete guard pattern. Inject state, inject `NavigationManager`, check the condition in `OnInitialized`, redirect if the condition fails. Every route guard follows these same three steps.

---

## Step 5: Discuss Edge Cases (2 min)

**Two edge cases to watch for:**

> **First: infinite redirect loops.** What happens if the guard redirects to a page that also has a guard? The second guard redirects back, the first guard fires again, and the browser hangs. Always ensure the redirect target is an unguarded page.
>
> **Second: forgetting the `return`.** After `NavigateTo`, the current method keeps executing. Navigation does not throw an exception or halt execution. If code after the redirect accesses data that does not exist, runtime errors follow.

**FAQ -- "Are route guards the same as authentication?"**

> No. Guards are UX flow control -- they prevent confusing navigation. A determined user can bypass client-side guards by manipulating the browser. Real security requires server-side middleware, covered starting Day 12 in Module 2. Think of guards like a "Please Wait to Be Seated" sign -- they manage flow, not access.

---

## Step 6: Refactoring for Testability (5 min)

**Why refactor for testability:**

> The guard works. But there is a problem for testing. The decision logic -- "should we redirect?" -- lives inside a Razor component. A unit test cannot call `OnInitialized` without starting Blazor's full rendering pipeline. Extract the decision into a testable static class.

### Step 6a: Create Guards/CartGuard.cs (CODE LIVE)

**Action:** Create a new folder `Guards/` in the project root and add `CartGuard.cs`:

> In a project with several features, the guard usually lives beside its feature (`Features/Cart/Guards/`) — Lab 1's starter is organised that way. This demo has one feature, so a root `Guards/` folder is enough.

```csharp
using Day07Demo.Features.Cart.Store;

namespace Day07Demo.Guards;

public static class CartGuard
{
    public static bool ShouldRedirect(CartState state) =>
        state.Items.Count == 0;
}
```

**Why static:**

> A static class with a static method. It takes `CartState` as input and returns a boolean. No Blazor, no NavigationManager, no component lifecycle -- just a pure decision function.
>
> Static is correct here because the guard has no dependencies. It evaluates state and returns a result. If a guard needed to call an async service -- like checking authentication in Module 2 -- an injected service would be the right choice instead.

### Step 6b: Update Checkout.razor (CODE LIVE)

**Action:** Replace the inline condition in `OnInitialized`:

```csharp
protected override void OnInitialized()
{
    base.OnInitialized();

    if (CartGuard.ShouldRedirect(CartState.Value))
    {
        Navigation.NavigateTo("/shop");
        return;
    }
}
```

**Why the refactor is worth it:**

> Same behavior externally. The component still injects state and `NavigationManager`. But the decision -- "should we redirect?" -- is now delegated to `CartGuard`. The component handles the action (redirecting); the static class owns the decision logic.
>
> Now a unit test can call `CartGuard.ShouldRedirect(new CartState { Items = [] })` and assert the result without touching Blazor. In Lab 1, the instructor test suite calls the guard's static method directly. If the decision logic lives inside the component, the tests cannot reach it.

**FAQ -- "Why not make `CartGuard` an injected service?"**

> Injected services are for classes with dependencies -- they need something from DI like an HTTP client or database context. `CartGuard` has zero dependencies. It takes state in and returns a boolean out. Static is simpler, faster, and just as testable. Save DI for when it is needed.

### Step 6c: Verify (1 min)

Run the project. Confirm the guard behavior is unchanged -- empty cart redirects, full cart renders.

---

## Transition to Practice

> The demo added a guard to Checkout and then extracted it for testability. The guard pattern is always the same three steps: inject state, inject `NavigationManager`, check in `OnInitialized`.
>
> Practice 1 applies the same guard pattern to an admin dashboard -- a role-based guard that redirects non-admin users. Practice 2 has you do what you just watched me do: extract that guard into a static class and write one test against it. Practice 3, the form wizard, is take-home.
>
> Open the student practice project (`starter/Day07Practice`). The checkout cart guard from the demonstration is already wired in there as a worked reference. Search for `TODO` to find every place where code needs to be written.
