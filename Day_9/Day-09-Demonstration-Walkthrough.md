# Demo Walkthrough -- Day 9: Advanced Routing Patterns -- Fallback Route and Catch-All Route

**Purpose:** A comprehensive, step-by-step reference for the Day 9 demo. Use this to prepare before class, to follow along while coding, or as a detailed guide when your instructor's abbreviated demo script needs more context.

**Starting point:** The `demo/starter/Day09Demo` project -- a Blazor Web App with Home, Products list, and Product Detail pages. No fallback route -- navigating to an unmatched URL gives you the browser's own error page.

**End state:** The app handles **two different kinds of 404**: a bad URL that matches no route at all (`/nonexistent`), and a good URL whose data does not exist (`/products/999`). It also captures flexible documentation paths via a catch-all `Docs.razor` page. Route precedence is demonstrated live.

---

## Step 1: Open the Starter Project and See the Real Problem (2 min)

Open `Code-Examples/demo/starter/Day09Demo` in Visual Studio. Press F5 to run.

**Before coding, open each file briefly to see the pre-built infrastructure:**

1. **`Components/Pages/Home.razor`** -- Contains the prompt text. This is the multi-page app: Home, Products, Product Detail.

2. **`Components/Pages/Products.razor`** -- `@page "/products"` and a product list. A literal route -- `/products` matches exactly this page. Each product links to a detail page.

3. **`Components/Pages/ProductDetail.razor`** -- `@page "/products/{id:int}"`. A constrained route from Day 2. Only integers match. `/products/laptop` would NOT match this page.

4. **`Components/Routes.razor`** -- Has a `<Found>` template and nothing else. When a URL matches, it renders here. When nothing matches... we are about to find out.

Now navigate to `/nonexistent`.

**What you actually see:** the browser's own error page. Not a blank content area -- no navbar, no layout, nothing of ours at all. The app might as well not exist.

**Why this happens -- and it is the important idea of the whole step:**

> ASP.NET Core routing resolves the URL **on the server**, before Blazor's `<Router>` component ever runs. `/nonexistent` matches no endpoint, so the server returns a bare 404 and stops. Our Blazor app never gets a chance to render anything -- not even the layout.
>
> This is why there is no `<NotFound>` template in `Routes.razor`. If you have seen `<NotFound>` in older Blazor tutorials, it belonged to the previous hosting model. In a **Blazor Web App** it still compiles, and it never runs: the framework ignores it entirely, and it is kept only so old code still builds. Code that compiles and silently does nothing is one of the hardest kinds of bug to find, because there is no error message to search for. Do not reach for it; `NotFoundPage` (Step 2) is what replaces it.
>
> The first half of the fix has to live where the decision is actually made: in the middleware pipeline.

---

## Step 2: Add the Fallback Route (CODE LIVE, 4 min)

Three pieces: a page to show, a line to route to it, and one attribute so the page *stays* on screen. All three are required -- leave any one out and the fallback does not work.

**Action 1 -- Create `Components/Pages/NotFound.razor`:**

```razor
@page "/not-found"

<PageTitle>Page Not Found</PageTitle>

<div class="alert alert-warning mt-4">
    <h3>Page Not Found</h3>
    <p>The page you requested does not exist.</p>
    <a href="/" class="btn btn-primary">Go Home</a>
</div>
```

**Action 2 -- Open `Program.cs` and add one line above `app.UseHttpsRedirection();`:**

```csharp
app.UseStatusCodePagesWithReExecute("/not-found");
```

**Action 3 -- Open `Components/Routes.razor` and add `NotFoundPage` to the `<Router>`:**

```razor
<Router AppAssembly="typeof(Program).Assembly" NotFoundPage="typeof(Pages.NotFound)">
```

**Why all three are needed -- this is the heart of the step:**

> `NotFound.razor` is an **ordinary routed page** -- not a special template. Because it is a normal page, it picks up `MainLayout` from `DefaultLayout` automatically, so the navbar appears without any extra work. There is no `<LayoutView>` wrapper to remember.
>
> `UseStatusCodePagesWithReExecute` catches the 404 the server was about to return, then **re-runs the pipeline** against `/not-found` -- while keeping the 404 status code. The user sees our styled page; the browser still correctly hears "404, that thing is not there."
>
> `NotFoundPage` is the piece people leave out, and leaving it out is the confusing one. Your app renders **twice**. The middleware fixes the first render -- the one the server sends. Then Blazor's circuit connects, and the router runs **again in the browser**, this time against the URL the user actually typed: `/nonexistent`. It still matches nothing. `NotFoundPage` is what the router renders when that happens. Without it, Blazor falls back to its own built-in text -- a bare, unstyled **"Not found"** -- which overwrites your page a moment after it appears.
>
> Note that the URL in the address bar does not change. The user still sees `/nonexistent`, which is what you want -- they can read their typo and fix it. That is also *why* the second render needs `NotFoundPage`: the router is still looking at the bad URL.

Run the app. Navigate to `/nonexistent`.

> The styled 404 renders inside the app layout. The navbar is back. The user can click "Go Home" or use the nav links -- and the page **stays put**.

**If you see the styled page flash and then get replaced by a bare "Not found":** you added the middleware but not `NotFoundPage`. That is Action 3. The server render was right; the browser render had nothing to show.

Navigate to `/` and `/products` to confirm they still work normally.

> Existing routes are unaffected. The fallback only fires when nothing matched.

**FAQ -- "Can we show the user the URL they tried to reach?"** Yes, and it is easy now: `NotFound.razor` is a real component, so inject `NavigationManager` and read `Navigation.Uri`. That was awkward with the old `<NotFound>` template -- another quiet advantage of the page-based approach.

**FAQ -- "Why keep the 404 status code? The user sees a nice page either way."** Search engines, monitoring tools, and API clients read the status code, not the page. A "soft 404" -- a friendly page served with a 200 -- tells Google the page exists and should be indexed. The status code is the machine-readable truth.

---

## Step 3: Create Docs.razor Catch-All Page (CODE LIVE, 4 min)

**Action:** Open `Components/Pages/Docs.razor` and replace the TODO comments with:

```razor
@page "/docs/{*slug}"

<h3>Documentation</h3>

<p>Requested path: <code>@Slug</code></p>

@code {
    [Parameter] public string? Slug { get; set; }
}
```

**Why the star matters:**

> This is a catch-all route. The star in `{*slug}` tells Blazor to capture everything after `/docs/` -- including nested slashes. A regular `{slug}` would only capture one segment.
>
> The parameter is `string?` because navigating to just `/docs/` with nothing after it yields null or an empty string.

Run the app.

Navigate to `/docs/getting-started`. The page displays "getting-started."

> Single segment -- works just like a regular parameter.

Navigate to `/docs/api/v2/endpoints`. The page displays "api/v2/endpoints."

> Multiple segments with slashes -- this is what makes catch-all special. A regular `{slug}` parameter would fail here because it only captures up to the first slash. The catch-all captures the entire remaining path.

Navigate to `/docs/`. The slug is null or empty.

> Edge case -- nothing after the prefix. The component needs to handle this, usually with a default view or redirect.

**FAQ -- "When would this be used in a real app?"** Documentation systems, CMS pages, help centers -- anywhere the URL structure is flexible or unknown at compile time. A company wiki with `/wiki/{*page}` lets users create pages with any path depth. It also works for URL rewriting -- catching old URLs and redirecting to new ones.

**FAQ -- "What about `/docs` without a trailing slash?"** It already works. A catch-all matches **zero** segments, so the bare `/docs` reaches `Docs.razor` with `Slug` set to null -- the same as `/docs/`. You do **not** need a second `@page "/docs"` directive, and adding one is redundant. This is precisely why `Slug` is declared `string?` and not `string`: on `/docs` the framework has nothing to give it.

---

## Step 4: The Second Kind of 404 (CODE LIVE, 4 min)

Everything so far handles a URL that matched **nothing**. Now consider `/products/999`.

Navigate there. You get the Product Detail page, cheerfully reporting product 999 -- a product that does not exist.

**Why the fallback route did not save us:**

> `999` is an integer, so `/products/999` **satisfies the `{id:int}` constraint**. Routing matched `ProductDetail.razor` and returned 200. The middleware from Step 2 never saw a problem, because as far as the server is concerned, there wasn't one. The URL is fine. The **product** is what's missing.
>
> Only the page itself knows the difference. So the page has to be the one to say so.

Good news: the router already knows where to send them. `NotFoundPage` went on in Step 2, and it is about to do its **second** job -- no change to `Routes.razor` needed here.

**Action -- Open `Components/Pages/ProductDetail.razor` and make it look up the product:**

```razor
@page "/products/{id:int}"
@inject NavigationManager Navigation

<h3>Product Detail</h3>

<p>Viewing product: <strong>@_name</strong> (ID @Id)</p>
<a href="/products" class="btn btn-outline-primary">Back to Products</a>

@code {
    [Parameter] public int Id { get; set; }

    private static readonly Dictionary<int, string> Catalog = new()
    {
        [1] = "Laptop",
        [2] = "Mouse",
        [3] = "Keyboard",
        [4] = "Monitor"
    };

    private string? _name;

    protected override void OnInitialized()
    {
        if (!Catalog.TryGetValue(Id, out _name))
        {
            Navigation.NotFound();
        }
    }
}
```

Run the app. Navigate to `/products/2` -- "Mouse" renders normally. Navigate to `/products/999`.

> The same styled 404 page appears, and the response carries a 404 status. One page, `NotFound.razor`, now serves both kinds of "not there."

**The two kinds of 404, side by side -- this is the distinction to hold on to:**

| | Bad URL (`/nonexistent`) | Missing data (`/products/999`) |
|---|---|---|
| Did a route match? | No -- nothing claimed it | **Yes** -- `{id:int}` matched |
| Who detects it? | ASP.NET Core routing, on the server | **Your page**, after looking up the data |
| What handles it? | `UseStatusCodePagesWithReExecute` in `Program.cs` **+** `NotFoundPage` on the `<Router>` | `NavigationManager.NotFound()` **+** `NotFoundPage` on the `<Router>` |

> They are genuinely different problems, detected by different things. The middleware cannot know that product 999 is missing -- only your code can. That part is the real distinction, and it is what the table is for.
>
> But notice what the bottom row is telling you: **`NotFoundPage` appears in both columns.** It is not the missing-data tool. It is simply the page the *router* renders whenever it has nothing to show -- whether that is because your code called `NavigationManager.NotFound()`, or because the browser-side router ran against a URL that matches nothing. Both kinds of 404 end up needing it.
>
> The middleware is still not optional. It is what makes the **server** render the page at all, and it is what preserves the 404 status code. Delete it and `/nonexistent` returns a bare 404 before Blazor ever loads -- browser error page, `NotFoundPage` or no `NotFoundPage`.
>
> **So: two kinds of 404, two detectors, one shared page -- and for a bad URL you need both tools, not one.**

**FAQ -- "Isn't this what an exception is for?"** No. A missing product is an expected outcome of a valid request, not an exceptional condition. `NotFound()` produces the correct HTTP semantics (404) without the cost and noise of throwing.

---

## Step 5: Show Precedence in Action (2 min)

Navigate through URLs to demonstrate the precedence rules from the lecture:

| URL | What Matches | Why |
|-----|-------------|-----|
| `/products` | Products.razor | Literal route -- highest precedence |
| `/products/42` | ProductDetail.razor | Constrained parameter `{id:int}` -- second highest |
| `/docs/api/v2/auth` | Docs.razor | Catch-all `{*slug}` -- lowest route precedence |
| `/anything/else` | Fallback route (`NotFound.razor` via middleware + `NotFoundPage`) | No route matches at all |

**Why precedence matters:**

> `/products` hits the literal route. `/products/42` hits the constrained parameter. The catch-all only gets what nothing else claims. More specific routes always win -- deterministic and predictable. There is no risk of a catch-all stealing traffic from a specific page.
>
> And the fallback is the true last resort: it fires only when **no route matches at all**, not even a catch-all. Note where the fallback sits -- it is not a route competing in this table. It is what happens after the table has nothing left to offer.

---

## Transition to Practice

> The demo showed two kinds of 404 and the catch-all route that sits between them. The fallback route catches bad URLs at the middleware level. `NavigationManager.NotFound()` handles the URL that matched but whose data is gone. The catch-all page provides a full component with access to the path -- useful for dynamic content systems.
>
> **Practice 1:** Add a fallback route to the practice project -- guided, the same three pieces as Step 2 above.
>
> **Practice 2:** Build a catch-all colors page with a pre-built lookup dictionary -- the same `{*slug}` mechanic as the demo's `/docs`, applied to a different set of pages -- independent, requires handling lookup logic and the "not found" case. Note it handles its miss with an **in-page message**, not a 404 page: a third valid choice, and worth asking yourself why.
>
> **Practice 3:** A written quiz on route precedence -- no coding, just pattern matching.
