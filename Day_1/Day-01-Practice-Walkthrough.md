# Practice Walkthrough -- Day 1: Routing Practices

**What this is:** A companion for your Day 1 practices. Practice 1 walks you through every file and step — it names which directive each file needs and why, and you recall the directive itself from the demonstration you just watched. Practices 2 and 3 give you the same kind of direction with less structure.

**Your project:** Open the `Day01Practice` starter project in Visual Studio. The navigation links are already wired up -- they return a bare 404 until you add the correct routing directives. A "404" in this project is your browser's own error page, not a styled page of ours: this app has no fallback page yet (you build one on Day 9).

**How to find what needs changing:** Search for `TODO` in the project (Ctrl+Shift+F in Visual Studio). Every place you need to write code is marked with a numbered `TODO` comment that tells you what to do.

> **Before you start:** Practice 1 is the same nested layout chain your instructor just demonstrated with the Admin section. If you want to review that first, see <a href="Day-01-Demonstration-Walkthrough.html?isCourseFile=true" target="_blank" rel="noopener">Day 1 — Demonstration Walkthrough</a>.

---

## Practice 1: Reports Section (worked launch, 20 min)

A back-office reporting app — the kind a finance team uses to pull sales numbers, check inventory snapshots, and review audit trails. Every section has its own sidebar (date pickers, report categories, export buttons), but the whole app shares a single outer chrome with the company logo, main nav, and user menu.

Pasting sidebar markup into every page creates a maintenance problem. The first time a designer rebrands the app, every page needs updating. Layout chaining writes the Reports sidebar *once*, nests it inside the site-wide `MainLayout`, and every page in the Reports folder picks up both — without a single layout reference on the individual pages.

**You just saw this exact 7-directive pattern in today's demo** with the Admin section (blue theme): `@inherits LayoutComponentBase`, `@layout MainLayout`, `@Body`, the folder-level `_Imports.razor`, and three `@page` routes. Reports uses the same steps, green theme. That's why this walkthrough names *which* directive each file needs and *why*, but doesn't print the directive — recall it from the demo.

You'll know it works when `/reports/overview` renders with both sidebars (dark main + green Reports), navigating between report pages swaps only the content, and clicking Home makes the Reports sidebar disappear. Sidebar markup, CSS, and page content are pre-built — you add the 7 directives. If you get stuck, <a href="Day-01-Demonstration-Walkthrough.html?isCourseFile=true" target="_blank" rel="noopener">Day 1 — Demonstration Walkthrough</a> has the full Admin version to compare against.

### What you're building

When you're done, navigating to `/reports/overview` will show:
- The **main sidebar** (dark, on the far left) -- from `MainLayout`
- A **Reports sidebar** (green, next to it) -- from `ReportsLayout`
- The **page content** (overview cards) -- from `ReportsOverview.razor`

> **How this practice runs.** This is a *worked launch* — your instructor adds `@inherits LayoutComponentBase` to `ReportsLayout.razor` on screen first, so you see exactly where the directive goes. After that, you complete the remaining six TODO markers independently. The pattern is identical to the Admin section you just watched: same 7 directives, different names and colour scheme.

### Steps 1-2: Make ReportsLayout a real layout

**File:** `Components/Layout/ReportsLayout.razor`

The file already has the sidebar markup built for you. At the very top, the TODO comments mark where **two directives** go — the same pair you saw at the top of `AdminLayout` in the demonstration:

- One directive turns a regular component into a layout. Without it there is no `@Body` property and no way to render child pages inside it.
- One directive nests `ReportsLayout` inside `MainLayout`, so a report page renders through the chain `MainLayout → ReportsLayout → your page`.

Recall both from the demonstration and replace the TODO comments with them.

### Step 3: Add the @Body placeholder

**File:** `Components/Layout/ReportsLayout.razor` (same file)

Inside the `<div class="reports-content">` div, replace the TODO comment with the directive that marks **where the page content renders** — the same one-word placeholder `AdminLayout` used inside its own content div. Without it, the Reports sidebar would show but the page area would be blank.

### Step 4: Set the folder-level layout

You need to know what an **`_Imports.razor`** file is. `_Imports.razor` is a folder-level directive file. Razor reads it before every `.razor` page in the same folder (and any sub-folders) and applies its directives as if they were typed at the top of each page. The `_Imports.razor` at the project root carries the standard `@using` statements. The folder-level `_Imports.razor` you edit below sets `@layout ReportsLayout` *once*, instead of pasting that line on every report page.

**File:** `Components/Pages/Reports/_Imports.razor`

Replace the TODO comment with the directive that applies `ReportsLayout` to **every** `.razor` page in the `Reports/` folder — the folder-level `@layout` move from the demonstration. Setting it once here saves you from writing it on every single report page.

### Steps 5-7: Add route directives to the report pages

Three files each need the directive that **makes a component routable** added at the top -- without it, there's no URL that points to the component. Replace the TODO comment in each, giving each page the route shown:

| File | Route the page should answer to |
|------|---------------------------------|
| `Components/Pages/Reports/ReportsOverview.razor` | `/reports/overview` |
| `Components/Pages/Reports/ReportsSales.razor` | `/reports/sales` |
| `Components/Pages/Reports/ReportsInventory.razor` | `/reports/inventory` |

The pages don't need an `@layout` directive: they pick up `ReportsLayout` from the `_Imports.razor` you set in Step 4.

### Verify it works

Press F5 (or Ctrl+Shift+B to build, then F5 to run). Click the navigation links:

- `/reports/overview` -- You should see both sidebars + overview cards
- `/reports/sales` -- Both sidebars + sales content
- `/reports/inventory` -- Both sidebars + inventory content
- Click "Home" -- The Reports sidebar disappears (Home doesn't use `ReportsLayout`)

If you see a 404 instead of the page, double-check:
- Did you add `@inherits LayoutComponentBase` on `ReportsLayout`? (Without it, `@Body` doesn't exist)
- Did you add the `@page` directive on the page? (Without it, there's no route)
- Did you add `@layout ReportsLayout` in `_Imports.razor`? (Without it, pages don't use the Reports layout)

---

## Practice 2: Product Catalog (Independent, 25 min)

**You used the bare `@page "/admin/dashboard"` shape in today's demo** and again in Practice 1 for the Reports pages. This practice adds the parameter syntax on top of that same directive: same `@page`, now with `{category:alpha}/{id:int:min(1)}` slots that the URL must match before the page is allowed to render.

Every product detail page on an e-commerce site is a SKU (product code) lookup — `/products/electronics/42`, `/products/books/318`, `/products/garden/77`. The URL itself carries enough information for the page to render: which category to highlight in the breadcrumb, which product record to fetch from the database. Real e-commerce backends rely heavily on URL shape because it is the cheapest input validation you will ever get.

Route parameters with constraints push validation up to the *framework*, before your `@code` block ever runs. A bare `int Id` parameter would crash if someone typed `/products/electronics/abc`. The constraint `{id:int:min(1)}` makes the router silently say "no match" and serve a 404 instead — your code never sees garbage input, because the route refused to bind it.

You'll know it works when `/products/electronics/42` renders with "electronics" and "42" filled into the heading, while `/products/home-garden/42` (hyphen fails `:alpha`), `/products/electronics/0` (fails `:min(1)`), and `/products/123/42` (digits fail `:alpha`) all 404 silently. Page HTML is pre-built; you add the `@page` directive and two `[Parameter]` properties.

### Where to work

**File:** `Components/Pages/Products/ProductDetail.razor`

You need to know what the **`[Parameter]`** attribute is. `[Parameter]` is a property attribute that tells Blazor "this property is filled in from outside the component." For route parameters, the outside source is the URL: when the route matches, Blazor copies each captured slot into the matching property. The property name must match the route parameter name (case-insensitive, but exact matching is cleaner). Blazor finds the matching property by name — if the route slot is `{category}`, it looks for a `[Parameter]` property called `Category` (or `category`). If the names don't match, the property is never filled in. Without `[Parameter]`, the property exists but Blazor leaves it at its default value — nothing tells Blazor to fill it in.

There are TODO comments in this file. You need to:

1. **Add a `@page` directive** with a route that has two parameters:
   - A `category` parameter that only accepts alphabetic characters
   - An `id` parameter that must be a positive integer (at least 1)
   - The route pattern is `/products/{...}/{...}`

2. **Display the parameter values** in the heading (replace the `???` placeholders)

3. **Add two `[Parameter]` properties** in the `@code` block:
   - A `string` property for the category (initialize it to `string.Empty`)
   - An `int` property for the ID

### Hints

- Route constraints are chained with colons: `{paramName:constraint1:constraint2}`
- The constraint for "alphabetic characters only" is `:alpha`
- The constraint for "integer" is `:int`
- The constraint for "minimum value" is `:min(N)`

### Test your work

After adding the route, test these URLs by typing them in the browser address bar:

| URL | Expected | Why |
|-----|----------|-----|
| `/products/electronics/42` | Shows the page | Alpha string + positive integer |
| `/products/home-garden/42` | 404 | Hyphen is not alphabetic (`:alpha` only accepts a-z) |
| `/products/electronics/0` | 404 | 0 fails `:min(1)` |
| `/products/electronics/-5` | 404 | -5 is a valid integer, but `:min(1)` requires the value to be at least 1 |
| `/products/123/42` | 404 | Digits are not alpha |

**Key takeaway:** Constraint failures are not errors -- they just mean "this route doesn't match." The router moves on, and if nothing matches, ASP.NET Core returns a bare 404 before Blazor runs -- so your browser shows its own error screen. **Seeing that plain error page means the constraint did its job.**

---

## Practice 3: Project Management (Independent Take-Home, ~20 min)

Project management apps — tools like Jira, Linear, Asana, or Notion — share a clean URL shape: every project lives at its own address (`/projects/47/tasks`, `/projects/47/team`, `/projects/47/settings`), and the URL carries the project context across every page in the section. A teammate can paste `/projects/47/tasks/2` into Slack and you land on the exact task they were looking at, with the main chrome and the project-specific sidebar already in place.

This practice is composition — taking the two patterns from earlier today and combining them on the same page. Each project page binds a `{projectId}` route parameter while rendering inside a nested layout chain. One page (TaskDetail) takes two parameters. Nothing here is new syntax; what's new is the *wiring*, which is the shape of nearly every day from now on.

**You saw both halves separately today** — the layout chain in the demo's Admin section (`@inherits` + `@layout` + `_Imports.razor`), the parameterized routes with constraints in Practice 2 (`{id:int:min(1)}`). This practice puts them on the same page.

You'll know it works when `/projects/1/tasks` shows both sidebars with the task list rendered between, and `/projects/1/tasks/2` drills into a specific task carrying both `projectId` and `taskId` through the URL. Layout markup, page content, and CSS are pre-built — six files have TODOs for `@page` routes and `[Parameter]` properties.

> **This is take-home extension.** PRACTICING in class covers Practices 1 and 2; Practice 3 is for students who finish Practice 2 with time to spare in class, or as after-class reinforcement. Days 2–3 build on Practices 1 and 2 only -- you will not be behind on Day 2 if you complete Practice 3 at home.

### Where to work

Six files have TODO comments. The shape mirrors Practice 1 (layout + `_Imports.razor` + pages), with each page using parameterized routes like Practice 2.

| File | What to add |
|------|-------------|
| `Components/Layout/ProjectLayout.razor` | Two directives at the top + `@Body` (mirrors Steps 1-2 of Practice 1) |
| `Components/Pages/Projects/_Imports.razor` | Folder-level layout directive (mirrors Step 3 of Practice 1) |
| `Components/Pages/Projects/ProjectTasks.razor` | `@page` with one parameter + `[Parameter]` property |
| `Components/Pages/Projects/ProjectSettings.razor` | `@page` with one parameter + `[Parameter]` property |
| `Components/Pages/Projects/ProjectTeam.razor` | `@page` with one parameter + `[Parameter]` property |
| `Components/Pages/Projects/TaskDetail.razor` | `@page` with **two** parameters + two `[Parameter]` properties |

### Direction

Start with the layout (same shape as `ReportsLayout`), then wire `_Imports.razor` (same shape as the Reports `_Imports.razor`), then add `@page` + `[Parameter]` to each page.

**Layout setup:**
- `ProjectLayout.razor` needs the same two opening directives as `ReportsLayout` (one makes it a layout, one nests it inside `MainLayout`) and an `@Body` placeholder inside the content div.
- `Projects/_Imports.razor` needs the same folder-level `@layout` directive that the Reports `_Imports.razor` uses, but pointing at `ProjectLayout`.

**Routes with one parameter** (`ProjectTasks`, `ProjectSettings`, `ProjectTeam`):
- The route pattern is `/projects/{...}/tasks`, `/projects/{...}/settings`, `/projects/{...}/team`.
- The `{...}` slot is a `projectId` parameter that must be a positive integer.
- Each page needs an `int ProjectId` property in `@code` marked with `[Parameter]`.

**Routes with two parameters** (`TaskDetail`):
- The route pattern is `/projects/{...}/tasks/{...}` -- both slots are positive integers.
- The page needs **two** `[Parameter]` properties: `int ProjectId` and `int TaskId`.

### Hints

- The integer constraint is `:int` (same as Practice 2).
- You don't need `:min(1)` here -- the project/task IDs in the test URLs are always positive, so plain `:int` is enough.
- Route parameter names in the `@page` directive **must match** the `[Parameter]` property names exactly (case-insensitive, but matching is cleaner).
- `_Imports.razor` files apply to every page in their folder -- you only need to set the layout once per folder, not on every page.

### Test your work

After wiring everything up, run the app and click through the **Projects** links in the main sidebar. Then try these URLs directly:

| URL | Expected | Why |
|-----|----------|-----|
| `/projects/1/tasks` | Main sidebar + project sidebar + task list | Nested layout chain working + `projectId` = 1 |
| `/projects/1/settings` | Main sidebar + project sidebar + settings form | Same chain, different page |
| `/projects/1/team` | Main sidebar + project sidebar + team cards | Same chain, different page |
| `/projects/1/tasks/2` | Main sidebar + project sidebar + task #2 detail | Two-parameter route -- both `projectId` and `taskId` bind |
| `/projects/abc/tasks` | 404 | `abc` fails the `:int` constraint |

**Key takeaway:** This is the same building blocks you've already used -- a layout chain from Practice 1 plus route parameters from Practice 2. The skill being practiced is **combining patterns**, not learning a new one. That's the shape of the rest of the course: each day adds one new piece, and the practice is wiring it into what you already know.

---

## Stuck?

- **Search for TODOs:** Ctrl+Shift+F > search `TODO` > look at the hints in each comment
- **Check the reference sheet:** Your Day 1 reference sheet has the syntax for `@page`, `@layout`, `@inherits`, `[Parameter]`, and route constraints
- **Review the demonstration:** Practice 1 is the same pattern the instructor demonstrated -- <a href="Day-01-Demonstration-Walkthrough.html?isCourseFile=true" target="_blank" rel="noopener">Day 1 — Demonstration Walkthrough</a> has the full record
- **Compare with Practice 1:** Practices 2 and 3 reuse the routing from Practice 1 -- the only new piece is adding route parameters and constraints
- **Ask the instructor:** They are available for questions.
