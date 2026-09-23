# Practice Walkthrough -- Day 5: Fluxor Effects and Async Operations

**What this is:** A companion for your Day 5 practices. Practice 1 is a worked launch -- the instructor walks the first step on screen, then you recall and complete the rest from the demonstration you just watched. Practice 2 gives direction without giving away the answer. Practice 3 is a challenge with hints.

**Your project:** Open the `Day05Practice` starter project in Visual Studio. The project has Day 4's working todo store (add, remove, toggle) plus empty stubs for the new async loading features. (This is the student practice project — separate from the `Day05Demo` project the instructor used for the in-class demonstration.)

**How to find what needs changing:** Search for `TODO` in the project (Ctrl+Shift+F in Visual Studio). Every place you **change existing code** is marked with a `TODO` comment. **Five files do not exist yet and you create them from scratch** — `TodoEffects.cs` (Practice 1, Task 3) and the four Product store files `ProductActions.cs`, `ProductReducers.cs`, `ProductEffects.cs`, `ProductFeature.cs` (Practice 2, items 2–5). They have no marker because there is no file to mark. Each task below names them explicitly.

---

## Practice 1: Todo Load Effect (worked launch, 15 min)

Real applications do not store data directly in the code — a task app fetches your tasks from a server, a shopping app loads products from a database, a mail app pulls messages from a server. Every screen showing server data has three on-screen states: a loading spinner while the request is in flight, an error alert if it fails, and the data when it arrives.

The triple-action pattern (Load → Success / Failure) is the universal shape for async state in Redux-style stores. State models the lifecycle: `IsLoading`, `ErrorMessage`, `Items` — three properties the UI keys off to decide what to render. The pure-function rule from Day 4 still holds; the new piece is *who calls the async service* — a new file type called an effect.

**This is the same triple-action pattern your instructor just demonstrated on the Product catalog** — three actions, three reducers, the effect class with constructor DI, Todo domain instead of Products. Because you watched it built step by step, this walkthrough names *which* records/methods each file needs and *what each does*, but doesn't print the code. If you get stuck, the <a href="Day-05-Demonstration-Walkthrough.html?isCourseFile=true" target="_blank" rel="noopener">Day 5 — Demo Walkthrough</a> has the full Product version.

You'll know it works when the Todos page shows a spinner for ~500ms, three todos appear, Add/Remove/Toggle still works, and Redux DevTools shows `LoadTodosAction` → `LoadTodosSuccessAction`. State (including the `IsLoading` and `ErrorMessage` properties), Feature, Day 4's reducers, the mock service, and the loading/error display in `Todo.razor` are pre-built — you add three actions, three reducers, one effect, plus the `OnInitialized` dispatch. The loading spinner and error alert markup are provided on purpose: this course teaches the C# backend that drives those states, not the Blazor markup itself.

> **How this practice runs.** The triple-action effect pattern is familiar from the demonstration, so your instructor runs Task 1 as a **worked launch** — they walk adding the three Load actions on screen in `TodoActions.cs` (trigger + success + failure records, matching the Product shape you just saw). After Task 1 you continue **on your own**. All four tasks and their independence status: Task 1 (Add Load Actions) — **instructor demos live, you follow along**; Task 2 (Add Load Reducers) — **independent**; Task 3 (Create TodoEffects.cs) — **independent**; Task 4 (Wire Todo.razor) — **independent**, the `OnInitialized` dispatch only (the loading/error display is pre-built).

### What's already built

The todo feature is mostly complete from Day 4:

- `TodoState.cs` -- record with `Items`, `NextId`, and the async-lifecycle properties `IsLoading` and `ErrorMessage` (all pre-built)
- `TodoActions.cs` -- `AddTodoAction`, `RemoveTodoAction`, `ToggleTodoAction` (load actions are TODO stubs)
- `TodoReducers.cs` -- add/remove/toggle reducers working (load reducers are TODO stubs)
- `TodoFeature.cs` -- complete
- `Todo.razor` -- wired for add/remove/toggle; the loading spinner and error alert are pre-built; only the `OnInitialized` dispatch is a TODO stub
- `ITodoService`/`MockTodoService` -- registered and working

### Task 1: Add Load Actions

**File:** `Features/Todo/Store/TodoActions.cs`

After the existing Day 4 actions, add three new action records that follow the triple-action pattern from the demonstration. You need:

- A **trigger action** (`LoadTodosAction`) -- carries no data; the component dispatches this to signal "start loading"
- A **success action** (`LoadTodosSuccessAction`) -- carries a `List<TodoItem>` parameter named `Items`; the effect dispatches this when data arrives
- A **failure action** (`LoadTodosFailureAction`) -- carries a `string` parameter named `Error`; the effect dispatches this on exception

Recall the record syntax for primary-constructor actions from the demonstration. Each is a `public record` with the appropriate parameter (or no parameters for the trigger).

### Task 2: Add Load Reducers

**File:** `Features/Todo/Store/TodoReducers.cs`

The `IsLoading` and `ErrorMessage` properties are already on `TodoState` (pre-built) — the same `IsLoading` / `ErrorMessage` pattern you saw on `ProductState` in the demonstration. Your reducers set them. After the existing Day 4 reducers, add three `[ReducerMethod]` static methods — one for each of the three load actions. Recall from the demonstration:

- The **trigger reducer** (`ReduceLoadTodosAction`) sets `IsLoading = true` and clears any previous error
- The **success reducer** (`ReduceLoadTodosSuccessAction`) sets `Items` from the action's data and sets `IsLoading = false`
- The **failure reducer** (`ReduceLoadTodosFailureAction`) sets `ErrorMessage` from the action's error and sets `IsLoading = false`

Each uses the `state with { ... }` expression — the same pure pattern as Day 4's reducers, never mutating state directly.

Before Task 3, you need to know what **`[EffectMethod]`** is. `[EffectMethod]` is the Fluxor attribute that turns a method into an effect. At startup Fluxor scans for it the same way it does `[ReducerMethod]` and wires the method to handle any action whose type matches the method's first parameter. The differences from a reducer: the method returns `Task` (effects do async work — calling services, fetching data, talking to APIs), is typically `async`, and receives an `IDispatcher` as its second parameter so it can fire follow-up actions when the work completes (the success / failure pair of the triple-action pattern). Without `[EffectMethod]`, the method compiles but Fluxor never runs it.

Before Task 3 you also need to know what a **primary constructor** is. A primary constructor is C# 12+ syntax that declares constructor parameters on the class header itself, like `public class TodoEffects(ITodoService todoService)`. The parameter is in scope throughout the class body — you do not write a separate constructor body and you do not need a `private readonly _todoService` field. It is the shorter way to write classic constructor-DI. (Your prior NAIT semesters used .NET 8, which supports primary constructors, but most prior coursework used the traditional constructor form — so the syntax may be the first time you write it yourself.)

### Task 3: Create TodoEffects.cs

**Create new file:** `Features/Todo/Store/TodoEffects.cs`

This is the new piece. Create an effect class that mirrors `ProductEffects` from the demonstration:

- Use a **primary constructor** to inject `ITodoService` -- Fluxor resolves this from DI
- Add one `[EffectMethod]`-decorated `public async Task` method named `HandleLoadTodosAction` that receives `LoadTodosAction` and `IDispatcher`
- Inside: call `todoService.GetAllAsync()` in a try/catch; on success dispatch `LoadTodosSuccessAction` with the returned list; on exception dispatch `LoadTodosFailureAction` with the exception message

The namespace should be `Day05Practice.Features.Todo.Store`, and you will need a `using` for `Day05Practice.Services` so `ITodoService` resolves.

### Task 4: Wire Todo.razor

**File:** `Components/Pages/Todo.razor`

The component is already wired for Day 4's actions (FluxorComponent, IState, IDispatcher), and the loading spinner + error alert markup is pre-built — it already reads `TodoState.Value.IsLoading` and `TodoState.Value.ErrorMessage`. One TODO marker remains:

**OnInitialized:** Add the `OnInitialized` override to the `@code` block. Call `base.OnInitialized()` first, then dispatch `LoadTodosAction` via the injected `Dispatcher`. This kicks off the async load the moment the component mounts — the same `OnInitialized` dispatch pattern from `Products.razor` in the demonstration. Once your effect and reducers are in place, this dispatch makes the pre-built spinner and data states light up.

### Verify it works

Press F5. Navigate to the Todos page:

- A loading spinner should appear for ~500ms
- Three sample todos appear after the delay
- Existing Add/Remove/Toggle still works
- Open F12 → Redux tab: you should see `LoadTodosAction` followed by `LoadTodosSuccessAction` in the action log

### Troubleshooting

| Problem | Likely cause | Fix |
|---------|-------------|-----|
| Todos don't load | Missing `OnInitialized` or missing `base.OnInitialized()` | Add both |
| "Type not found" error | Missing `using` for services namespace | Check `_Imports.razor` includes `@using Day05Practice.Services` |
| Effect never runs | Method returns `void` instead of `Task` | Change to `async Task` |

---

## Practice 2: Product Catalog -- Independent (25 min)

The triple-action pattern can look simple when you watch a demonstration, but applying it independently takes more effort. This practice asks you to rebuild the demo from blank stubs — same Product domain, same files, same component wiring. Repeating the pattern intentionally is the point: you are building practice, not just reading.

The pattern beat: this is the full 6-file shape any real Fluxor feature requires — the original 5 from Day 4 plus the new `Effects` file. Every screen that loads server data follows this exact shape; once you can produce it from blank stubs in 25 minutes, you can build any data-loading feature for the rest of the course and for Lab 1.

**You just watched the instructor walk every line of this exact code** — `LoadProductsAction`, `LoadProductsSuccessAction(List<Services.Product> Products)`, `LoadProductsFailureAction(string Error)`, three reducers with `state with { ... }`, the effect with constructor DI and try/catch. Same names, same shape, same domain. This time, you type it.

You'll know it works when the Products page shows a 1-second spinner, then five cards render with name, currency-formatted price, and category badge, and Redux DevTools shows the full `LoadProductsAction` → `LoadProductsSuccessAction` trace. Service infrastructure and the status chrome (spinner, error alert) are pre-built — you create four files in `Features/Product/Store/`, fill in the existing `ProductState.cs`, then wire `Products.razor`: the three directives, the two state conditions, the product-card `@foreach`, the Refresh button, and the `OnInitialized` dispatch. The card markup is yours to write; `Day 5 — Demo Walkthrough` Step 7 shows the shape.

### What's already built

- `IProductService`/`MockProductService` -- registered, returns 5 products with 1-second delay
- `Products.razor` -- the loading spinner and error alert are pre-built (behind TODO placeholders you flip to real state); the product-card markup inside the data section is yours to write
- `ProductState.cs` -- exists in `Features/Product/Store/` but has empty body (TODO)

### What you need to add or create

1. **`ProductState.cs`** -- Add three properties to the existing file:
   - `List<Services.Product> Items` (default: empty list) — same `Services.` qualifier as the action below, and for the same reason
   - `bool IsLoading` (default: false)
   - `string? ErrorMessage` (default: null)

2. **`ProductActions.cs`** (new file in `Features/Product/Store/`):
   - `LoadProductsAction` (trigger, no parameters)
   - `LoadProductsSuccessAction(List<Services.Product> Products)` (success) — the `Services.` qualifier is required because the store namespace (`Day05Practice.Features.Product.Store`) contains "Product" as a segment; the compiler needs the qualifier to find the right type (same reason as the demo)
   - `LoadProductsFailureAction(string Error)` (failure)

3. **`ProductReducers.cs`** (new file):
   - Three `[ReducerMethod]` methods (`ReduceLoadProductsAction`, `ReduceLoadProductsSuccessAction`, `ReduceLoadProductsFailureAction`) -- one per action
   - Same pattern as Practice 1's todo reducers

4. **`ProductEffects.cs`** (new file):
   - Constructor DI for `IProductService`
   - One `[EffectMethod]` method, `HandleLoadProductsAction`, with try/catch
   - Dispatches success or failure

5. **`ProductFeature.cs`** (new file):
   - Extends `Feature<ProductState>`
   - `GetName()` returns `"Product"`
   - `GetInitialState()` returns new state with defaults

6. **Wire `Products.razor`**:
   - Add `@inherits FluxorComponent`, inject state and dispatcher
   - Replace TODO placeholders with real state bindings
   - Dispatch `LoadProductsAction` in `OnInitialized`
   - Add a "Refresh" button that re-dispatches `LoadProductsAction` (it is in the success criteria below — not optional)

### Hints (if stuck)

- **Namespace:** Use `Day05Practice.Features.Product.Store` for all store files
- **Product type:** `using Day05Practice.Services;` is what `ProductEffects.cs` needs for `IProductService`. It does **not** help with the `Product` type — inside the store namespace you must write `Services.Product`, because `Product` alone is a namespace there.
- **Feature class:** This is the most commonly forgotten file. Without it, your state will always have default values and no products will display.
- **Refresh button:** It's just a `<button>` that calls a method which dispatches `LoadProductsAction`.
    - The `ReduceLoadProductsAction` reducer already clears the error and sets IsLoading, so the full loading cycle repeats.

### Success Criteria

- Products page shows loading spinner for ~1 second, then 5 product cards
- Each card shows name, price (formatted as currency), and category badge
- Error state shows error message when service fails
- Refresh button triggers a new load cycle
- Redux DevTools shows full action trace

---

## Practice 3: Retry and Empty-State Handling -- Challenge (10 min)

Demo code shows the happy path (the expected success flow — no errors). Production code handles the unhappy paths — the failed fetch the user wants to retry, the empty result list that would render as a blank grid and look broken. Every shipping product catalog, every shipping inbox, every shipping dashboard has retry buttons and empty-state messages because users will hit those states and silently leave if you don't handle them.

Both improvements use existing tools from the store — no new actions required. Retry doesn't need a new action — `ReduceLoadProductsAction` already clears the error and re-flips `IsLoading`, so re-dispatching `LoadProductsAction` rolls the full loading cycle again. Empty state isn't a state-management problem at all; it's a UI affordance, an `@if (!Items.Any())` check in the markup. The lesson is recognizing which problems need new store machinery and which don't.

**The demo showed only the happy path** — spinner, then five products. The error path was demonstrated by throwing in the service, but no retry. This practice adds the error recovery features — a Retry button and an empty-state message — that a real application would need.

You'll know it works when the error state renders with both the error message AND a Retry button, clicking Retry transitions error → loading → data without losing the user's place, and an empty-products result renders "No products found" instead of an empty grid. Reuse `LoadProductsAction` for retry; add one `@if (!Items.Any())` check in markup.

### Retry Button

When the product catalog is in error state, add a "Retry" button below the error message. Its `@onclick` handler dispatches the action that starts a load.

> **Tip:** You can dispatch straight from the handler with the same inline lambda pattern you used for Day 4's toggle and remove buttons. A small private method called from `@onclick` works just as well. Both are fine.

When clicked, dispatch `LoadProductsAction` again. The `ReduceLoadProductsAction` reducer already clears the error and sets IsLoading, so the UI transitions cleanly: error → loading → data (or error again).

**Think about:** Do you need a new action for this, or can you reuse `LoadProductsAction`?

### Empty-State Handling

If products load successfully but the list is empty, show a "No products found" message instead of an empty grid. In the data section of `Products.razor`, add a branch that tests whether the items list has any entries. When it does not, render the message. When it does, render the product cards as before.

**To test the empty state:** Temporarily change `MockProductService.GetAllAsync()` to return an empty list: `return [];`

**To test the error state:** In the same method, replace the `return` with `throw new InvalidOperationException("Database connection failed");` — your effect's catch turns that into a `LoadProductsFailureAction` and the error alert (plus your Retry button) appears. Put the `return` back when you are done.

### Optional: ClearProductsErrorAction

For cleaner state transitions, create a separate `ClearProductsErrorAction` and its reducer, which clears the error message. Dispatch it immediately before the retry dispatch, so two actions fire in order instead of one.

This is optional -- dispatching just `LoadProductsAction` works because the `ReduceLoadProductsAction` reducer already clears the error. But the explicit clear makes the intent visible in Redux DevTools.

### Success Criteria

- Error state shows both the error message AND a Retry button
- Clicking Retry shows the loading spinner, then loads products
- Empty list shows "No products found" message
- Clean state transitions visible in Redux DevTools

---

## What You Learned

After completing these practices, you can:

1. Implement the **triple-action pattern** (Load/Success/Failure) for any async operation
2. Create **effect classes** with constructor DI and `[EffectMethod]`
3. Wire **loading and error state** into the Fluxor store and component UI
4. Trace the full async flow: Component → Action → Effect → Service → Result Action → Reducer → Store → Component

**Next class (Day 6):** You'll learn the BYSResult pattern -- a better way to handle service errors than try/catch in every effect. Read the BYSResult Pattern Guide (~20 min) linked from the Day 6 page in Brightspace before next class.
