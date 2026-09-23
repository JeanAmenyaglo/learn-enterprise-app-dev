# Day 2 Reference Sheet -- Multiple Route Parameters, NavigationManager, and Programmatic Navigation

**Course:** SDEV 2351 | **Module:** 1 | **CO:** CO1
**Estimated Reading Time:** ~5 minutes

---

## Key Concepts

- **Multiple route parameters** allow a single URL to contain several values (for example, a category and an ID). Each URL value needs a matching `[Parameter]` property in the component
- **NavigationManager** is a built-in Blazor service that moves users between pages using C# code instead of using HTML link clicks
- **Wizard-style flows** use `NavigateTo()` so your C# code controls which page loads next, based on user actions or form state
- **`replace: true`** replaces the current browser history entry -- use this option with back buttons to prevent repeated forward-and-back navigation loops

## Syntax Reference

### NavigationManager Methods

| Method / Property | What It Does |
|-------------------|--------------|
| `NavigateTo("/path")` | Client-side navigation. Fast, and keeps the Blazor circuit alive |
| `NavigateTo("/path", forceLoad: true)` | Full page reload from the server -- use only for non-Blazor pages or external URLs |
| `NavigateTo("/path", replace: true)` | Client-side navigation, but replaces the current history entry |
| `Navigation.Uri` | Returns the current absolute URI (not used in today's exercises — future reference) |
| `Navigation.BaseUri` | Returns the application base URI (not used in today's exercises — future reference) |

### Multiple Route Parameters

```
@page "/products/{category:alpha}/{id:int:min(1)}"
```

Each URL segment corresponds to a separate `[Parameter]` property. Constraints are validated independently -- if any segment does not match its constraint, the entire route returns a 404.

### Constraints Used Today

| Constraint | Example | Matches | Rejects |
|------------|---------|---------|---------|
| `int` | `{id:int}` | 123 | abc, 12.5 |
| `alpha` | `{category:alpha}` | electronics | home-garden, 123 |
| `min(x)` | `{id:int:min(1)}` | 1, 100 | 0, -5 |
| `range(x,y)` | `{page:int:range(1,3)}` | 1, 2, 3 | 0, 4 |

Chain constraints with colons: `{id:int:min(1)}` means the value must be an integer **and** at least 1. The full constraint list is on the Day 1 reference sheet.

## Code Pattern

```csharp
// Multi-param route with programmatic navigation
@page "/products/{category:alpha}/{id:int:min(1)}"
@inject NavigationManager Navigation

<h3>@Category -- Product #@Id</h3>
<button class="btn btn-primary" @onclick="GoToNext">
    Next Product
</button>

@code {
    [Parameter] public string Category { get; set; } = string.Empty;
    [Parameter] public int Id { get; set; }

    private void GoToNext()
    {
        Navigation.NavigateTo($"/products/{Category}/{Id + 1}");
    }
}
```

## Common Mistakes

| Mistake | Symptom | Fix |
|---------|---------|-----|
| Missing `@inject NavigationManager` | `NullReferenceException` at runtime when calling `NavigateTo()` | Add `@inject NavigationManager Navigation` at the top of the component |
| Using `forceLoad: true` everywhere | Page flickers, state is lost, and navigation is slow | Remove `forceLoad`. The default behavior (client-side navigation) is correct for Blazor pages |
| Only one `[Parameter]` for a two-parameter route | Second parameter keeps its default value (`0` or `""`) | Add a `[Parameter]` property for every `{segment}` in the route template |
| Back button creates infinite loop | Browser back button returns to the page the user navigated away from | Use `NavigateTo(path, replace: true)` for back buttons in wizard flows |
| NavigateTo path doesn't match `@page` route | 404 page, no error | Compare your `NavigateTo` string with the target component's `@page` directive |

## Quick Check

1. **How do you inject NavigationManager into a component?**
   <details><summary>Answer</summary><code>@inject NavigationManager Navigation</code> -- the Blazor framework registers this service automatically.</details>

2. **What does `replace: true` do in `NavigateTo("/page", replace: true)`?**
   <details><summary>Answer</summary>It replaces the current entry in the browser's history stack instead of adding a new one. The browser's back button skips the replaced page.</details>

3. **A route is defined as `/orders/{status:alpha}/{id:int}`. Write the two `[Parameter]` properties.**
   <details><summary>Answer</summary><code>[Parameter] public string Status { get; set; } = string.Empty;</code> and <code>[Parameter] public int Id { get; set; }</code></details>

## Pre-Class Reading for Day 3

| Reading | Time | Location |
|---------|------|----------|
| **1. Delegates and Events Refresher** | ~16 min | Linked from the Day 3 page in Brightspace (Pre-Class Preparation section) |
| **2. Bridge to Fluxor** | ~10 min | Same location |
| **3. Fluxor Quick Reference** | ~30 min | Same location |
| **Total** | **~56 min** | |

Read all three before Day 3. The Refresher reviews SDEV 2301 delegates and events -- skim it if they are fresh. The Bridge explains how events map to actions and handlers map to reducers. The Quick Reference covers store setup, actions, reducers and effects -- **keep it open during Days 3-6 and Lab 1.**

---

*See also: [MS Learn: Blazor Routing](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing) | [Exploring Blazor Ch.3](https://learning.oreilly.com/library/view/exploring-blazor-creating/9798868815249/html/487978_3_En_3_Chapter.xhtml) | Day 2 practices in Code-Examples/*
