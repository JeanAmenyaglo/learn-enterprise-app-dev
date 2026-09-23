# Day 6 Reference Sheet -- Centralized State, Component Integration, and the BYSResult Pattern

**Course:** SDEV 2351 | **Module:** 1 | **CO:** CO2
**Pre-Class Due:** BYSResult Pattern Guide (20 min, assigned Day 5)
**Estimated Reading Time:** ~5 minutes

---

<img src="../assets/day-06-store-vs-local.svg" alt="Decision matrix with five criterion rows and two result columns. Left column Goes in the Fluxor Store lists criteria that push state into centralized storage (survives navigation, shared across components, cross-cutting changes, needs audit, needs server sync). Right column Stays local in the component lists opposing criteria (tied to this render, private to one component, single-component changes, pure UI micro-state, transient). Example code cards contrast a [FeatureState] DashboardState record holding NotificationCount/ProductCount/UserName/IsLoading/ErrorMessage against a @code block with private _isDropdownOpen/_showHoverCard/_searchDraft/_activeTab fields. Footer rule: centralization is a cost, not a goal." />

**How to read this diagram:** Work top to bottom, one row at a time. Each criterion is a yes/no question — if your answer matches the left column, the state belongs in the Fluxor store; if it matches the right, keep it in the component. You only need **one clear yes on the right** to keep state local — don't make it a tie-breaker. The two code cards below the criteria show the shape each side produces: a `[FeatureState] public record` on the left, plain `@code { private ... }` fields on the right. **Key rule:** *centralization is a cost, not a goal*. Every store entry costs you a reducer, an action, and a re-render surface — pay that cost only when a second consumer needs the value.

---

> **Companion build guide.** Today you refactor the effect so the service returns a typed `Result<T>` instead of throwing. The **"Make the failure a value — the Result pattern (Day 6)"** part of <strong><a href="Fluxor-Store-Anatomy.html?isCourseFile=true" target="_blank" rel="noopener">Anatomy of a Fluxor Store</a></strong> walks this exact change — the Day-5 `try/catch` effect becoming a `result.IsSuccess` check on a single value.

> **Deciding what belongs in the store.** Today's centralized-vs-local question — *what must the app remember, and where should that state live?* — is the opening move of store design. <strong><a href="Whiteboarding-A-Ride-Share-Trip.html?isCourseFile=true" target="_blank" rel="noopener">Whiteboarding a Ride-Share Trip</a></strong> works that question from requirements, deriving State and Actions before any code — the same reasoning you apply when deciding what goes in the store versus a component.

## Key Concepts

- **Centralized vs local state** -- not all state belongs in the Fluxor store. Put shared data that must survive navigation in the store. Keep pure UI state (dropdown open, tooltip visible) in the component.
- **Multi-component subscription** -- multiple components inject `IState<SameState>` and all re-render when the store updates. No prop drilling, no event callbacks.
- **BYSResult `Result<T>`** -- a service method returns `Result<T>` instead of throwing exceptions. The caller checks `IsSuccess` or `IsFailure` to determine the outcome.
- **Result + Fluxor integration** -- effects check `result.IsSuccess` instead of wrapping service calls in try/catch. Errors from `result.Errors` flow into failure actions.

## Decision Framework: Store vs Local

| Criterion | Centralized (Store) | Local (Component) |
|-----------|--------------------|--------------------|
| Survives navigation? | Yes | No |
| Shared across components? | Yes | No |
| Needs time-travel debug? | Yes | No |
| Pure UI state? | No | Yes |
| Scope of change? | Cross-cutting | Single component |

## BYSResult API Summary

```csharp
// Create a result container
var result = new Result<MyType>();

// Add an error
result.AddError(new Error("Category", "Message"));

// Return success
return result.WithValue(data);

// Return failure (errors already added)
return result;

// Check outcome
if (result.IsSuccess) { var val = result.Value; }
if (result.IsFailure) { var errs = result.Errors; }
```

## Code Pattern: Service Method with Result<T>

```csharp
public async Task<Result<Product>> GetByIdAsync(int id)
{
    var result = new Result<Product>();

    if (id <= 0)
    {
        result.AddError(new Error("Validation", "ID must be greater than zero"));
        return result;
    }

    var product = await _repository.FindAsync(id);
    if (product is null)
    {
        result.AddError(new Error("Not Found", $"Product {id} not found"));
        return result;
    }

    return result.WithValue(product);
}
```

> **Modern C# you'll meet here.** The finished Day 6 code uses a few constructs explained in the course reference <strong><a href="/d2l/le/lessons/184456/units/6193672" target="_blank" rel="noopener">Modern C# You'll Meet Here</a></strong>: the **`with` expression** -- `product with { Id = ... }`, a copy of a record with one field changed (Section 2.2; you first met it on Day 3) -- the **null-forgiving `!`** in `result.Value!` (Section 6.1), and **`is null`** checks (Section 6.2).

## Common Mistakes

| Mistake | Why It Fails | Fix |
|---------|-------------|-----|
| Accessing `result.Value` without checking `IsSuccess` | Value may not be set if the operation failed | Always check `if (result.IsSuccess)` first |
| Putting UI state (dropdown open/closed) in the store | Adds unnecessary complexity; store updates re-render all subscribers | Keep pure UI state in private component fields |
| Forgetting `@inherits FluxorComponent` on subscriber components | Component will not re-render when store updates | Add `@inherits FluxorComponent` to every component that injects `IState<T>` |
| Throwing exceptions for business logic in a Result-based service | Defeats the purpose of the Result pattern | Use `result.AddError()` for business rules; reserve exceptions for truly exceptional cases (network failure, out of memory) |

## Quick Check

1. **A user's shopping cart items -- store or local?**
   <details><summary>Answer</summary>Store -- shared across components (cart badge, checkout page) and must survive navigation.</details>

2. **What method returns a successful Result?**
   <details><summary>Answer</summary><code>result.WithValue(data)</code></details>

3. **In a Fluxor effect, what replaces the catch block when using BYSResult?**
   <details><summary>Answer</summary><code>if (result.IsFailure) { dispatcher.Dispatch(new FailureAction(result.Errors...)); }</code></details>

## Watch three components subscribe to one store in action

**How do siblings share state without prop drilling?** A 16-second screen recording of the Day 6 demo — the Dashboard page dispatches a single LoadDashboardAction, the reducer updates DashboardState, and all three sibling components (NotificationPanel, ProductSummary, UserProfile) re-render at once. Zero parent coordination, zero EventCallback plumbing.

<video controls preload="metadata" width="720" style="max-width:100%;height:auto;border:1px solid #DEE2E6;">
  <source src="../demo-videos/day-06-multi-component-store.mp4" type="video/mp4" />
  <track kind="captions" src="../demo-videos/day-06-multi-component-store.vtt" srclang="en" label="English" default />
  Your browser does not support embedded video. <a href="../demo-videos/day-06-multi-component-store.mp4">Download the MP4 (16 s, silent, captioned)</a>.
</video>

- Video file: [`day-06-multi-component-store.mp4`](../demo-videos/day-06-multi-component-store.mp4) (16 s, silent, 1280×720, H.264)
- Caption track: [`day-06-multi-component-store.vtt`](../demo-videos/day-06-multi-component-store.vtt) (WebVTT sidecar — WCAG 1.2.2)
- Full text transcript (screen-reader friendly): [`day-06-multi-component-store-transcript.md`](../demo-videos/day-06-multi-component-store-transcript.md) (WCAG 1.2.1)

The video has **no audio**. All narration is in the on-screen captions and the transcript. Nothing in Day 6 assessments requires watching the video — the transcript is a complete alternative.

---

*See also: [BYSResults GitHub](https://github.com/thumper631/BYSResults) | [MS Learn: Blazor State Management](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/) | [Fluxor Docs](https://github.com/mrpmorris/Fluxor/blob/master/Docs/README.md) | BYSResult Pattern Guide (pre-class reading) | Day 6 exercises in Code-Examples/*
