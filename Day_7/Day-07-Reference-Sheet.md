# Day 7 Reference Sheet -- Route Guards and Navigation with State

**Course:** SDEV 2351 | **Module:** 1 | **CO:** CO1, CO2
**Estimated Reading Time:** ~5 minutes

---

<img src="../assets/day-07-route-guards.svg" alt="Two-band timeline comparing route guard placement. Top band (red) shows the anti-pattern: Navigate to /checkout, Create component, Render HTML (empty checkout shipped), OnAfterRender guard fires too late, Redirect — outcome is a ~50 ms flash of empty checkout. Bottom band (green) shows the correct pattern: Navigate, Create component, OnInitialized guard runs BEFORE the first render with `if (Cart.Items.Count == 0) { Nav.NavigateTo('/shop'); return; }` — outcome is a clean redirect with no flash. Footer key rules: inject IState&lt;T&gt; + NavigationManager, put the check in OnInitialized, always return after redirect." />

**How to read this diagram:** Read the **top band first** — that is the bug students write when they reach for `OnAfterRender`. Notice the red "flash of empty checkout" cell on the right: the page HTML has already shipped by the time the guard runs, so the user sees ~50 ms of content they cannot use. Now read the **bottom band**: the guard is pulled left, before the render step. The three-line template (`if (condition) { NavigateTo; return; }`) is all you write — the router takes care of the rest. **Key rules:** inject `IState<T>` for the condition, inject `NavigationManager` for the redirect, put the check in `OnInitialized`, and **always `return;` after `NavigateTo`** — without it your method keeps running against a component the router is about to abandon.

---

## Key Concepts

- **Route guard** -- logic in `OnInitialized` that checks application state and redirects the user if a condition is not met. The page never renders if the guard redirects.
- **Guard vs. UI hiding** -- guards prevent a page from rendering at all (page-level). UI hiding (`@if`) hides elements but the page still renders (element-level).
- **Guard ingredients** -- every guard injects `IState<T>` (to read the condition) and `NavigationManager` (to redirect). The check runs in `OnInitialized` before the first render.
- **Guards are not security** -- client-side guards control UX flow (preventing confusing navigation), not unauthorized access. Server-side auth middleware (Module 2) provides real access control.

## Syntax Reference

| Guard Component | Purpose | Example |
|-----------------|---------|---------|
| `@inject IState<CartState> CartState` | Read application state | Check cart item count |
| `@inject NavigationManager Navigation` | Redirect on failure | `Navigation.NavigateTo("/shop")` |
| `OnInitialized()` | Lifecycle hook before first render | Guard logic goes here |
| `return` after `NavigateTo` | Stop further execution | Prevents errors after redirect |

### Guard vs. UI Hiding Decision

| Approach | When to Use | Renders Page? |
|----------|-------------|---------------|
| Route Guard | Page-level: user should not see this page | No |
| UI Hiding (`@if`) | Element-level: hide a button or section | Yes |

## Code Pattern

```csharp
// Route guard pattern -- check state before allowing page access
@page "/checkout"
@using Fluxor
@inherits Fluxor.Blazor.Web.Components.FluxorComponent @* Required when page displays state and needs to re-render on changes — not needed for redirect-only guards *@
@inject IState<CartState> CartState
@inject NavigationManager Navigation

<h3>Checkout</h3>
@* Page content renders only if guard passes *@

@code {
    protected override void OnInitialized()
    {
        base.OnInitialized();

        // Guard: redirect if cart is empty
        if (CartState.Value.Items.Count == 0)
        {
            Navigation.NavigateTo("/shop");
            return; // Always return after redirect
        }
    }
}
```

## Extracting Guards for Testability

When guard logic lives inside a component, it can't be unit tested without rendering the component. Extract the decision into a static class:

```csharp
// Guards/CartGuard.cs -- the decision logic
public static class CartGuard
{
    public static bool ShouldRedirect(CartState state)
    {
        return state.Items.Count == 0;
    }
}

// Checkout.razor -- wires the decision to a redirect
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

**Why static?** The guard has no dependencies -- it evaluates state and returns a boolean. Static is simpler than an injected service for pure decision functions. Lab 1's instructor test suite calls your guard's static method directly.

## Testing a Guard

Because the decision is a plain static method, a test builds a state, calls it, and checks a bool -- no component, no rendering, no Fluxor:

```csharp
using Xunit;

public class CartGuardTests
{
    [Fact]
    public void ShouldRedirect_WhenCartEmpty_ReturnsTrue()
    {
        // Arrange
        var state = new CartState { Items = [] };

        // Act
        var shouldRedirect = CartGuard.ShouldRedirect(state);

        // Assert
        Assert.True(shouldRedirect);
    }

    [Fact]
    public void ShouldRedirect_WhenCartHasItems_ReturnsFalse()
    {
        // Arrange
        var state = new CartState
        {
            Items = [new CartItem("Blue Mug", 12.99m, 1)]
        };

        // Act
        var shouldRedirect = CartGuard.ShouldRedirect(state);

        // Assert
        Assert.False(shouldRedirect);
    }
}
```

**Test both directions.** A guard that always returned `true` would still pass the empty-cart test. The false case is what proves the condition is real.

**Naming convention:** `MethodName_Scenario_ExpectedResult` -- the same shape you used for reducer tests on Day 4.

**Run tests:** Test Explorer in Visual Studio (`Test > Test Explorer`, or `Ctrl+E, T`).

## Common Mistakes

| Mistake | Symptom | Fix |
|---------|---------|-----|
| Guard logic in `OnAfterRender` instead of `OnInitialized` | Page briefly flashes before redirect | Move guard to `OnInitialized` |
| Missing `return` after `NavigateTo` | `NullReferenceException` or unexpected behavior after redirect | Add `return` immediately after `NavigateTo` |
| Guard redirects to a page that also has a guard | Infinite redirect loop, browser hangs | Ensure redirect targets are unguarded pages |
| Using guard for element-level visibility | Over-engineering -- the page still renders and then redirects, which is unnecessary | Use `@if` in markup for element-level hiding |

## Quick Check

1. **A checkout page should only be accessible when the cart has items. Route guard or UI hiding?**
   <details><summary>Answer</summary>Route guard -- page-level protection. The user should not see checkout at all if the cart is empty.</details>

2. **What two services does every route guard inject?**
   <details><summary>Answer</summary><code>IState&lt;T&gt;</code> (to read state) and <code>NavigationManager</code> (to redirect).</details>

3. **Why use `OnInitialized` instead of `OnAfterRender` for guards?**
   <details><summary>Answer</summary><code>OnInitialized</code> runs before the first render, so the page never displays. <code>OnAfterRender</code> runs after rendering, causing a visible flash.</details>

## Watch a route guard block an empty-cart checkout in action

**Watch the guard fire before any markup renders.** A 17-second screen recording of the Day 7 demo — click Checkout with an empty cart and get redirected back to /shop before any Checkout markup renders; add two items; click Checkout again and watch the guard pass through to the full cart summary. The closing caption names the contrast with element-level `@if` hiding; for that comparison itself, see the Guard vs. UI Hiding table above.

<video controls preload="metadata" width="720" style="max-width:100%;height:auto;border:1px solid #DEE2E6;">
  <source src="../demo-videos/day-07-route-guard.mp4" type="video/mp4" />
  <track kind="captions" src="../demo-videos/day-07-route-guard.vtt" srclang="en" label="English" default />
  Your browser does not support embedded video. <a href="../demo-videos/day-07-route-guard.mp4">Download the MP4 (17 s, silent, captioned)</a>.
</video>

- Video file: [`day-07-route-guard.mp4`](../demo-videos/day-07-route-guard.mp4) (17 s, silent, 1280×720, H.264)
- Caption track: [`day-07-route-guard.vtt`](../demo-videos/day-07-route-guard.vtt) (WebVTT sidecar — WCAG 1.2.2)
- Full text transcript (screen-reader friendly): [`day-07-route-guard-transcript.md`](../demo-videos/day-07-route-guard-transcript.md) (WCAG 1.2.1)

The video has **no audio**. All narration is in the on-screen captions and the transcript. Nothing in Day 7 assessments requires watching the video — the transcript is a complete alternative.

---

*See also: [MS Learn: Blazor Routing & Navigation](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing) | [Fluxor Docs](https://github.com/mrpmorris/Fluxor/blob/master/Docs/README.md) | Day 7 practice project in Code-Examples/*
