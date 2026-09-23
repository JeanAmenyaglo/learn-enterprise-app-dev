# Day 9 Reference Sheet -- Advanced Routing Patterns

**Course:** SDEV 2351 | **Module:** 1 | **CO:** CO1
**Estimated Reading Time:** ~5 minutes

---

## Key Concepts

- **Fallback route** -- handles URLs that match no `@page` directive at all. **Three pieces, all required:** `UseStatusCodePagesWithReExecute` in `Program.cs`, an ordinary `NotFound.razor` page, **and** `NotFoundPage` on the `<Router>`.
- **`NotFoundPage`** -- the page the *router* renders whenever it has nothing to show. It serves **both** kinds of 404 (see the table below), not just the missing-data one.
- **`NavigationManager.NotFound()`** -- for a URL that *did* match a route, but whose data does not exist (`/products/999`). Your page detects this and calls it; the router renders the page named by `NotFoundPage`.
- **Catch-all parameter** -- `{*paramName}` captures everything after the slash, including nested segments with slashes (e.g., `/docs/api/v2` captures `api/v2`).
- **Route precedence** -- when multiple routes could match, Blazor picks the most specific: literal > constrained parameter > unconstrained parameter > catch-all.

> **`<NotFound>` does not work in a Blazor Web App.** Older tutorials put a `<NotFound>` block inside `<Router>`. The framework ignores it — it is kept only so old code still compiles. The block never runs. Use `NotFoundPage` and the fallback route below.

## Syntax Reference

### The Two Kinds of 404

| | Bad URL (`/nonexistent`) | Missing data (`/products/999`) |
|---|---|---|
| Did a route match? | No -- nothing claimed it | **Yes** -- `{id:int}` matched |
| Who detects it? | ASP.NET Core routing, on the server | **Your page**, after looking up the data |
| What handles it | `UseStatusCodePagesWithReExecute` (`Program.cs`) **+** `NotFoundPage` on `<Router>` | `NavigationManager.NotFound()` **+** `NotFoundPage` on `<Router>` |

Both render the same `NotFound.razor` page and both return a 404 status.

> **`NotFoundPage` is in both columns — that is not a typo.** Your app renders twice: once on the server, once again in the browser when Blazor connects. The middleware fixes the *server* render of a bad URL (and keeps the 404 status). `NotFoundPage` fixes the *browser* render, where the router re-runs against the URL you actually typed and still matches nothing. **Leave `NotFoundPage` out and your styled page appears, then gets overwritten by a bare "Not found".** Leave the middleware out and you never see your page at all.

### Parameter Types

| Syntax | Type | Example URL | Captures |
|--------|------|-------------|----------|
| `{id}` | Single segment | `/products/laptop` | `laptop` |
| `{id:int}` | Constrained | `/products/42` | `42` (int only) |
| `{id:int?}` | Optional | `/products` or `/products/42` | `null` or `42` — component must check for null |
| `{*slug}` | Catch-all | `/docs/api/v2/auth` | `api/v2/auth` |

### Route Precedence (Highest to Lowest)

| Priority | Type | Example |
|----------|------|---------|
| 1 | Literal segment | `@page "/products"` |
| 2 | Constrained parameter | `@page "/products/{id:int}"` |
| 3 | Unconstrained parameter | `@page "/products/{name}"` |
| 4 | Catch-all | `@page "/{*path}"` |
| — | *Fallback route* | *Not a route. Runs when the table above has nothing left.* |

## Code Pattern

### Fallback route (the bad-URL 404) -- all three pieces required

```csharp
// Program.cs -- above app.UseHttpsRedirection();
// Catches the 404 and re-runs the pipeline against /not-found,
// KEEPING the 404 status code. Note: ReExecute, not "ReExecution".
app.UseStatusCodePagesWithReExecute("/not-found");
```

```razor
@* Components/Pages/NotFound.razor -- an ORDINARY routed page. *@
@* It gets MainLayout from DefaultLayout, so the navbar comes along. *@
@* No <LayoutView> wrapper needed. *@
@page "/not-found"

<div class="alert alert-warning mt-4">
    <h3>Page Not Found</h3>
    <p>The page you requested does not exist.</p>
    <a href="/" class="btn btn-primary">Go Home</a>
</div>
```

```razor
@* Components/Routes.razor -- the THIRD piece, and the one people forget. *@
@* The middleware fixed the SERVER render. When Blazor connects, the router *@
@* runs again in the BROWSER against the URL you typed -- still no match.   *@
@* NotFoundPage is what it renders. Without it, Blazor's bare "Not found"   *@
@* overwrites your styled page a moment after it appears.                   *@
<Router AppAssembly="typeof(Program).Assembly"
        NotFoundPage="typeof(Pages.NotFound)">
```

### Missing data (the `/products/999` 404)

`Routes.razor` needs no further change -- the same `NotFoundPage` above now does its second job.

```razor
@* The page detects the missing data and says so. *@
@page "/products/{id:int}"
@inject NavigationManager Navigation

@code {
    [Parameter] public int Id { get; set; }
    private string? _name;

    protected override void OnInitialized()
    {
        if (!Catalog.TryGetValue(Id, out _name))
        {
            Navigation.NotFound();   // renders NotFoundPage, returns 404
        }
    }
}
```

### Catch-all page

```razor
@* Captures flexible URL paths, including nested segments *@
@page "/docs/{*slug}"

<h3>Documentation: @Slug</h3>

@code {
    [Parameter] public string? Slug { get; set; }
}
```

## Common Mistakes

| Mistake | Symptom | Fix |
|---------|---------|-----|
| Adding a `<NotFound>` template to `Routes.razor` | Compiles fine, never runs — you still get the browser's error page | It does not work in a Blazor Web App. Use `NotFoundPage` + the middleware |
| `UseStatusCodePagesWithReExecution` | Does not compile | The method is `UseStatusCodePagesWithReExecute` |
| Middleware line added, but no `NotFound.razor` page | Re-execute finds no page — bare 404 again | Create `NotFound.razor` with `@page "/not-found"` |
| **Middleware added, but no `NotFoundPage` on the `<Router>`** | **Your styled 404 appears, then is replaced by a bare "Not found" a moment later** | **Add `NotFoundPage="typeof(Pages.NotFound)"`. The server render was fine; the browser render had nothing to show** |
| Expecting `NotFoundPage` *alone* to handle bad URLs | `/nonexistent` shows the browser's error page — nothing of your app at all | `NotFoundPage` covers the browser render; the **middleware** is what makes the server render your page. A bad URL needs both |
| Wrapping `NotFound.razor` in `<LayoutView>` | Unnecessary | It is a normal page — `DefaultLayout` already gives it `MainLayout` |
| Using `{slug}` instead of `{*slug}` | Only captures first segment, fails on nested paths | Use `{*slug}` for multi-segment capture |
| Using a *global* catch-all `@page "/{*path}"` as a fallback | Returns 200 for missing pages (a "soft 404") | Use the fallback route — it keeps the 404 status. A *prefix* catch-all (e.g. `/docs/{*slug}`) is fine and coexists with it — the demo uses both this way |

## Quick Check

1. **A user navigates to `/products/42`. You have both `@page "/products/{id:int}"` and `@page "/products/{name}"`. Which matches?**
   <details><summary>Answer</summary>The constrained route <code>{id:int}</code> -- constrained parameters have higher precedence than unconstrained.</details>

2. **What does `{*slug}` capture that `{slug}` does not?**
   <details><summary>Answer</summary>Multiple path segments including slashes. <code>{slug}</code> captures a single segment (e.g., <code>getting-started</code>). <code>{*slug}</code> captures everything (e.g., <code>api/v2/endpoints</code>).</details>

3. **You add a `<NotFound>` template to `Routes.razor`, run the app, and `/nonexistent` still shows the browser's error page. Why?**
   <details><summary>Answer</summary><code>&lt;NotFound&gt;</code> does not work in a Blazor Web App. ASP.NET Core routing resolves the URL on the server before <code>&lt;Router&gt;</code> ever runs, so an unmatched URL never reaches the router. The block compiles and never runs. Use <code>UseStatusCodePagesWithReExecute("/not-found")</code> in <code>Program.cs</code> plus a <code>NotFound.razor</code> page.</details>

4. **`/products/999` returns the Product Detail page for a product that does not exist. Why did the fallback route not catch it, and what does?**
   <details><summary>Answer</summary><code>999</code> is an integer, so the URL satisfies <code>{id:int}</code> and routing matched the page successfully — the server saw no problem. Only the page knows the product is missing. It calls <code>NavigationManager.NotFound()</code>, which renders the page named by <code>NotFoundPage</code> on the <code>&lt;Router&gt;</code> and returns a 404.</details>

---

*See also: [MS Learn: Blazor Routing](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing) | [Exploring Blazor Ch.3](https://learning.oreilly.com/library/view/exploring-blazor-creating/9798868815249/html/487978_3_En_3_Chapter.xhtml) | Day 9 practices in Code-Examples/*
