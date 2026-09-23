# Practice Walkthrough -- Day 4: Fluxor Actions and Reducers Exercises

**What this is:** A guided companion for your Day 4 practice exercises. Practice 1 is fully guided. Practice 2 gives direction without giving away the answer. Practice 3 is a take-home challenge. The Reducer Testing section walks you through writing your first unit test.

**Your project:** Open the `Day04Practice` starter project in Visual Studio. Fluxor is already installed and configured. The UI for all exercises is pre-built.

**How to find what needs changing:** Search for `TODO` in the project (Ctrl+Shift+F in Visual Studio). Every place you need to write code is marked with a `TODO` comment.

**Before you start:** Several checks below use **Redux DevTools** (press **F12** in the browser, then the **Redux** tab). This is a free browser extension you install once -- for Edge, Chrome, or Firefox. If you have not installed it yet, see the "One-time setup" note at the top of the Day 4 demo walkthrough.

---

## Practice 1: Counter Store -- Guided (10 min)

**You just saw the 5-file Fluxor pattern in the demo** — State, Actions, Reducers, Feature, component wiring — built end-to-end. This practice extends two of those files (Actions and Reducers) plus one component, leaving State, Feature, and the Increment reducer untouched.

Every real app has a counter-shaped feature — a cart-item badge, a notification bell with an unread count, a like button. The reason this trivial example earns class time is that *extending an existing store* is the most common Fluxor task you'll do in real work. Most days in a real project, you're adding actions to a store that already exists, rarely creating one from scratch.

The key pattern here is small but essential: parameterless actions are *signals*, and a reducer that ignores `state.Count` entirely (Reset always returns 0) teaches you that reducers decide what an action *means*, not actions themselves. The action just says "reset happened"; the reducer chooses what reset *does*.

You'll know it works when Reset returns the count to 0 from anywhere, the "Counter is at zero" alert toggles as the count crosses zero, and Redux DevTools shows all three action types. UI and existing wiring are pre-built — you add one action record, two reducer methods, two click handlers, and a 6-line `@if` block.

> **How this practice runs.** This is the first hands-on Fluxor exercise, so your instructor leads the **first ~5 minutes** — the class completes Task 1 together (adding `ResetCounterAction`) and sees the `[ReducerMethod]` callout explained live. After that you continue **on your own** — Tasks 2, 3, and 4 are independent. The guided section takes you through one action record so that Task 2 (the reducer stubs) and Task 3 (the button wiring) have a completed starting point.

### What's already built

The counter store is 80% complete:

- `CounterState.cs` -- complete (record with `Count` property)
- `CounterFeature.cs` -- complete (initial state with Count = 0)
- `CounterActions.cs` -- `IncrementCounterAction` and `DecrementCounterAction` defined; `ResetCounterAction` is a TODO stub
- `CounterReducers.cs` -- Increment reducer complete; Decrement and Reset are TODO stubs
- `Counter.razor` -- partially wired; Increment button works; Decrement, Reset, and conditional message are TODO stubs

### Task 1: Add ResetCounterAction

**File:** `Features/Counter/Store/CounterActions.cs`

Add the reset action after the existing actions:

```csharp
public record ResetCounterAction;
```

This is a parameterless signal -- "reset the counter." The reducer will decide what reset means (setting count to 0).

Before Task 2, you need to know what **`[ReducerMethod]`** is. `[ReducerMethod]` is the Fluxor attribute that marks a static method as a reducer. At application startup, Fluxor scans every type for methods carrying this attribute and registers each one to handle the action type in its second parameter. Without `[ReducerMethod]`, the method still compiles but Fluxor never sees it, so dispatching the matching action has no effect on state. In hand-written state management, a switch statement would route each action to the right handler. The `[ReducerMethod]` attribute replaces that switch statement — Fluxor uses it to find and register each reducer automatically.

### Task 2: Add Decrement and Reset Reducers

**File:** `Features/Counter/Store/CounterReducers.cs`

Replace the TODO stubs with:

```csharp
[ReducerMethod]
public static CounterState ReduceDecrementCounterAction(
    CounterState state,
    DecrementCounterAction action)
{
    return state with { Count = state.Count - 1 };
}

[ReducerMethod]
public static CounterState ReduceResetCounterAction(
    CounterState state,
    ResetCounterAction action)
{
    return state with { Count = 0 };
}
```

Notice: the Reset reducer doesn't use `state.Count` at all -- it always returns 0 regardless of the current count.

### Task 3: Wire Decrement and Reset Buttons

**File:** `Components/Pages/Counter.razor`

In the `@code` block, replace the TODO stubs:

```csharp
private void Decrement()
{
    Dispatcher.Dispatch(new DecrementCounterAction());
}

private void Reset()
{
    Dispatcher.Dispatch(new ResetCounterAction());
}
```

### Task 4: Add Conditional Message

**File:** `Components/Pages/Counter.razor`

In the markup, add below the button group:

```razor
@if (CounterState.Value.Count == 0)
{
    <div class="alert alert-info mt-3">
        Counter is at zero. Try incrementing!
    </div>
}
```

### Verify it works

- Increment, Decrement, and Reset buttons all update the count
- Reset always returns to 0
- The alert appears when count is 0 and disappears when it's not
- Redux DevTools (F12 -> Redux tab) shows all three action types

---

## Practice 2: Todo List -- Independent (27 min)

**You just saw the demo wire all five files for the counter.** This practice asks you to wire all five for the todo list — Actions, Reducers, Feature, and component, with the State record provided.

The todo list is the canonical "hello world" of state management because it covers the three actions in every real CRUD store: add an item, remove an item, mutate one property on an existing item. Trello cards, Linear tickets, Spotify playlists, your email inbox — they're all just todo lists with prettier UI. Once you can build this, you can build any feature whose state is "a list of things."

The key pattern: this is the 5-file Fluxor pattern repeated for a new feature. Same shape as the counter, different domain, list-shaped state instead of an int, and one new difficulty — the reducers do non-trivial LINQ work (`.Where`, `.Select`, `.Max`) because the state is a list. Pure-function discipline still applies: every reducer returns a *new* list, never mutates `state.Items`.

You'll know it works when Add inserts a row, the checkbox flips that row's strikethrough without disturbing others, delete removes only the targeted row, and Redux DevTools shows three action types with their data (Title for Add, Id for Remove and Toggle). UI is pre-built, and so are `ToggleTodoAction` and its reducer — you write two action records (Add, Remove), two reducer methods, one feature class, and the dispatch wiring for all three actions.

### What's already built

- `Features/Todo/Models/TodoItem.cs` -- `record TodoItem(int Id, string Title, bool IsComplete)`
- `Features/Todo/Store/TodoState.cs` -- record with `Items` property (`IReadOnlyList<TodoItem>`)
- `Features/Todo/Store/TodoActions.cs` -- TODO comments for the two actions you write, plus the pre-built `ToggleTodoAction(int Id)` record
- `Features/Todo/Store/TodoReducers.cs` -- TODO comments for Add and Remove, plus the pre-built `ReduceToggleTodoAction` (your worked example)
- `Features/Todo/Store/TodoFeature.cs` -- empty class skeleton
- `Components/Pages/Todo.razor` -- the form and instructions are built; the list markup (`@foreach`, checkbox, strikethrough, delete button) is supplied **commented out** at `Todo.razor:33-52` -- restore it once your state is wired. The `@code` block has TODO stubs.

### What you need to build

**1. TodoActions.cs** -- Define two action records:

| Action | Data it carries | What it means |
|--------|----------------|---------------|
| `AddTodoAction` | `string Title` | User typed a title and clicked Add |
| `RemoveTodoAction` | `int Id` | User clicked delete on a specific item |

`ToggleTodoAction(int Id)` is already in the file -- it ships pre-built with its reducer.

**2. TodoReducers.cs** -- Implement two reducer methods:

| Reducer | What it does |
|---------|-------------|
| `ReduceAddTodoAction` | Append a new `TodoItem` to the list with an auto-incremented Id |
| `ReduceRemoveTodoAction` | Filter out the item matching the given Id |

`ReduceToggleTodoAction` is already written. Read it before you start -- it is the worked
example for building a new list instead of editing the old one.

**Hints for reducer implementations:**

- **Add:** Create a new `TodoItem` with an incremented Id, then return the list with that item appended.
    - To calculate the next Id: `state.Items.Count > 0 ? state.Items.Max(t => t.Id) + 1 : 1`
    - To append immutably: `state with { Items = [.. state.Items, newItem] }`. The `[ ]` is a **collection expression** and the `..` is a **spread** -- it copies the existing items into a *new* list, then adds one more. This is the immutable way to "add to a list": you never change the old list. See *Modern C# You'll Meet Here* Section 3.2.
- **Remove:** Use `.Where(t => t.Id != action.Id).ToList()` to create a new list without the item.
    - `.ToList()` returns `List<T>`, which implements `IReadOnlyList<T>`, so the assignment is type-compatible — no cast required.
- **Toggle:** already written for you. Open `ReduceToggleTodoAction` and read how it builds a new list rather than editing the existing one -- Remove uses the same idea with a different LINQ method.

Before you write `TodoFeature`, you need to know what **`Feature<T>`** is. `Feature<T>` is a Fluxor base class that registers one state record with the store. The two methods you override give Fluxor (1) a unique name for the feature (`GetName()`) and (2) the starting value when the application boots (`GetInitialState()`). Every state record in the application has exactly one corresponding `Feature<T>` class — without it, Fluxor never registers the state, and any component that injects `IState<TodoState>` throws "No feature found" at runtime.

**3. TodoFeature.cs** -- Implement `Feature<TodoState>`:

- `GetName()` returns `"Todo"`
- `GetInitialState()` returns a `TodoState` with an empty items list

Before you wire `Todo.razor`, you need to know what **`@inherits FluxorComponent`** does. `@inherits FluxorComponent` is a Razor directive that makes the component subscribe to every `IState<T>` it injects. The base class listens for Fluxor's "state changed" notification so the moment a reducer returns a new state, the component re-renders. Without it, the component reads state once when first rendered and never updates — the page goes stale on the next dispatch (the symptom in the Stuck section: "Component renders but doesn't update").

**4. Todo.razor wiring:**

- Add `@inherits FluxorComponent`
- Inject `IState<TodoState>` and `IDispatcher`
- Uncomment the list markup at `Todo.razor:33-52` and delete the placeholder alert above it
- Wire the Add button to dispatch `AddTodoAction`
- Wire delete buttons to dispatch `RemoveTodoAction`
- Wire checkboxes to dispatch `ToggleTodoAction`

### Success threshold

**If you complete `AddTodoAction`, its reducer, `TodoFeature`, and the Add component wiring -- that's the core objective.** Toggle is pre-built; Remove extends the same pattern. Aim for both, but Add proves you understand the Fluxor cycle.

### Verify it works

- Type a title and click Add -- item appears in the list
- Click the checkbox -- text gets strikethrough (or styling changes)
- Click delete -- item disappears
- Redux DevTools shows all three action types with correct data payloads

---

## Your First Reducer Test (10 min)

Reducers are pure static functions. You can test them without Fluxor running -- just call the method and check the result.

### What's already built

The test project (`Day04Practice.Tests/`) has one completed sample test:

```csharp
[Fact]
public void ReduceIncrementCounterAction_WithCountOf5_ReturnsCountOf6()
{
    // Arrange
    var initialState = new CounterState { Count = 5 };
    var action = new IncrementCounterAction();

    // Act
    var newState = CounterReducers.ReduceIncrementCounterAction(
        initialState, action);

    // Assert
    Assert.Equal(6, newState.Count);
}
```

**The pattern:** Arrange a state and an action. Act by calling the reducer as a plain static method. Assert the returned state.

### Write these two tests

`CounterReducerTests.cs` has a `// TODO` marker for each one. Fill them in, following the Arrange / Act / Assert shape of the sample test above.

**Test 1 — `ReduceDecrementCounterAction_WithCountOf3_ReturnsCountOf2`**

- Arrange a `CounterState` with `Count = 3` and a `DecrementCounterAction`
- Act by calling `CounterReducers.ReduceDecrementCounterAction`
- Assert the returned state's `Count`

**Test 2 — `ReduceResetCounterAction_WithCountOf10_ReturnsCountOf0`**

- Arrange a `CounterState` with `Count = 10` and a `ResetCounterAction`
- Act by calling `CounterReducers.ReduceResetCounterAction`
- Assert the returned state's `Count`

Think about the Reset assertion before you write it. The reducer never looks at the starting count -- so ask yourself what the expected value is, and whether arranging `10` instead of `3` would change your answer. The test name is telling you.

Run the tests: open **Test Explorer** in Visual Studio (`Test > Test Explorer`, or `Ctrl+E, T`) and choose **Run All Tests**. All three should pass -- the Increment sample that was already there, plus the two you just wrote. If one is red, right-click it and choose **Debug**: put a breakpoint in your reducer first, which is the quickest way to see what the test actually expects.

### Test naming convention

`MethodName_Scenario_ExpectedResult`

- `ReduceDecrementCounterAction` -- the method being tested
- `WithCountOf3` -- the starting condition
- `ReturnsCountOf2` -- what you expect

### If you finish early

Write a third test, this time for one of your **Todo** reducers.

The reason this is worth doing: a Counter reducer returns a number, so one `Assert.Equal` checks it. A Todo reducer returns a **list**. You assert about the collection, and then about an item inside it.

Name it `ReduceAddTodoAction_WithEmptyState_ReturnsListWithOneItem`, and follow the same Arrange / Act / Assert shape. Arrange an empty `TodoState` and an `AddTodoAction`; act by calling `TodoReducers.ReduceAddTodoAction`; then assert on the returned list.

The assertion is the part worth thinking about. A Counter reducer returns a number, so one `Assert.Equal` covers it. A list needs you to decide **how many** things to check — how long is the list now, and is the item you added actually the item that came back, with the right title and the right completion state? Three assertions is a reasonable answer. `Assert.Single` is worth looking up.

Two mistakes students often make here:

- `CounterReducerTests.cs` imports only the Counter store. Add `using Day04Practice.Features.Todo.Store;` and `using Day04Practice.Features.Todo.Models;` at the top of the file. The second one is for `TodoItem`.
- `new TodoState()` already gives you an empty `Items` list. There is no setup to write.

**Key insight:** No Fluxor setup, no components, no browser. Reducers are just functions. Input → output → assert. In Lab 1, you'll write 3+ tests like this for your feature's reducers.

---

## Practice 3: Cart Refactor -- Take-Home Challenge

**This is the same 5-file Fluxor pattern from the counter demo and your todo-list practice, applied to the cart.**

In Day 2, you saw the shopping cart's state disappear when navigating between Products and Cart — two private lists, items lost on every route change. In Day 3, you diagrammed on paper what the fix would look like: state moved into a centralized store, components subscribing instead of owning. Today, with the Fluxor 5-file pattern in your hands, you actually do it.

The key pattern: this isn't new — it's the *same* 5-file pattern from the counter and the todo list, applied to the cart. The proof the pattern works is that the code that broke on Day 3 (navigate to Cart, find it empty) now succeeds, *because* state lives outside the components.

The demo previewed this in the "Connection to Day 3" section — "If you added a second page that also injected `IState<CounterState>`, both pages would see the same count." That's what you're doing for the cart: Products dispatches `AddToCartAction`, Cart subscribes to `IState<CartState>`, both see the same items because the store survives navigation.

You'll know it works when adding items on Products, navigating to Cart, then back to Products preserves the cart through every route change — Day 3's failure mode no longer reproducible. State, Feature, and AddToCart are pre-built; you complete the RemoveFromCart and UpdateQuantity actions, both reducers, and the Cart page's delete-button and quantity-input wiring.

### What's already built (about half)

- `CartState.cs` -- complete (record with `Items`, computed `TotalPrice`/`TotalItems`)
- `CartFeature.cs` -- complete
- `CartActions.cs` -- `AddToCartAction` defined; `RemoveFromCartAction` and `UpdateQuantityAction` are TODO stubs
- `CartReducers.cs` -- AddToCart reducer complete; RemoveFromCart and UpdateQuantity are TODO stubs
- `Products.razor` -- Fluxor partially wired (add button works)
- `Cart.razor` -- UI complete; the remove button and the quantity input are TODO stubs

### What you need to complete

1. **Add `RemoveFromCartAction(int ProductId)`** in `CartActions.cs`

2. **Add the Remove reducer** in `CartReducers.cs` -- filter out the item by ProductId (same `.Where()` pattern as Todo Remove)

3. **Wire the remove button** in `Cart.razor` -- dispatch `RemoveFromCartAction`

4. **Add `UpdateQuantityAction(int ProductId, int NewQuantity)`** in `CartActions.cs`

5. **Add the UpdateQuantity reducer** in `CartReducers.cs` -- this is the *same* `.Select(... ? item with { ... } : item)` copy-replace shape the pre-built Todo toggle reducer showed you, with `Quantity = action.NewQuantity` instead of a flipped flag. Recognising that you can reuse that same pattern here is the point of this task.

6. **Wire the quantity input** in `Cart.razor` -- dispatch `UpdateQuantityAction`

### The key test

Navigate between Products and Cart. **Cart data should persist across navigation.** This is Day 3's state-loss problem solved -- state now lives in the Fluxor store, not in component fields.

Also check the quantity box: change a quantity and the row subtotal and the cart total both update. If the number changes but the totals do not, `UpdateQuantityAction` is not wired.


### Complete before Day 5

This exercise reinforces everything from today. Day 5 builds on this foundation by adding effects (async operations).

---

## Stuck?

- **Search for TODOs:** Ctrl+Shift+F → search `TODO`
- **Check the reference sheet:** Syntax for `record`, `[ReducerMethod]`, `@inherits FluxorComponent`, `IState<T>`, `IDispatcher`, and the `with` expression
- **Check the demo walkthrough:** Shows the complete five-file pattern with explanations
- **Common issues:**
  - Component renders but doesn't update → Missing `@inherits FluxorComponent`
  - "No feature found" exception → Missing or misconfigured `Feature<T>` class
  - State doesn't change on dispatch → Missing `[ReducerMethod]` attribute, or parameter types don't match
  - Reducer compiles but wrong result → Mutating state instead of using `with`
  - `NullReferenceException` on `State.Value` → `GetInitialState()` returns null or doesn't initialize all properties
- **Ask the instructor:** They are available for questions.
