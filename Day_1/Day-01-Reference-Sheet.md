# Day 1 Reference Sheet -- Nested Routes, Route Parameters, and Route Constraints

**Course:** SDEV 2351 | **Module:** 1 | **CO:** CO1
**Estimated Reading Time:** ~5 minutes

---

> **Unfamiliar C# syntax today?** <strong><a href="/d2l/le/lessons/184456/units/6193672" target="_blank" rel="noopener">Modern C# You'll Meet Here</a></strong> explains the shapes this day introduces -- file-scoped namespaces -- the `namespace X;` line with no braces (Section 5.1) -- and the global/implicit `using` lines you never see in the file (Section 5.2).

## Key Concepts

- **Nested layouts** connect through `@layout` + `@inherits LayoutComponentBase` -- `MainLayout` contains `AdminLayout`, which contains the page component
- **Route parameters** map URL segments to `[Parameter]` properties; a nullable parameter `{name?}` makes a segment optional
- **Route constraints** validate parameters before the component loads. If validation fails, the router returns a 404. The router does not throw an exception.
- **`_Imports.razor`** placed in a folder applies `@layout` to every routable component in that folder without extra configuration

## Syntax Reference

### Route Constraints

| Constraint | Example | Matches | Rejects |
|------------|---------|---------|---------|
| `int` | {id:int} | 123, -1 | abc, 12.5 |
| `bool` | {active:bool} | true, false | yes, 1 |
| `datetime` | {date:datetime} | 2026-02-10 | not-a-date |
| `decimal` | {price:decimal} | 49.99, 100 | abc |
| `guid` | {token:guid} | a1b2c3d4-... | not-a-guid |
| `alpha` | {name:alpha} | hello | hello123 |
| `min(x)` | {id:int:min(1)} | 1, 100 | 0, -5 |
| `max(x)` | {id:int:max(99)} | 1, 99 | 100 |
| `range(x,y)` | {id:int:range(1,100)} | 1, 50 | 0, 101 |
| `minlength(x)` | {name:minlength(3)} | bob | ab |
| `maxlength(x)` | {code:maxlength(5)} | ABC | ABCDEF |
| `regex(expr)` | {code:regex(...)} | AB123 | abc |
| `required` | {search:required} | any non-empty | (empty) |

Combine multiple constraints with colons: `{id:int:min(1)}` means the value must be an integer AND at least 1.

**When to use constraints vs. validation:** Route constraints control whether the URL matches the route. If the constraint fails, the router returns a 404. For business rules that require error messages or redirects (for example, "product not found in database"), perform validation in `OnParametersSet` instead.

### Layout Directives

| Directive | Purpose |
|-----------|---------|
| `@page "/path"` | Makes a component routable at a URL |
| `@layout AdminLayout` | Wraps this component in the specified layout |
| `@inherits LayoutComponentBase` | Makes this component a layout (provides `@Body`) |

## Code Pattern

```csharp
// Constrained route parameter
@page "/products/{id:int:min(1)}"

<h3>Product #@Id</h3>

@code {
    [Parameter] public int Id { get; set; }
}
```

## Common Mistakes

| Mistake | Symptom | Fix |
|---------|---------|-----|
| Missing `@inherits LayoutComponentBase` on a layout | The page is blank or a compile error occurs because there is no `@Body` property | Add `@inherits LayoutComponentBase` at top of layout file |
| Missing `_Imports.razor` in section folder | Pages render with `MainLayout` only; the router does not apply the nested layout | Create `_Imports.razor` in the folder with `@layout AdminLayout` |
| `[Parameter]` attribute missing on property | The URL value is not bound to the property; the property keeps its default value (`0`, `""`) | Decorate the property: `[Parameter] public int Id { get; set; }` |
| Constraint order wrong (`{min(1):int}`) | The route does not match any URL | Write the type constraint first, then the value constraint: `{id:int:min(1)}` |

## Quick Check

1. **What directive makes one layout render inside another layout?**
   <details><summary>Answer</summary><code>@layout</code> -- placed in the nested layout to specify its parent.</details>

2. **Write a route template that only accepts a positive integer ID.**
   <details><summary>Answer</summary><code>@page "/items/{id:int:min(1)}"</code> -- combines <code>int</code> type constraint with <code>min(1)</code>.</details>

3. **A student navigates to `/products/abc` but the route expects `{id:int}`. What happens?**
   <details><summary>Answer</summary>The URL does not match the route. The router checks other routes. If no route matches, ASP.NET Core returns a bare 404 before Blazor runs, so the browser shows its own error page. Today's project has no styled 404 page; you build one on Day 9. No exception is thrown.</details>

## Pre-Class Reading for Day 2

**None required.** Day 2 builds directly on today's content — nested routes and constraints, extended to multi-parameter routes and `NavigationManager`. No advance reading needed.

The first required reading lands at the end of Day 2 and is due before Day 3.

| Heads-up | Time | Location |
|---------|------|----------|
| **Pre-Lab 1 Environment Check** (ungraded setup check — clone, build, git identity — completed during Day 5 Practice) | ~15 min | [Pre-Lab 1 Environment Check](../common/Pre-Lab-1-Git-Onboarding.html?isCourseFile=true) in Brightspace |
| **Micro-Assessment #1 — Git Branching Workflow** (graded take-home, CO6, 2%; assigned Day 5, due Day 9 — perform the branch→commit→PR→conflict→merge loop in your own repo) | assigned Day 5 | [MA #1 — Git Branching Workflow](../common/MA-01-Git-Branching-Workflow.html?isCourseFile=true) in Brightspace |
| **Git Branching — Your Complete Lab Guide** (the single reference for all Git branching in this course — new material, taught from zero) | read anytime | [Git Branching — Your Complete Lab Guide](Git-Branching-Master-Reference.html?isCourseFile=true) in Brightspace |

---

*See also: [MS Learn: Blazor Routing](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing) | [Exploring Blazor Ch.3](https://learning.oreilly.com/library/view/exploring-blazor-creating/9798868815249/html/487978_3_En_3_Chapter.xhtml) | Day 1 demonstration + practices in Code-Examples/*
