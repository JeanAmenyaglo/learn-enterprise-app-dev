# Practice Walkthrough -- Day 7: Route Guards and Navigation with State

**What this is:** A guided companion for your Day 7 practices. Practice 1 gives direction without giving away the answer. Practice 2 is directed -- the TODO comments name the class and the method signature, because the point is the move, not guessing the shape. Practice 3 is a take-home challenge with hints.

**Your project:** Open the `Day07Practice` starter project in Visual Studio. The project has a complete Fluxor cart store, a user store with roles, and a form wizard store -- all pre-built. You write only the guard logic.

**How to find what needs changing:** Search for `TODO` in the project (Ctrl+Shift+F in Visual Studio). Every place you need to write code is marked with a `TODO` comment.

---

## Demonstration Recap: Cart Checkout Guard

In class, your instructor built a route guard live on a checkout page: it injects
`IState<CartState>` and `NavigationManager`, overrides `OnInitialized`, and
redirects to `/shop` when the cart is empty -- then extracts the decision into a
static `CartGuard` class for testability.

That cart guard is **already wired into `Components/Pages/Checkout.razor`** in your practice
starter -- open it and read it as a worked reference. The two practices below
apply the same three-ingredient pattern (inject state + `NavigationManager`,
check in `OnInitialized`, redirect and return) to new slices of state.

---

## Practice 1: Admin Dashboard Guard -- Guided opening, then independent (25 min)

Every SaaS app with admin features eventually faces this: a regular user clicks a link to a page they shouldn't see, and the page renders anyway — maybe with a "you don't have permission" banner, maybe with broken UI that crashes when it can't find admin-only data. The right shape is to prevent the page from rendering at all and route them elsewhere.

The pattern is the same as the cart checkout guard, but applied to a different part of the application state (`UserState.Role` instead of `CartState.Items`). Instead of reading `CartState.Items.Count`, you read `UserState.Role`. The structure — inject state + `NavigationManager`, check in `OnInitialized`, redirect and return — is unchanged. Only the condition changes.

**You just watched the instructor build this exact pattern** for the empty-cart case (`if (CartState.Value.Items.Count == 0) Navigation.NavigateTo("/shop")`), then extract the decision into a static `CartGuard.ShouldRedirect()` for testability. Here you apply the same skeleton with `UserState.Value.Role != "Admin"` and a redirect to `/access-denied`.

You'll know it works when clicking the Admin Dashboard link as a non-admin (or before logging in) immediately redirects to `/access-denied` with no flash of admin content, logging in as Admin lets the page render, and the AccessDenied page itself has no guard (so visitors don't bounce-loop). Store, login UI, and admin dashboard markup are pre-built; you add the guard in `AdminDashboard.razor` and create one new file for `AccessDenied.razor`.

> **How this practice runs.** The route guard is a new pattern, so your instructor leads the **first ~10 minutes as a guided group walk-through** — the whole class builds the opening of the admin guard together, step by step, at the instructor's pace (inject `IState<UserState>` + `NavigationManager`, override `OnInitialized`, write the first `if (UserState.Value.Role != "Admin")` check). After that you continue **on your own** — finish the redirect, build the AccessDenied page, and test. Tasks 1-2 below are the guided part; tasks 3-4 are independent.

### What's already built

The user feature is complete:

- `UserState.cs` -- record with `Role` (default "User") and `UserName` (default "Guest")
- `UserActions.cs` -- `SetUserRoleAction(string Role)`
- `UserReducers.cs` -- reducer that updates the role
- `UserFeature.cs` -- initial state
- `Login.razor` -- buttons to set role to "User" or "Admin", plus a link to the Admin Dashboard
- `AdminDashboard.razor` -- admin content with TODO comments for the guard

### Your tasks

1. **Open `AdminDashboard.razor`.** It already injects `IState<UserState>`. You need to add `NavigationManager` and an `OnInitialized` guard.

2. **The guard condition:** Check if the user's role is NOT "Admin". If the user is not an admin, redirect to `/access-denied`.

   *Hint:* The condition is `UserState.Value.Role != "Admin"`.

3. **Create `AccessDenied.razor`** in `Components/Pages/`. In Visual Studio, right-click `Components/Pages/` → Add → Razor Component. Name it `AccessDenied`. Replace the entire file contents with the hint code below. It needs:
   - A `@page "/access-denied"` directive
   - A message telling the user they don't have permission
   - A link back to the home page (use `<a href="/">` or `<NavLink>`)

   *Hint:* AccessDenied.razor does NOT need `@inherits FluxorComponent` -- it doesn't display state. **Important:** do NOT add a guard to `AccessDenied.razor`. A guard on this page would cause an infinite redirect loop — the page redirects to itself, the browser hangs, and there is no clear error message. You would have to stop the dev server manually to recover.

4. **Test your guard:**
   - Click the Admin Dashboard link without logging in -- should redirect to Access Denied
   - Log in as Admin, click Admin Dashboard -- should render
   - Log in as User, click Admin Dashboard -- should redirect to Access Denied

### Success criteria

- Non-admin users are redirected to `/access-denied`
- Admin users see the dashboard
- The access denied page has a link back to home
- No infinite redirect loops

### Hints if you're stuck

<details>
<summary>Hint 1: What to inject</summary>

You need `@inject NavigationManager Navigation` in AdminDashboard.razor. The page already has `@inject IState<UserState> UserState`.

</details>

<details>
<summary>Hint 2: The guard pattern</summary>

Same structure as the demonstration cart guard, different condition:

```csharp
protected override void OnInitialized()
{
    base.OnInitialized();

    if (/* condition */)
    {
        Navigation.NavigateTo("/access-denied");
        return;
    }
}
```

</details>

<details>
<summary>Hint 3: AccessDenied.razor structure</summary>

```razor
@page "/access-denied"

<h3>Access Denied</h3>
<p>You do not have permission to view this page.</p>
<a href="/">Return to Home</a>
<a href="/login">Go to Login</a>
<a href="/login">Go to Login</a>
```

That's all you need. No Fluxor, no state injection.

</details>

### Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| AccessDenied page shows 404 | Missing `@page "/access-denied"` directive | Add the directive at the top of the file |
| Infinite redirect loop | AccessDenied.razor also has a guard | Remove any guard from AccessDenied.razor |
| Guard doesn't trigger | Role comparison is case-sensitive | Ensure your comparison matches the exact string ("Admin") |
| `NavigateTo("access-denied")` doesn't work | Missing leading `/` in the path | Use `"/access-denied"` with the leading slash |

---

## Practice 2: Extract and Test the Admin Guard (10 min)

Your guard works. But right now the decision -- *is this person an admin?* -- lives inside `AdminDashboard.razor`, and a test cannot reach it there. To exercise it you would have to render the whole page, which means a test harness, a renderer, and a lot of setup for one boolean.

Pull the decision out into a plain static class and it becomes a one-line test. Nothing about the app's behaviour changes; what changes is that the decision can now be checked without the UI. **This is the move Lab 1 grades**: 2 points for the guard being a static method, and part of your individual-contribution marks for the test you write against it.

### Part 1 -- Extract (about 6 minutes)

1. Create `Guards/AdminGuard.cs` in the `Day07Practice` project:

    ```csharp
    using Day07Practice.Features.User.Store;

    namespace Day07Practice.Guards;

    public static class AdminGuard
    {
        public static bool ShouldRedirect(UserState state) =>
            state.Role != "Admin";
    }
    ```

2. In `AdminDashboard.razor`, add `@using Day07Practice.Guards` next to the other `@using` lines.

3. Replace the condition in `OnInitialized` with a call to the guard:

    ```csharp
    if (AdminGuard.ShouldRedirect(UserState.Value))
    {
        Navigation.NavigateTo("/access-denied");
        return;
    }
    ```

The redirect and the `return` stay in the component. The guard answers the question; the component acts on the answer. That split is the whole point -- a question can be tested, a redirect cannot.

### Part 2 -- Test (about 4 minutes)

The test project is already set up. `Day07Practice.Tests` references the app, xUnit is wired, and `AdminGuardTests.cs` has a TODO waiting for you. Write one test:

- **Arrange:** `var state = new UserState { Role = "User" };`
- **Act:** call `AdminGuard.ShouldRedirect(state)`
- **Assert:** `Assert.True(...)`

Name it `ShouldRedirect_WhenRoleIsNotAdmin_ReturnsTrue` -- the same `MethodName_Scenario_ExpectedResult` convention you used for reducer tests on Day 4.

Run it: **Test > Test Explorer** in Visual Studio, or `Ctrl+E, T`.

### Stretch: test the other direction

Write a second test where `Role = "Admin"` and the guard returns **false**.

This matters more than it looks. A guard that ignored its input and always returned `true` would still pass your first test. The false case is the one that proves the condition is real -- and in Lab 1 you will meet guard stubs that do exactly that.

### Success criteria

- The app behaves exactly as it did at the end of Practice 1 -- nothing visible changed
- `AdminDashboard.razor` contains no role comparison; it calls `AdminGuard.ShouldRedirect`
- One green test in Test Explorer, with no component rendered anywhere in it

### If you get stuck

| Symptom | Likely cause | Fix |
|---|---|---|
| `AdminGuard` not found in the `.razor` | Missing `@using Day07Practice.Guards` | Add it with the other `@using` lines -- the error names the class, not the using |
| Test project shows no tests | `GlobalUsings.cs` missing `global using Xunit;` | It ships with the starter; restore the line if it was deleted |
| Your test needs a renderer or a component | The condition is still inside the component | Finish Part 1 first -- if the test needs the UI, the extraction did not happen |
| Guard returns void | `NavigateTo` was moved into the guard | The guard returns `bool` and nothing else; the redirect stays in `OnInitialized` |

Worked solution: `Code-Examples/finished/exercise-2/`.

---

## Practice 3: Form Wizard Guards -- Take-Home Challenge

Every multi-step form in a serious app — tax filing software, insurance quote flows, account onboarding, government applications — guards against URL hacks where a user types `/form/step3` without ever doing Steps 1 or 2. The server will reject the submission anyway. But it is better to redirect the user back to the earliest step they have not finished, so they do not fill out Step 3 only to lose that work.

The pattern is the same guard skeleton as Practice 1, with a *composite* condition. Step 2's guard checks one set membership (`CompletedSteps.Contains(1)`); Step 3's guard checks two and redirects to the *earliest* incomplete step. This is the first guard you'll write where the redirect target is computed, not hardcoded — a small but real piece of decision logic that belongs in the guard.

**The cart guard from the demo had a single condition** (`Items.Count == 0`) with a single redirect target (`/shop`). Step 3 here has two conditions chained with an `else if`, choosing between two redirect targets. Same guard pattern, deeper decision logic.

You'll know it works when typing `/form/step3` directly redirects to `/form/step1`, completing Step 1 then jumping straight to Step 3 redirects to `/form/step2`, and going through the wizard in order lands cleanly on Step 3. Form store, reducers, and step UI are pre-built — you add the guard in `FormStep2.razor` and the chained guard in `FormStep3.razor`.

### What's already built

The form feature is complete:

- `FormState.cs` -- record with `CompletedSteps` (`IReadOnlySet<int>`)
- `FormActions.cs` -- `CompleteStepAction(int Step)`
- `FormReducers.cs` -- reducer that adds the step number to the set
- `FormFeature.cs` -- initial state with empty set
- `FormStep1.razor` -- entry point (no guard needed), dispatches `CompleteStepAction(1)` and navigates to Step 2
- `FormStep2.razor` -- Step 2 with TODO comments for a guard
- `FormStep3.razor` -- Step 3 with TODO comments for a guard

### Your tasks

1. **Guard FormStep2.razor:** Check if Step 1 has been completed. If not, redirect to `/form/step1`.

2. **Guard FormStep3.razor:** Check if both Steps 1 AND 2 have been completed. If either is missing, redirect to the earliest incomplete step.

3. **Test:**
    - Navigate directly to `/form/step3` -- should redirect.
    - Complete Step 1, navigate to Step 3 -- should redirect to Step 2.
    - Complete Steps 1 and 2, navigate to Step 3 -- should render.

### Success criteria

- Navigating to Step 2 without completing Step 1 redirects to Step 1
- Navigating to Step 3 without completing Step 1 redirects to Step 1
- Navigating to Step 3 with only Step 1 complete redirects to Step 2
- Completing all steps in order allows access to each page

### Hints if you're stuck

<details>
<summary>Hint 1: Checking if a step is completed</summary>

`FormState.Value.CompletedSteps` is an `IReadOnlySet<int>` — a read-only view over a `HashSet<int>`. Use `.Contains(stepNumber)` to check if a step has been completed. The state uses a set instead of a `List<int>` because a set cannot hold duplicates — you can only complete Step 1 once.

Your guard needs the *negative* test: the redirect fires when the step is **not** in the set.

</details>

<details>
<summary>Hint 2: Step 3 redirect logic</summary>

Check the earliest missing step first. If Step 1 is missing, redirect there. Otherwise, if Step 2 is missing, redirect there.

The shape is two `if` checks chained with `else if`, tested earliest-step-first, each one navigating to its own target and then returning so the rest of the method does not run.

**Think about:** the guards chain — Step 2's page has its own guard. So if you tested Step 2 first, a visitor who has completed nothing would still *end up* on Step 1, just by way of an extra redirect. Earliest-first is about sending them straight there, not about landing somewhere different.

</details>

### Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Guard redirects even after completing steps | `NavigationManager` and `IState<FormState>` already injected, but guard condition is wrong | Check your `.Contains()` logic |
| Steps don't track completion | The `CompleteAndNext` method already dispatches the action -- no changes needed there | Verify you're checking the right step numbers |

---

## What You Learned

After completing these practices, you can:

- **Implement a route guard** that checks Fluxor state and redirects before the page renders
- **Choose between guard and UI hiding** -- guards prevent pages from rendering; `@if` hides elements on a rendered page
- **Apply the guard pattern to different state types** -- cart items (count check), user roles (string check), completed steps (set membership check)

Both practices use the same three-step pattern -- the same one from the demonstration:

1. Inject `IState<T>` and `NavigationManager`
2. Check the condition in `OnInitialized`
3. Redirect and return if the condition fails

### Looking ahead

**Day 8 -- State Persistence:** Your guards check Fluxor state. But what happens when the user refreshes the browser? The Fluxor store resets, the cart empties, and the guard blocks checkout. Tomorrow you'll learn to persist state to localStorage so it survives page refreshes.
