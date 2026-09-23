# Practice Walkthrough -- Day 2: NavigationManager Exercises

**What this is:** A guided companion for your Day 2 practice exercises. Practice 1 has full step-by-step instructions. Practices 2 and 3 give you direction without giving away the answer.

**Your project:** Open the `Day02Practice` starter project in Visual Studio. The navigation links are already wired up -- they'll show a 404 page until you add the correct routing and navigation code.

**How to find what needs changing:** Search for `TODO` in the project (Ctrl+Shift+F in Visual Studio). Every place you need to write code is marked with a `TODO` comment that tells you what to do.

---

## Practice 1: User Registration Wizard (Guided, 15 min)

A new user is signing up for an account on a cloud application (SaaS — Software as a Service) — Notion, Slack, your bank's portal. No app places twelve fields on one screen. The signup is split into a friendly sequence (who you are → what you like → confirmation) so each step is manageable and the user can think about one thing at a time.

What makes the "Next" button work is *code*, not a link. An `<a href>` only ever goes one place. `NavigationManager.NavigateTo()` lives inside a method. Because it is inside a method, you can also validate input, save partial state, or choose a different destination based on user answers. Today the routing is unconditional — every "Next" goes to the same next page — but you're laying the pattern you'd use for any real wizard.

You need to know what **`NavigationManager`** is. `NavigationManager` is the Blazor framework service for programmatic URL navigation. Blazor registers one per circuit/session; your component asks for it with `@inject NavigationManager Navigation` and then calls `Navigation.NavigateTo(url)` to send the user to a new page from C# code. The distinction the whole Practice rests on: `<a href>` and `<NavLink>` are pure markup — they always go to the same place. `NavigationManager.NavigateTo()` lives inside a method, so the *code* decides where to go (and can validate, save partial state, or branch based on user input first).

**You just saw this shape in today's in-class demo** with the 3-step Checkout wizard (shipping → payment → confirmation), including the `replace: true` trick that stops the browser's back button from looping users into a page they just left. Now you're driving the same pattern: info → preferences → complete, with one `replace: true` call protecting the back button.

You'll know it works when you can click through fluently, hit "Back" on Preferences to return to Info, then hit the *browser's* back button and *not* loop back into Preferences. All HTML and `@code` method stubs are pre-built; you add `@inject NavigationManager` and roughly one `NavigateTo()` line per page.

> **How this practice runs.** Practice 1 is instructor-led first, then independent. Your instructor will inject `@inject NavigationManager Navigation` on `RegisterInfo` and implement the first `NavigateTo("/register/preferences")` call together with the class. Once the first "Next" button works, Steps 3–7 are yours to complete independently.

### What you're building

When you're done, navigating to `/register/info` will start a wizard flow:
- **Step 1:** Personal Info → click "Next" → navigates to Step 2
- **Step 2:** Preferences → click "Next" → navigates to Step 3, click "Back" → returns to Step 1
- **Step 3:** Complete → click "Register Another Account" → returns to Step 1

### Step 1: Inject NavigationManager on RegisterInfo

**File:** `Components/Pages/Register/RegisterInfo.razor`

The file already has the form HTML built for you. You need to inject NavigationManager at the top (replace the TODO comment on line 2):

```razor
@inject NavigationManager Navigation
```

**What this does:** `@inject` gets the NavigationManager service into your component via dependency injection. Without it, you can't call `NavigateTo()` — you'll get a `NullReferenceException` at runtime.

### Step 2: Implement the "Next" button

**File:** `Components/Pages/Register/RegisterInfo.razor` (same file, scroll to `@code` block)

Inside the `GoToPreferences()` method, replace the TODO comment with:

```csharp
Navigation.NavigateTo("/register/preferences");
```

**What this does:** When the user clicks "Next: Preferences," your code programmatically navigates to the preferences page. The code decides where to go — not a link.

### Step 3: Inject NavigationManager on RegisterPreferences

**File:** `Components/Pages/Register/RegisterPreferences.razor`

Replace the TODO comment at the top with:

```razor
@inject NavigationManager Navigation
```

### Step 4: Implement the "Back" button with replace: true

**File:** `Components/Pages/Register/RegisterPreferences.razor` (same file, `GoBack()` method)

Replace the TODO with:

```csharp
Navigation.NavigateTo("/register/info", replace: true);
```

**What this does:** `replace: true` replaces the current browser history entry instead of adding a new one. Without it, clicking the browser's back button after "Back" would take you right back to Preferences — an infinite loop. With `replace: true`, the browser's back button skips the Preferences page entirely.

### Step 5: Implement the "Next" button on RegisterPreferences

**File:** `Components/Pages/Register/RegisterPreferences.razor` (same file, `GoToComplete()` method)

Replace the TODO with:

```csharp
Navigation.NavigateTo("/register/complete");
```

### Step 6: Inject NavigationManager on RegisterComplete

**File:** `Components/Pages/Register/RegisterComplete.razor`

Replace the TODO comment at the top with:

```razor
@inject NavigationManager Navigation
```

### Step 7: Implement the "Register Another Account" button

**File:** `Components/Pages/Register/RegisterComplete.razor` (same file, `StartOver()` method)

Replace the TODO with:

```csharp
Navigation.NavigateTo("/register/info");
```

### Verify it works

Press F5 (or hot-reload). Click "Start Registration" in the sidebar:

- `/register/info` → Fill in name/email → Click "Next: Preferences" → Navigates to step 2
- `/register/preferences` → Select language, check newsletter → Click "Next: Complete" → Navigates to step 3
- Click "Back" on step 2 → Returns to step 1. Now click browser back button → Does NOT go back to step 2 (because of `replace: true`)
- `/register/complete` → Click "Register Another Account" → Returns to step 1

If you see a `NullReferenceException` when clicking a button, check that you added `@inject NavigationManager Navigation` at the top of that file.

---

## Practice 2: Product Return Wizard with Conditional Paths (Independent, 25 min)

**You just saw the basic shape in today's in-class demo** with the 3-step Checkout wizard. The difference: Checkout always went the same direction (shipping → payment → confirm). This wizard *forks* — same starting point, two possible endings.

A customer bought something three weeks ago and it didn't work out. They click "Return Order #42" on their orders page and your app handles the next few minutes of their experience: confirm the order, ask the reason, then route them to the right outcome — refund or exchange. Real e-commerce systems — Amazon, Best Buy, and every retailer's product return flow — depend on this flow feeling smooth.

This is the practice where `<a href>` falls apart. The destination of the "Continue" button on the Reason page *depends on what the customer just clicked*. A static link can't make that decision; a method with two `NavigateTo()` calls in an `if/else` can. This is the purpose of programmatic navigation — code, not markup, decides where the user goes next.

You'll know it works when `/returns/start/42` lands on a page that knows about order 42, clicking "I want a refund" lands on `/returns/refund/42`, and "I want an exchange" lands on `/returns/exchange/42` — with the order ID preserved end-to-end. The HTML is pre-built; you wire the `@page` routes, `[Parameter]` properties, and the conditional `NavigateTo()` calls.

### Where to work

> **Reminder:** `[Parameter]` is the Blazor attribute introduced in Day 1's Practice 2 — it tells Blazor "fill this property from the URL." Each route parameter slot needs a matching `[Parameter]` property with the same name.

Four files in `Components/Pages/Returns/`:

| File | What to add |
|------|-------------|
| `ReturnStart.razor` | `@page` directive with `{orderId:int:min(1)}`, `@inject`, `[Parameter]`, `NavigateTo()` |
| `ReturnReason.razor` | `@page` directive with `{orderId:int:min(1)}`, `@inject`, `[Parameter]`, conditional `NavigateTo()` (2 paths) |
| `ReturnRefund.razor` | `@page` directive with `{orderId:int:min(1)}`, `[Parameter]`, display value |
| `ReturnExchange.razor` | `@page` directive with `{orderId:int:min(1)}`, `[Parameter]`, display value |

### Direction

This practice introduces two new concepts beyond Practice 1:

1. **Route parameters with constraints** — Each page needs an `@page` directive with `{orderId:int:min(1)}` and a matching `[Parameter] public int OrderId { get; set; }` property.

2. **Conditional navigation** — The ReturnReason page has two buttons. Each navigates to a different page based on the user's choice. Use string interpolation to include the OrderId in the URL: `$"/returns/refund/{OrderId}"`.

### Hints

- Route constraints chain with colons: `{orderId:int:min(1)}`
- Every route parameter needs a matching `[Parameter]` property
- The TODO comments tell you the exact route pattern and NavigateTo path for each file
- Preserve the `OrderId` throughout the flow by including it in every `NavigateTo()` URL

### Test your work

Start the flow by clicking "Return Order #42" in the sidebar (or type `/returns/start/42`):

| URL / Action | Expected |
|-------------|----------|
| `/returns/start/42` | Shows "Start a Return for Order #42" |
| Click "Start Return Process" | Navigates to `/returns/reason/42` |
| Click "I want a refund" | Navigates to `/returns/refund/42` |
| Click "I want an exchange" | Navigates to `/returns/exchange/42` |
| `/returns/start/0` | 404 — fails `:min(1)` constraint |
| `/returns/start/abc` | 404 — fails `:int` constraint |

**Success threshold:** TODOs 1-8 **plus Step 10** cover the core objective — routing with parameters and conditional NavigateTo. Step 10 is the `@page` directive on `ReturnExchange`: without it the exchange half of the fork 404s, so it is not polish. If you get through those, you've met today's learning target. Steps 9a/9b and 11a/11b add the display polish (showing the OrderId on the refund/exchange pages).

---

## Practice 3: Survey Wizard with Dynamic Navigation (Challenge, 10 min)

**The in-class demo's Checkout wizard used three separate pages** to do three steps. This practice asks: can you do the same job with one component and arithmetic on a route parameter?

A 3-question feedback survey — "How satisfied are you?" → "How would you rate the instructor?" → "Would you recommend us?" → "Thanks!" Familiar shape. The naive way to build it is what you did in Practices 1 and 2: one component per page, three `@page` directives, three sets of navigation methods. That works, but it's repetitive when every page looks the same and only the question text changes.

Here's the twist: **one component handles all three pages**. The current page number lives in the URL as a route parameter (`/survey/1`, `/survey/2`, `/survey/3`), and `@if` blocks decide which question to show. "Next" navigates to `/survey/{Page + 1}`. "Back" navigates to `/survey/{Page - 1}` with `replace: true` — the same back-button trick from the demo, second time you're using it.

You'll know it works when `/survey/1` shows question 1 with only a Next button, `/survey/2` and `/survey/3` show their own questions with Back + Next/Finish, and typing `/survey/4` returns a 404 because the `:range(1,3)` constraint refuses it. The `@if` blocks are pre-built; you wire the route, the `[Parameter]` property, and the three navigation calls.

### Where to work

Two files in `Components/Pages/Survey/`:

| File | What to add |
|------|-------------|
| `SurveyPage.razor` | `@page` directive with `{page:int:range(1,3)}`, `@inject`, `[Parameter]`, `NavigateTo()` for next/back/finish |
| `SurveyComplete.razor` | `@inject`, `NavigateTo()` to restart |

### Direction

This exercise has a twist — the **page number is a route parameter**, not a separate page per step:

- The route `/survey/{page:int:range(1,3)}` uses the `:range(1,3)` constraint to limit pages to 1, 2, or 3
- The "Next" button increments the page: `Navigation.NavigateTo($"/survey/{Page + 1}")`
- The "Back" button decrements with `replace: true`: `Navigation.NavigateTo($"/survey/{Page - 1}", replace: true)`
- The "Finish" button on page 3 navigates to `/survey/complete`

### Test your work

| URL / Action | Expected |
|-------------|----------|
| `/survey/1` | Shows question 1, Next button only (no Back) |
| Click "Next" | Navigates to `/survey/2`, shows question 2 with Back + Next |
| Click "Next" | Navigates to `/survey/3`, shows question 3 with Back + Finish |
| Click "Finish Survey" | Navigates to `/survey/complete` |
| Click "Retake Survey" | Returns to `/survey/1` |
| `/survey/0` or `/survey/4` | 404 — fails `:range(1,3)` constraint |

**Key takeaway:** A single component handles all three pages by using the route parameter to control its content and behavior. The `@if` blocks in the HTML are already pre-built to show different questions based on the `Page` value — you just need to wire the routing and navigation.

---

## Stuck?

- **Search for TODOs:** Ctrl+Shift+F > search `TODO` > look at the hints in each comment
- **Check the reference sheet:** Your Day 2 reference sheet has the syntax for `@inject`, `NavigateTo()`, `[Parameter]`, and route constraints
- **Check the demo walkthrough:** The demo walkthrough shows exactly how multi-param routes and NavigateTo work
- **Common issues:**
  - `NullReferenceException` on button click → Missing `@inject NavigationManager Navigation`
  - 404 when clicking a link → Missing or incorrect `@page` directive
  - Parameter value is always 0 or empty → Missing `[Parameter]` attribute on the property
  - Browser back button loops → Missing `replace: true` on back-navigation calls
- **Ask the instructor:** They are available for questions.

---

## What You Learned

After completing these exercises, you can:

- **Use multiple route parameters** with constraints like `{category:alpha}/{id:int:min(1)}` to build precise, validated URLs
- **Inject `NavigationManager`** and use `NavigateTo()` for programmatic navigation between pages
- **Apply `replace: true`** to prevent confusing browser back-button behavior on redirect-style navigation
- **Combine route constraints** to enforce URL shape at the framework level, before your code runs

### Key Distinctions

| Pattern | When to Use | Example |
|---------|-------------|---------|
| `NavLink` / `<a href>` | User clicks a link (declarative) | Navigation menu, page links |
| `NavigateTo("/path")` | Code decides to navigate (programmatic) | After form submit, guard redirect |
| `NavigateTo("/path", replace: true)` | Replace current history entry | Wizard step transitions, redirects |

### Looking Ahead

**Day 3 -- Instruction Day:** State management and the Flux/Redux pattern. Your pre-class readings (assigned today at wrap-up -- three documents, ~56 min total) prepare you with the vocabulary for Fluxor: actions, reducers, effects, and state. Read them before Day 3.
