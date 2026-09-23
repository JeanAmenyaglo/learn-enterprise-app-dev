# Practice Walkthrough -- Day 9: Advanced Routing Patterns Exercises

**What this is:** A guided companion for your Day 9 practice exercises. Practice 1 is fully guided -- follow every step. Practice 2 gives direction without giving away the answer. Practice 3 is a written quiz with no coding.

**Your project:** Open the `Day09Practice` starter project in Visual Studio. The project has a multi-page app with Home, About, and Contact pages, plus stubs for the routing practices.

**How to find what needs changing:** Search for `TODO` in the project (Ctrl+Shift+F in Visual Studio). Every place you need to write code is marked with a `TODO` comment.

---

## Practice 1: Fallback Route -- Guided (10 min)

> **How this practice runs.** Practice 1 is instructor-led. This is a familiar pattern -- you watched the instructor build it in the demo -- so it runs as a worked launch, not a group walk-through. Wait for the instructor to open `Program.cs` before typing. The instructor adds the `UseStatusCodePagesWithReExecute` line and creates `NotFound.razor` together with the class. Once those pieces are in place, Tasks 1-4 below are yours to complete and verify independently.

**This is the same fallback route the demonstration just built** -- the same one-line middleware call, the same `NotFound.razor` page, the same `NotFoundPage` attribute on the router, the same alert markup with a Go Home button. This practice repeats it in your own project. There is no new pattern here. The value is in your fingers typing the three pieces once, so they are familiar when you need them in Lab 1.

Every web app needs a 404 page. Users type URLs incorrectly. They click old links in emails. They visit pages that were removed during a redesign. Right now those users get the browser's own error page -- no navbar, no message from your app, no way back. A styled "Page Not Found" inside your layout tells them: we noticed, and here is how to recover.

The fallback lives in **three** places, and all three are required:

1. **`Program.cs`** -- `app.UseStatusCodePagesWithReExecute("/not-found");` catches the 404 the server was about to return and re-runs the pipeline against `/not-found`, keeping the 404 status code.
2. **`Components/Pages/NotFound.razor`** -- an ordinary routed page at `@page "/not-found"`. Because it is a normal page, it picks up `MainLayout` automatically. The navbar comes along with no extra work.
3. **`Components/Routes.razor`** -- `NotFoundPage="typeof(Pages.NotFound)"` on the `<Router>`. This is the piece that is easy to forget, and the next paragraph explains why it matters.

Before you start, you need to know what **middleware** is. Middleware is code that runs on every request, in order, before your page renders. ASP.NET Core builds a pipeline of these steps in `Program.cs` -- each `app.UseSomething()` line adds one. `UseStatusCodePagesWithReExecute` adds a step that watches for error status codes on the way back out. When it sees a 404, it re-runs the pipeline against the path you gave it. Your page never had to know anything went wrong.

You also need to know that **your app renders twice.** The server renders the page first and sends the HTML to the browser. Then Blazor connects and renders it **again**, in the browser. The middleware in step 1 fixes the *server* render. The second render is where `NotFoundPage` comes in: the router runs again in the browser, against the URL the user actually typed (`/nonexistent`), and that URL still matches nothing. `NotFoundPage` names the page it should show. Leave it out and Blazor shows its own built-in text instead -- a bare, unstyled **"Not found"** -- which lands on top of your styled page a moment after it appears. You will see your page flash, then disappear. That symptom always means step 3 is missing.

> **Why there is no `<NotFound>` template.** Older Blazor tutorials put a `<NotFound>` block inside `<Router>` in `Routes.razor`. Do not look for one here, and do not add one. In a Blazor Web App the framework ignores it -- it is kept only so old code still compiles, and it never runs. `NotFoundPage` is what replaces it. Code that compiles and silently does nothing is the hardest kind of bug to find, because nothing warns you.

You will know it works when `/nonexistent`, `/xyz`, and `/products/reviews` all render the styled 404 inside the layout with the navbar visible **and it stays on screen**, "Go Home" returns to `/`, and `/`, `/about`, and `/contact` still render normally.

### What's already built

The practice project has these pages working:
- `Home.razor` -- home page at `/` with practice instructions
- `About.razor` -- about page at `/about`
- `Contact.razor` -- contact page at `/contact`
- `Routes.razor` -- the router. **You add one attribute to it in Task 3.**
- `MainLayout.razor` -- navbar with Home, About, Contact links

**The problem:** Run the app and navigate to `/nonexistent`. You get the browser's error page. Your app renders nothing at all -- not even the navbar.

### Task 1: Add the Middleware Line

**File:** `Program.cs`

Find the `EXERCISE 1` TODO comments above `app.UseHttpsRedirection();`. Add this line:

```csharp
app.UseStatusCodePagesWithReExecute("/not-found");
```

The name is `UseStatusCodePagesWithReExecute`. Watch the ending -- **ReExecute**, not "ReExecution."

### Task 2: Create the Fallback Page

**File:** `Components/Pages/NotFound.razor` (new file)

```razor
@page "/not-found"

<PageTitle>Page Not Found</PageTitle>

<div class="alert alert-warning mt-4">
    <h3>Page Not Found</h3>
    <p>The page you requested does not exist.</p>
    <a href="/" class="btn btn-primary">Go Home</a>
</div>
```

Notice there is no `<LayoutView>` wrapper. You do not need one. This is an ordinary routed page, so it gets `MainLayout` from the router's `DefaultLayout` setting, exactly like `About.razor` and `Contact.razor` do.

### Task 3: Tell the Router About the Page

**File:** `Components/Routes.razor`

Find the `PRACTICE 1` TODO comment. Add `NotFoundPage` to the `<Router>` so it reads:

```razor
<Router AppAssembly="typeof(Program).Assembly" NotFoundPage="typeof(Pages.NotFound)">
```

This is the piece that keeps your page on screen. Without it, the browser render (the second one) has nothing to show and Blazor substitutes its own bare "Not found."

Note that `NotFoundPage` names a **type** -- `typeof(Pages.NotFound)` -- not a URL string. `"/not-found"` will not compile here.

### Task 4: Test Your 404 Page

Run the app and navigate to these URLs:

| URL | Expected Result |
|-----|----------------|
| `/nonexistent` | Custom 404 page with "Go Home" link, navbar visible -- and it **stays** |
| `/xyz` | Custom 404 page |
| `/products/reviews` | Custom 404 page |
| `/` | Home page (normal) |
| `/about` | About page (normal) |
| `/contact` | Contact page (normal) |

Wait a second or two on the 404 page before you call it done. The failure this practice is most likely to produce does not appear instantly -- it appears when Blazor finishes connecting.

### Success Criteria

- Unmatched URLs show the styled 404 message inside the app layout (navbar visible)
- The 404 page **stays on screen** -- it does not get replaced by a bare "Not found" a moment later
- The "Go Home" link navigates back to `/`
- All existing pages (`/`, `/about`, `/contact`) still render normally
- The address bar still shows the URL the user typed (`/nonexistent`), not `/not-found`

### Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| **Your 404 page appears, then is replaced by a bare "Not found"** | **`NotFoundPage` is missing from the `<Router>`.** The server render worked; the browser render had nothing to show | **Do Task 3.** Add `NotFoundPage="typeof(Pages.NotFound)"` to the `<Router>` in `Routes.razor` |
| Still getting the browser's error page (nothing of your app at all) | The middleware line is missing, or it sits below `app.MapRazorComponents<App>()` | Put `app.UseStatusCodePagesWithReExecute("/not-found");` above `app.UseHttpsRedirection();` |
| `UseStatusCodePagesWithReExecute` does not compile | Spelling -- it is `ReExecute`, not `ReExecution` | Correct the method name |
| `NotFoundPage="/not-found"` does not compile | It takes a type, not a URL | Use `NotFoundPage="typeof(Pages.NotFound)"` |
| The 404 page renders, but as a bare page with no navbar | `NotFound.razor` is missing its `@page "/not-found"` directive, so the re-execute found no page | Add the `@page` directive at the top |
| Existing pages show the 404 page | Typo in an `@page` directive | Check that `/`, `/about`, `/contact` directives are correct |
| You added a `<NotFound>` block and nothing happened | `<NotFound>` does not work in a Blazor Web App | Remove it. Use the three pieces above instead. |

---

## Practice 2: Catch-All Color Route -- Independent (20 min)

**This is the same catch-all route pattern the demo just built** -- the demo used it for documentation paths; you are applying it to a different set of pages, and adding the lookup layer the demo did not have.

Catch-all routes are useful in documentation systems, CMS pages, help centers, and internal wikis -- anywhere the URL structure is flexible or can go many levels deep. Notion's URL is `notion.so/your-workspace/page-title-and-id` where the whole tail is one parameter. Confluence paths nest three or four levels deep. Catch-all routes are how apps handle flexible content paths without registering a route per page.

The key idea: the **star** in `{*slug}` is what makes a catch-all different from an ordinary parameter. It captures everything after the prefix, slashes included. An ordinary `{slug}` stops at the first `/`. That one character is the whole lesson here.

**The demo built the basic catch-all `Docs` page** that echoed the requested path back to the screen. This practice keeps the same routing shape but does something with the captured value: it looks the slug up and serves different content for known slugs, unknown slugs, and no slug at all.

Your page serves colors. There are three of them, they are already written for you, and you are not being asked to invent any content -- see "What's already built" below. **The routing is the exercise.**

### What's already built

`Components/Pages/Colors.razor` -- the page stub. It has **no `@page` directive**, no parameter, and no display logic. That is your work.

What it *does* already have is the lookup table, fully written, in the `@code` block:

```csharp
private readonly Dictionary<string, string> _knownColors = new(StringComparer.OrdinalIgnoreCase)
{
    ["red"] = "#FF0000",
    ["green"] = "#00FF00",
    ["blue"] = "#0000FF"
};
```

This dictionary is **pre-built on purpose. Do not rewrite it.** Building a dictionary is not what Day 9 teaches -- you already know how to do that. It stands in for a database lookup: a real app would query a table of colors here. The **key** is the slug taken from the URL. The **value** is that color's hex code.

| Slug (the key) | Hex code (the value) |
|---|---|
| `red` | `#FF0000` |
| `green` | `#00FF00` |
| `blue` | `#0000FF` |

`StringComparer.OrdinalIgnoreCase` is already passed to the constructor, so `/colors/RED` and `/colors/red` both find the same entry. You do not need to handle casing yourself.

### Task 1: Add the Route and the Parameter

Open `Components/Pages/Colors.razor`.

- Add **one** `@page` directive with a **catch-all** parameter, so that `/colors/red` reaches this page.
- Add a `[Parameter]` property to capture the slug.

**Hint:** the catch-all syntax is a star prefix -- `{*paramName}`.

**One directive is enough, and here is the part worth understanding.** A catch-all also matches **zero** segments. So the bare path `/colors` -- nothing after it at all -- still reaches this page, and the slug simply arrives as **null**. You do not need a second `@page "/colors"` directive, and you should not add one.

That is exactly why the property has to be `string?` rather than `string`. The framework has nothing to give it on `/colors`, so it hands you null, and your first branch is what handles that.

### Task 2: Add the Three Display Branches

The page has three states. Write them in this order, because the checks build on each other:

1. **No slug, or the slug is `"index"`** -- list all three colors as links to `/colors/<name>`.
2. **The slug IS a known color** -- show the color's name, its hex code, and a swatch (a `<div>` with `background-color` set to the hex value).
3. **The slug is NOT a known color** -- show a "Color Not Found" message, and **print the captured slug on screen**. Do not skip that last part; the next task depends on seeing it.

**Hint:** check `string.IsNullOrEmpty(Slug)` first, then use `TryGetValue` on `_knownColors`. `TryGetValue` returns `true` and hands you the value when the key exists, so it does branch 2 and branch 3 in a single call.

### Task 3: See What the Star Actually Bought You

Navigate to **`/colors/red/dark`** -- a path with *two* segments after `/colors`.

The page shows "Color Not Found", and the slug it prints back is **`red/dark`** -- the whole path, both segments, captured into one string. That is the catch-all doing its job.

Now understand the alternative. If you had written an ordinary `{slug}` instead of `{*slug}`, that same URL would **not have matched this page at all**. You would have landed on the 404 page you built in Practice 1. The star is the only reason `/colors/red/dark` reaches `Colors.razor`.

This is the difference the whole practice exists to show you. A one-segment route and a catch-all route look almost identical on screen until a URL arrives with a slash in it.

### Bonus: Add the Nav Link

`Components/Layout/MainLayout.razor` has a comment marking where the Colors nav link goes. Add a `<NavLink>` for `/colors` next to Home, About, and Contact.

### Success Criteria

| URL | Expected Result |
|-----|----------------|
| `/colors` | Lists red, green, and blue as links (the catch-all matched **zero** segments, so `Slug` is null -- one directive, no second `@page` needed) |
| `/colors/index` | Same list (the slug `index` is handled by the catch-all -- no separate route needed) |
| `/colors/red` | Shows `red`, `#FF0000`, and a red swatch |
| `/colors/BLUE` | Shows `BLUE`, `#0000FF`, and a blue swatch (case-insensitive lookup) |
| `/colors/purple` | "Color Not Found", printing the slug `purple` |
| `/colors/red/dark` | "Color Not Found", printing the slug **`red/dark`** -- the catch-all captured both segments |
| `/nonexistent` | Still the Practice 1 fallback 404 page |

### Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| `/colors/red/dark` gives the 404 page instead of "Color Not Found" | You wrote `{slug}` instead of `{*slug}` | Add the star. Without it the route only matches one segment, so a two-segment URL never reaches the page |
| `/colors` throws a null reference | The slug is null on the bare path, and branch 1 does not run first | A catch-all matches zero segments, so `/colors` arrives with `Slug == null`. Check `string.IsNullOrEmpty(Slug)` **before** the dictionary lookup |
| `/colors/index` says "Color Not Found" | The `"index"` slug is not handled in branch 1 | Branch 1 fires on null/empty **or** `Slug == "index"` |
| The swatch `<div>` is invisible | No width/height on the div | A `<div>` with a background color but no size renders as nothing. Give it a width and a height |
| Lookup fails on `/colors/RED` | You built your own dictionary without the comparer | Use the pre-built `_knownColors` -- it already has `StringComparer.OrdinalIgnoreCase` |

---

## Practice 3: Routing Consolidation Quiz -- Written (10 min)

Real codebases have dozens of routes, and the first day you add a route and it "doesn't work" you'll suspect Blazor before you suspect yourself. The actual cause is almost always precedence: a more-specific existing route is catching the traffic before your new one gets a chance. Experienced engineers find the problem by reading each route template in order and applying the precedence rules — without running the app, pressing F5, or using a debugger.

The key idea: precedence is **literal > constrained parameter > unconstrained parameter > catch-all**, and the fallback route sits after all of them. Deterministic. A constrained `{id:int}` beats an unconstrained `{name}` because the constraint is more specific. A catch-all only gets what nothing else claims. The fallback route is the absolute last resort, after every other route has refused.

Note that the fallback route is not itself a route. It is what happens when the route table has nothing left to offer, which is why it never competes for a URL and never steals one.

**The demo demonstrated this in Step 5** with a four-row precedence table — `/products` to literal, `/products/42` to constrained, `/docs/api/v2/auth` to catch-all, `/anything/else` to the fallback route. This quiz is a pattern-matching exercise: eight URLs, eight route templates — predict which route handles each URL without running the app.

You'll know it worked when you can look at any URL and the route table together and call the match in one beat — not by elimination, but by knowing the precedence order. The eight questions cover every shape you'll see in real code: literal collisions, optional parameters, nested literals, catch-all multi-segment paths.

No coding for this practice. Navigate to `/quiz` in the practice app to see the questions on screen, or use the table below.

### Instructions

Match each URL to the route template that would handle it. Consider route precedence: literal > constrained parameter > unconstrained parameter > catch-all.

**Available route templates:**
- `@page "/products"`
- `@page "/products/{id:int}"`
- `@page "/products/{name}"`
- `@page "/docs/{*slug}"`
- `@page "/settings/{id:int?}"`
- `@page "/products/{id:int}/reviews"`
- `@page "/blog/{*slug}"`
- Fallback route (`NotFound.razor`, via the middleware)

### Questions

| # | URL | Your Answer |
|---|-----|-------------|
| 1 | `/products` | |
| 2 | `/products/42` | |
| 3 | `/products/laptop` | |
| 4 | `/docs/api/v2/auth` | |
| 5 | `/settings` (no id) | |
| 6 | `/nonexistent` | |
| 7 | `/products/42/reviews` | |
| 8 | `/blog/2026/02/my-post` | |

<details>
<summary>Answers (check after attempting)</summary>

1. `@page "/products"` -- literal route, highest precedence
2. `@page "/products/{id:int}"` -- constrained parameter, `42` is an integer
3. `@page "/products/{name}"` -- unconstrained parameter, `laptop` is not an integer
4. `@page "/docs/{*slug}"` -- catch-all, captures `api/v2/auth`
5. `@page "/settings/{id:int?}"` -- optional parameter, `id` is null
6. Fallback route (`NotFound.razor`) -- no route matches `/nonexistent`, so the middleware re-executes against `/not-found`
7. `@page "/products/{id:int}/reviews"` -- nested literal with constrained parameter
8. `@page "/blog/{*slug}"` -- catch-all, captures `2026/02/my-post`
</details>

---

## What You Learned

After completing these practices, you can:
- Add a fallback route (`UseStatusCodePagesWithReExecute` + `NotFound.razor` + `NotFoundPage` on the `<Router>`) to handle unmatched URLs gracefully
- Explain why a `<NotFound>` template does nothing in a Blazor Web App
- Use catch-all route parameters (`{*slug}`) to capture flexible multi-segment paths
- Explain what the star adds: without it, a URL with a slash in it never reaches the page at all
- Route a captured slug through a lookup to serve known, unknown, and empty paths differently
- Predict which route template matches a given URL based on precedence rules

**Next day preview:** Day 10 is Review + Lab Prep + Team Formation. No new instruction. You will form teams, review all Module 1 concepts, and start planning your Lab 1 architecture. Come prepared with questions about the Lab 1 spec and ideas about which features you want to own.
