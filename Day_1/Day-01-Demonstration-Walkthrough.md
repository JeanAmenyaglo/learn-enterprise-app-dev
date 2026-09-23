# Demonstration Walkthrough -- Day 1: Nested Layout Chain (Admin Section)

**What this is:** A written record of the Admin-section demonstration your instructor built in class. Use it to review the nested layout chain at your own pace before starting the practices. You do **not** type this code yourself — the demonstration project (`Day01Demo`) is the instructor's. You build the same pattern yourself in **Practice 1 (Reports Section)**.

**The idea:** A *layout chain* lets one layout render inside another. The site-wide `MainLayout` (dark sidebar) wraps a section-specific `AdminLayout` (light-blue sidebar), which wraps the page. The render chain is **`MainLayout → AdminLayout → page`**.

The whole demonstration is **7 directives** across 5 files. Nothing else changes — the sidebar markup, the page content, the CSS, and the navigation links are all pre-built.

---

## Steps 1-3: Making AdminLayout a Nested Layout

**File:** `Components/Layout/AdminLayout.razor`

This file already has the admin sidebar markup. Three directives turn it into a working nested layout:

```razor
@inherits LayoutComponentBase
@layout MainLayout

<div class="admin-container">
    <nav class="admin-sidebar">
        ... admin sidebar links (pre-built) ...
    </nav>

    <div class="admin-content">
        @Body
    </div>
</div>
```

- **`@inherits LayoutComponentBase`** — This is what makes a component a *layout*. Without it, the component has no `@Body` property and cannot render child pages inside it. Every layout needs this.

- **`@layout MainLayout`** — This is the *nesting*. It tells Blazor: when you render `AdminLayout`, wrap it inside `MainLayout` first. That is what produces the chain `MainLayout → AdminLayout → page`.

- **`@Body`** — This goes inside the content `<div>`. It is the slot where the actual page (Dashboard, Users, or Settings) renders. The admin sidebar stays fixed; only the `@Body` content swaps.

---

## Step 4: Scoping the Layout to the Folder

**File:** `Components/Pages/Admin/_Imports.razor`

```razor
@layout AdminLayout
```

An `_Imports.razor` file applies its directives to **every component in the same folder**. Putting `@layout AdminLayout` here means every page in `Components/Pages/Admin/` automatically uses `AdminLayout` — you never write `@layout` on the individual pages. With 3 admin pages it saves 3 lines; with 20 pages it saves 20.

---

## Steps 5-7: Routing the Admin Pages

Each admin page has its content pre-built but needs a `@page` directive to become reachable at a URL:

| File | Directive |
|------|-----------|
| `Components/Pages/Admin/AdminDashboard.razor` | `@page "/admin/dashboard"` |
| `Components/Pages/Admin/AdminUsers.razor` | `@page "/admin/users"` |
| `Components/Pages/Admin/AdminSettings.razor` | `@page "/admin/settings"` |

`@page` makes a component routable — it is the URL the component answers to. Notice none of these pages has an `@layout` directive: they pick up `AdminLayout` automatically from the `_Imports.razor` in Step 4.

---

## The Result: Watching the Chain Work

With all 7 directives in place, the running app behaves like this:

- Navigate to `/admin/dashboard` — you see **two** sidebars: the dark `MainLayout` sidebar and the light-blue `AdminLayout` sidebar, with the dashboard content between them.
- Click "Users" or "Settings" — both sidebars stay put; only the page content swaps. That is the layout chain doing its job.
- Click "Home" — the admin sidebar disappears. `Home.razor` does not use `AdminLayout`, so it renders inside `MainLayout` only.

**How the chain renders:** `MainLayout` renders its `@Body`, which is `AdminLayout`. `AdminLayout` renders its `@Body`, which is the page. Three layers, drawn from the outside in.

---

## What You Do Next

In **Practice 1**, you build a **Reports section** — the exact same 7-directive pattern with a green theme instead of blue (`ReportsLayout`, `Reports/_Imports.razor`, and three report pages). If you followed this walkthrough, Practice 1 is the same moves with different names. **Practice 2** then adds a new idea on top: route parameters and constraints. See <a href="Day-01-Practice-Walkthrough.html?isCourseFile=true" target="_blank" rel="noopener">Day 1 — Practice Walkthrough</a> for the step-by-step.

| Directive | What it does |
|-----------|--------------|
| `@inherits LayoutComponentBase` | Makes a component a layout (gives it `@Body`) |
| `@layout SomeLayout` | Nests this component inside `SomeLayout` |
| `@Body` | Marks where child page content renders |
| `@layout` in `_Imports.razor` | Applies a layout to every page in the folder |
| `@page "/path"` | Makes a component routable at a URL |
