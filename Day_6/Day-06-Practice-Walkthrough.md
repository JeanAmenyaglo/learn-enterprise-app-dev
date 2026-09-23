# Practice Walkthrough -- Day 6: Centralized State, Component Integration, and the BYSResult Pattern

**What this is:** A companion for your Day 6 practices. Practice 1 walks you through every file and step — it names which directives each card needs and why, and you recall the exact syntax from the demonstration you just watched. Practice 2 gives direction with hints but not full solutions.

**Your project:** Open the `Day06Practice` starter project in Visual Studio. The project has a complete Account Fluxor store (state, actions, reducers, effects, feature) plus a ProductService that throws exceptions on validation failure.

**How to find what needs changing:** Search for `TODO` in the project (Ctrl+Shift+F in Visual Studio). Every place you need to write code is marked with a `TODO` comment.

> **How this practice runs.** Practice 1 is a **worked launch** — the instructor wires `OrdersCard.razor` on screen first (the same three Fluxor directives you watched in the demo). After Task 1 you continue **on your own** — recall and apply the same three directives to `WishlistCard.razor` (Task 2) and `MembershipCard.razor` (Task 3), then run the project (Task 4) and test shared state (Task 5). Wait for your instructor to begin before typing.

---

## Practice 1: Account Overview 3-Component Wiring (worked launch, 20 min)

Every real app has an account or overview screen — the "my account" page on a shopping site, an order summary, a customer profile. Each card shows different data (orders placed, saved items, membership level), but the data usually comes from one backend call. Three separate API calls — one per card — is the wrong shape; one call into a shared store with three subscribers is right.

Multi-component subscriptions to a single store are the proof that centralized state pays for itself. One `LoadAccountAction` dispatch, one service call, one state update, three cards re-render with what they care about. No props between components, no callbacks, no parent telling the child what to show.

**This is the same multi-component subscription pattern the demonstration showed — now applied to a different feature, the Account Overview page.** The demo wired three Dashboard cards to `DashboardState`; here you wire three Account cards to `AccountState`. Same three Fluxor directives per card (`@using Fluxor`, `@inherits FluxorComponent`, `@inject IState<AccountState>`), same shared-store shape, different data. Because you watched the pattern written, this walkthrough names *which* directives each file needs and *why*, but doesn't print them. Recall from the demo; <a href="Day-06-Demonstration-Walkthrough.html?isCourseFile=true" target="_blank" rel="noopener">Day 6 — Demonstration Walkthrough</a> has the full reference.

You'll know it works when the Account page shows Orders: 12, Wishlist: 5, Membership: Gold — all from one `LoadAccountAction` dispatch — and changing the values `MockAccountService` returns updates every card that binds them, from one dispatch. Store, service, and parent are pre-built; you add three directives plus one binding per card.

### What's already built

The Account feature is complete:

- `Features/Account/Store/AccountState.cs` -- state record with OrderCount, WishlistCount, MembershipTier, IsLoading, ErrorMessage
- `Features/Account/Store/AccountActions.cs` -- Load, LoadSuccess, LoadFailure actions
- `Features/Account/Store/AccountReducers.cs` -- three reducers (one per action)
- `Features/Account/Store/AccountEffects.cs` -- effect calling MockAccountService
- `Features/Account/Store/AccountFeature.cs` -- initial state registration
- `Services/IAccountService.cs` / `Services/MockAccountService.cs` -- interface and mock
- `Components/Pages/Account.razor` -- parent page that dispatches `LoadAccountAction` in `OnInitialized`

Three card files have UI markup but **no Fluxor wiring**:

- `Components/Pages/OrdersCard.razor`
- `Components/Pages/WishlistCard.razor`
- `Components/Pages/MembershipCard.razor`

### Task 1: Wire OrdersCard

**File:** `Components/Pages/OrdersCard.razor`

At the very top of the file (before the `<div>`), the TODO comments mark where **three directives** go — the same set you saw at the top of each card in the demonstration:

- One directive imports the Fluxor namespace so `IState<T>` is recognized.
- One directive makes this card inherit from `FluxorComponent`, subscribing it to state changes. Without it, the card renders once and never updates when the store changes.
- One directive injects the account state from the Fluxor store into this card.

Recall all three from the demonstration and replace the TODO comments with them.

Then replace the `--` placeholder text with the data binding expression that reads the order count from the injected state. The expression follows the pattern the demo used to read values from `AccountState.Value`.

### Task 2: Wire WishlistCard

**File:** `Components/Pages/WishlistCard.razor`

Add the same three directives at the top — same namespace import, same inheritance, same state injection. Replace the `--` placeholder with the binding expression that reads the wishlist count from `AccountState.Value`.

The wiring is identical to Task 1. Only the property you bind to is different.

### Task 3: Wire MembershipCard

**File:** `Components/Pages/MembershipCard.razor`

Same three directives. Replace the `--` placeholder with the binding expression that reads the membership tier from `AccountState.Value`.

### Task 4: Run and Verify

Run the project and navigate to the Account page. You should see:

- OrdersCard: **12**
- WishlistCard: **5**
- MembershipCard: **Gold**

All three populated by a single `LoadAccountAction` dispatch from the parent.

### Task 5: Test Shared State

Open `Services/MockAccountService.cs`. Change `OrderCount: 12` to `OrderCount: 99` and `MembershipTier: "Gold"` to `"Platinum"`. Run again. Orders shows 99 and Membership shows Platinum -- both from the same single `LoadAccountAction` dispatch. One service call, one state update, three subscribed cards.

Change it back to `0` when done.

### Task 6: Add the Loading/Error Conditional Block

The bare `@AccountState.Value.OrderCount` binding shows a raw `0` during the 300ms service delay. The demo showed how to wrap the card body in a three-state conditional that renders loading, error, and value states separately.

In each card file, replace the bare binding with the conditional block:

```razor
@if (AccountState.Value.IsLoading)
{
    <p class="text-muted">Loading...</p>
}
else if (AccountState.Value.ErrorMessage is not null)
{
    <p class="text-danger">@AccountState.Value.ErrorMessage</p>
}
else
{
    <p class="display-4 text-primary">@AccountState.Value.OrderCount</p>
    <p class="text-muted">orders placed</p>
}
```

For `WishlistCard.razor` use `text-success`, `WishlistCount`, and caption "saved items". For `MembershipCard.razor` use `text-info`, `MembershipTier`, and caption "membership level". The shape is identical — only the Bootstrap colour class, bound property, and caption text change.

This is the shape the finished exercise-1 reference ships.

### Verify it works

Run the project (F5) and navigate to the Account page. All three cards should show live data from the store — confirm the values match what `MockAccountService` returns. Then test the shared-state proof (Task 5): changing what `MockAccountService` returns should update the matching cards on the next run, from a single dispatch.

If a card shows the `--` placeholder and never updates, check:

- Did you add `@inherits FluxorComponent`? (Without it the card never re-renders on state change)
- Did you add `@inject IState<AccountState>`? (Without it there is no state to read)
- Did you add `@using Fluxor`? (Without it `IState<T>` is not recognized)

### Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Card shows `--`, never updates | Missing `@inherits FluxorComponent` | Add the inheritance directive |
| `IState<T>` not recognized | Missing `@using Fluxor` | Add the using directive |
| `AccountState` not recognized | Missing using for the store namespace | Check `_Imports.razor` — it already includes `@using Day06Practice.Features.Account.Store`, so `AccountState` is in scope globally. You do not need to add a separate `@using` in the card file. |
| Compile error on `@inject AccountState` | Injecting the class instead of `IState<>` | Use `@inject IState<AccountState> AccountState` |

---

## Practice 2: Service-to-Result<T> Refactor -- Independent (25 min)

Real services have two kinds of failures. Infrastructure failures (the database is down, the network timed out) are genuinely exceptional — they should throw. Business-logic failures (the product ID was zero, the name is empty, the price is negative) are *expected outcomes* of normal operations — they shouldn't throw, because they're not surprises. Production codebases return `Result<T>` for business outcomes and reserve exceptions for infrastructure.

The Result<T> pattern makes the success-or-failure contract explicit: every public method returns a container with either a value or a list of errors, and the caller has to check. Multi-error accumulation in `AddAsync` is the deeper move — instead of throwing on the first failure, the method collects all errors and returns them together, so a form can show "name is required" and "price must be positive" in one round trip.

**The demo refactored this exact shape at the end of the demonstration** -- the Refactor Service to `Result<T>`, Refactor Effect, and Multi-Error Accumulation Preview steps -- for the Dashboard service. The pattern you're applying to ProductService is identical: `var result = new Result<T>()`, `.AddError()` on validation failure, `.WithValue()` on success, `if (result.IsSuccess)` in the effect.

You'll know it works when `GetByIdAsync(0)` returns a Result with the validation error instead of throwing, `AddAsync(new Product(0, "", -5))` returns a Result with *both* the name and price errors (multi-error accumulation), and the effect's try/catch is replaced with an `if (result.IsSuccess)` / `else` check. Effect skeleton and services are pre-built; you change three return types and three method bodies.

### What's already built

- `Services/IProductService.cs` -- interface with three methods returning `Task<Product>`
- `Services/ProductService.cs` -- implementation that throws exceptions on validation failure
- `Features/Product/Store/ProductEffects.cs` -- effect with try/catch calling `GetByIdAsync`

### Recall before you start

Remember that `Result<T>` is the BYSResult envelope (defined in the Guide's Core Concepts section, item 1) and `Error` is one problem inside it with a `Category` and a `Message` (defined in the Guide's Core Concepts section, item 2). The two methods you will use most below — `result.AddError(new Error(...))` and `result.WithValue(value)` — are also defined in the Guide's Core Concepts section, item 1. If either feels unclear, re-read those two sections before continuing.

### Method 1: GetByIdAsync (straightforward)

**Files:** `Services/IProductService.cs` and `Services/ProductService.cs`

This method currently throws two exceptions:

- `ArgumentException("Product ID must be greater than zero")` when `id <= 0`
- `InvalidOperationException("Product not found")` when the product doesn't exist

**Your tasks:**

1. Add `using BYSResults;` at the top of both files
2. Change the return type in the interface from `Task<Product>` to `Task<Result<Product>>`
3. Change the return type in the implementation to match
4. Create a `Result<Product>` at the start of the method: `var result = new Result<Product>();`
5. Replace `throw new ArgumentException(...)` with `result.AddError(new Error("Validation", "Product ID must be greater than zero"))` and `return result;`
6. Replace `throw new InvalidOperationException(...)` with `result.AddError(new Error("Not Found", ...))` and `return result;`
7. Replace the final `return product;` with `return result.WithValue(product);`

**Hint:** The pattern is always the same -- create the container, add errors if validation fails, return the container with or without a value.

### Method 2: AddAsync (multi-error accumulation)

This method currently throws on two conditions: empty name and non-positive price. The key difference: **collect both errors before returning**.

**Your tasks:**

1. Change the return type to `Task<Result<Product>>`
2. Replace both `throw` statements with `result.AddError(...)` -- but do NOT add `return result;` after the first one
3. After both validation checks, add: `if (result.IsFailure) return result;`
4. Replace the final `return newProduct;` with `return result.WithValue(newProduct);`

**Why this matters:** With try/catch, the first failure throws and the user only sees one error. With Result<T> accumulation, both errors come back at once. The user sees "name is required" AND "price must be greater than zero" in one response.

### Method 3: UpdateAsync (optional — complete after class if time runs out)

This method has three validation paths: invalid ID, empty name, and product not found. Apply the same pattern. **If you're running short on time, skip this and come back to it after class.**

### After refactoring services: Update the Effect

**File:** `Features/Product/Store/ProductEffects.cs`

Replace the entire try/catch block with Result<T> checking. Remove both the `try` and the `catch`, and replace the whole structure with:

1. Change `var product = await productService.GetByIdAsync(action.ProductId);` to `var result = await productService.GetByIdAsync(action.ProductId);`
2. Replace the entire try/catch structure with two `if` statements. One tests `result.IsSuccess` and dispatches the success action carrying the result's value. The other tests `result.IsFailure` and dispatches the failure action carrying the result's errors joined into a single string.
   Remove the try and catch keywords and their braces entirely — the two `if` statements replace the whole block.

   **Note:** `result.Value` is nullable, so the success dispatch needs the null-forgiving `!` operator — you have already checked `IsSuccess`, so you know a value is there.

### Success criteria

- `GetByIdAsync(0)` returns a Result with error "Product ID must be greater than zero"
- `GetByIdAsync(99)` returns a Result with error "Product with ID 99 not found"
- `GetByIdAsync(1)` returns a Result with Value = Wireless Mouse product
- `AddAsync(new Product(0, "", -5))` returns a Result with **two** errors (name + price)
- The effect uses `result.IsSuccess` instead of try/catch

### Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| `Result<T>` not recognized | Missing `using BYSResults;` | Add the using directive at the top of the file |
| Service still throws exception | Forgot to remove the `throw` statement | Replace `throw` with `result.AddError()` + `return result;` |
| AddAsync returns only one error | Returning after the first error | Don't return after the first `AddError()` -- check both, then check `result.IsFailure` |
| `NullReferenceException` on `result.Value` | Accessing Value without checking IsSuccess | Add `if (result.IsSuccess)` guard before accessing `.Value` |
| Nullable warning on `result.Value` | The compiler can't infer non-null from your `IsSuccess` check | Use `result.Value!` after the guard |
| Effect still has try/catch | Forgot to remove the wrapper | Remove the try/catch entirely and use if/else on the result |

---

## What You Learned

After completing these practices, you can:

- **Wire multiple components to shared state** -- three lines per component (`@using Fluxor`, `@inherits FluxorComponent`, `@inject IState<T>`) and all components update from a single dispatch
- **Refactor services from exceptions to Result<T>** -- create the container, add errors with `.AddError()`, return success with `.WithValue()`, return failure by returning the result with errors
- **Accumulate multiple errors** -- check all validations before returning, giving the caller the full picture instead of one error at a time
- **Update effects to use Result<T>** -- replace try/catch with `result.IsSuccess` / `result.IsFailure` checks

## What's Next

**Day 7: Route Guards and Navigation with State** -- protecting pages based on Fluxor store values. Does the user have items in their cart? Is the user authenticated? Route guards read the store to make these decisions.
