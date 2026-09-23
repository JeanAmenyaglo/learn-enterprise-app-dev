# Demo Walkthrough -- Day 2: Multiple Route Parameters, NavigationManager, Programmatic Navigation

**Purpose:** A comprehensive, step-by-step reference for the Day 2 demo. Use this to prepare before class, to follow along during the demo, or as a post-class reference. This is a **class follow-along document** — the project shown in Step 1 is your instructor's demo project. You do not need to open it yourself. Use the <a href="Day-02-Practice-Walkthrough.html?isCourseFile=true" target="_blank" rel="noopener">Practice Walkthrough</a> for your own hands-on exercises.

**Starting point:** The `Code-Examples/demo/starter/Day02Demo` project — a Blazor Web App with MainLayout, NavMenu (with Products and Checkout links pre-built), Home page (search form UI pre-built), ProductDetail (card and test URL table pre-built), and three Checkout pages (all form HTML pre-built). Only the routing, injection, and navigation lines are missing.

**End state:** A working app with multi-parameter constrained routes, a search form that navigates programmatically, and a 3-step checkout wizard with history management.

---

## Step 1: Open the Starter Project and Run It

Open `Code-Examples/demo/starter/Day02Demo` in Visual Studio. Press F5 to run.

**What you'll see:**
- The Home page loads with a search form (category + product ID fields)
- The sidebar has links for Products (Electronics #42, Clothing #7) and Checkout
- Clicking any Products link → **404 page** (ProductDetail has no `@page` directive yet)
- Clicking "Go to Product" button → nothing happens (no NavigationManager injected, no NavigateTo call)
- Clicking "Start Checkout" → the Shipping page renders (its `@page` is pre-built) — but clicking "Continue to Payment" does nothing, because the button's handler is an empty TODO

> **Why start here:** The UI is complete but non-functional. The roughly 12 lines added in the steps below are what bring it to life -- this framing makes the learning targets visible and concrete.

---

## Step 2: Add the Multi-Parameter Route to ProductDetail

**File:** `Components/Pages/ProductDetail.razor`

**What's already there:** A card showing a test URL table with expected results (match/404) and explanations. The `@code` block is empty.

**Action:** Replace the TODO at the top of the file with:

```razor
@page "/products/{category:alpha}/{id:int:min(1)}"
```

Then replace the TODO in the `@code` block with:

```csharp
[Parameter] public string Category { get; set; } = string.Empty;
[Parameter] public int Id { get; set; }
```

**Why it matters:**

> This is the first new concept today -- multiple parameters in one route template. You have typed this same two-segment template once already, in Day 1 Practice 2; today is where it gets explained. It chains two parameters: `{category:alpha}` requires alphabetic text, and `{id:int:min(1)}` requires a positive integer. Each segment validates independently.
>
> Every route parameter needs a matching `[Parameter]` property. `{category}` maps to `Category` (case-insensitive). `{id}` maps to `Id`. Blazor binds the URL values to these properties automatically.

**Now update the card title** — this is how you *see* the binding worked. Replace the static `<h5>` with:

```razor
<h5 class="card-title">@Category -- Product #@Id</h5>
```

---

## Step 3: Test Multi-Parameter Constraints

Hot-reload (Ctrl+Shift+F5 — applies code changes while the app keeps running) or restart (F5 — full stop and restart). Click the sidebar link **"Electronics #42"** — the product page renders showing "electronics -- Product #42."

Now test constraint failures by typing URLs in the browser address bar:

| URL | Result | Explanation |
|-----|--------|-------------|
| `/products/electronics/42` | Page renders | Alpha string ✓, positive integer ✓ |
| `/products/home-garden/42` | 404 | Hyphen is not alphabetic — `:alpha` only accepts a-z |
| `/products/electronics/0` | 404 | Zero fails `:min(1)` |
| `/products/123/42` | 404 | Digits are not alpha |

> **Key concept:** Constraint failures are not exceptions. They are not crashes. The constraint simply says "this route does not match for that value." The router moves on, finds nothing, and shows the 404 page. Clean, silent, expected.

---

## Step 4: DI Refresher — Inject NavigationManager on Home Page

**File:** `Components/Pages/Home.razor`

**What's already there:** A search form with category and product ID inputs, a "Go to Product" button wired to `GoToProduct()`, and the `@code` block with the method stub and field variables.

**Action:** Replace the TODO at the top with:

```razor
@inject NavigationManager Navigation
```

**Why it matters:**

> Quick recall from SDEV 2301 — `@inject` gets services into components. `NavigationManager` is just another built-in service, injected the same way. The alias `Navigation` keeps the calls short.

---

## Step 5: Connect the Search Form with NavigateTo()

**File:** `Components/Pages/Home.razor` (same file, scroll to `@code` block)

**Action:** In the `GoToProduct()` method, replace the TODO comment with:

```csharp
Navigation.NavigateTo($"/products/{category}/{productId}");
```

**Why it matters:**

> This is the key difference from Day 1. Previously, users clicked links to navigate. Now, the *code* builds the URL and navigates. String interpolation takes the form field values — `category` and `productId` — and constructs the full URL. When the user clicks "Go to Product," the C# code sends them there.

**Try it:** Hot-reload. The form already has "electronics" and "42" as defaults. Clicking "Go to Product" renders the product page.

Change the category to "clothing" and the ID to "7." Clicking again navigates to `/products/clothing/7`.

> The form does not care what values the user types — it builds the URL dynamically. If the values fail the route constraints, the result is a 404: the constraints are still the guard.

---

## Step 6: Add Navigation to CheckoutShipping — Forward Navigation

**File:** `Components/Pages/Checkout/CheckoutShipping.razor`

**What's already there:** A complete shipping form (name, address, city fields) and a "Continue to Payment" button wired to `ContinueToPayment()`.

**Action:** Replace the TODO at the top with:

```razor
@inject NavigationManager Navigation
```

In `ContinueToPayment()`, replace the TODO with:

```csharp
Navigation.NavigateTo("/checkout/payment");
```

**Why it matters:**

> Same pattern — inject, then use. The button calls the method, and the method calls `NavigateTo()`. Code decides where to go, not a link.

---

## Step 7: Add Navigation to CheckoutPayment — Forward and Back with `replace: true`

**File:** `Components/Pages/Checkout/CheckoutPayment.razor`

**What's already there:** A payment form (card number, expiry, CVV) with both a "Back" button and a "Review Order" button.

**Action:** Replace the TODO at the top with:

```razor
@inject NavigationManager Navigation
```

In `GoBack()`, replace the TODO with:

```csharp
Navigation.NavigateTo("/checkout/shipping", replace: true);
```

In `ReviewOrder()`, replace the TODO with:

```csharp
Navigation.NavigateTo("/checkout/confirm");
```

**Why `replace: true`:**

> `replace: true` is the important one here. Watch what it does to browser history. Without it, pressing the browser's back button after going back would return to the payment page again — an infinite loop. With `replace: true`, the back action replaces the current history entry instead of adding a new one. The browser's back button skips the replaced page entirely.

---

## Step 8: Add Navigation to CheckoutConfirm — Back and Start Over

**File:** `Components/Pages/Checkout/CheckoutConfirm.razor`

**What's already there:** A confirmation card with a success alert, a "Back to Payment" button, and a "Start Over" button.

**Action:** Replace the TODO at the top with:

```razor
@inject NavigationManager Navigation
```

In `GoBack()`, replace the TODO with:

```csharp
Navigation.NavigateTo("/checkout/payment", replace: true);
```

In `StartOver()`, replace the TODO with:

```csharp
Navigation.NavigateTo("/checkout/shipping");
```

**Why mix `replace: true` with a plain `NavigateTo`:**

> Same `replace: true` on the back button. The Start Over button uses plain `NavigateTo` without replace — that navigation action SHOULD stay in history, so the user can press the browser back button and return to the confirmation page if they change their mind.

---

## Step 9: Demo the Complete Wizard Flow

Hot-reload, then walk through the checkout:

1. **Click "Start Checkout"** in the sidebar → Shipping page. All of this HTML was pre-built; only two lines were added -- the `@inject` and the `NavigateTo` call.

2. **Click "Continue to Payment"** → Payment page. The Back button goes back to shipping, but with `replace: true`.

3. **Click "Back"** → Returns to Shipping. Now click the **browser's back button** → the URL stays on `/checkout/shipping`. Nothing appears to happen — and that is the proof: the payment entry was replaced, so there is no payment page behind you. Press back once more to leave checkout entirely.

   > That is `replace: true` in action. The payment page was replaced in history, so the browser no longer knows it existed.

4. Walk forward again: Shipping → Payment → **"Review Order"** → Confirmation page.

5. **Click "Start Over"** → Back to Shipping. Click the browser back button → Goes to Confirmation (because Start Over did NOT use `replace: true`).

   > About twelve lines of code total: multi-param routes, NavigationManager, and a wizard with history management. That is programmatic navigation.

---

## Step 10: NavigateTo Overloads Summary (30 sec, verbal)

A quick summary of the three overloads:

| Call | What it does |
|------|-------------|
| `NavigateTo("/path")` | Client-side navigation. Fast. Preserves Blazor circuit. |
| `NavigateTo("/path", forceLoad: true)` | Full server round-trip. Destroys circuit. Only for non-Blazor pages. |
| `NavigateTo("/path", replace: true)` | Client-side but replaces current history entry. Back button skips it. |

> The default is almost always what you want. `forceLoad` is for leaving the Blazor app entirely. `replace` is for back buttons and redirect-after-submit.

---

## Files Created/Modified Summary

| File | What was added | Lines typed |
|------|---------------|-------------|
| `ProductDetail.razor` | `@page` directive + 2 `[Parameter]` properties | 3 |
| `Home.razor` | `@inject` + `NavigateTo()` call | 2 |
| `CheckoutShipping.razor` | `@inject` + `NavigateTo()` call | 2 |
| `CheckoutPayment.razor` | `@inject` + `NavigateTo()` forward + `NavigateTo()` back with `replace` | 3 |
| `CheckoutConfirm.razor` | `@inject` + `NavigateTo()` back with `replace` + `NavigateTo()` start over | 3 |
| **Total** | | **13 lines** |

