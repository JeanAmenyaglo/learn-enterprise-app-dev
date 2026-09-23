# Day 3 — Activity Walkthrough

**What this is:** Today is a *reading and design* day — Activity 1 opens a read-only project in Visual Studio to investigate code on screen, Activity 2 is paper-and-pencil pair work, Activity 3 is small-group discussion. You do not write code today.

**Your project for Activity 1:** `Day03Explore` is the same shopping-cart app you just watched the instructor build — renamed so you observe it rather than edit it. Extract `Day-03-Demonstration-Finished.zip` from Brightspace and open the `Day03Explore` project in Visual Studio. **Read-only — do not modify any code.** Activities 2 and 3 do not need Visual Studio open.

**Vocabulary reminder.** Activities 2 and 3 use Fluxor vocabulary from your two pre-class readings: the `02-Bridge-To-Fluxor` reading (which maps events and delegates to Flux concepts) and the `03-Fluxor-Quick-Reference` guide (which defines the standard Fluxor names). If a term feels unclear, return to whichever reading covers it first:

- **action** — a message describing what happened
- **reducer** — a function that builds the new state from the old state plus an action
- **store** — the one place state lives
- **dispatcher** — the service that fires an action
- **state** — the data the application is holding right now

If any term feels unclear during the activities, return to the bridge reading first. The Key Concepts table at the bottom of this page is a one-line refresher for fast lookup.

---

## Activity 1: Guided Observation of the Demo App (15 min)

A new dev joins a team and the senior engineer says "before you write any state management code, just go observe the bug — read the code, click through the app, write down where the data lives." That's what you're doing with the shopping cart from this morning's demo. No editing, no fixing, just *seeing*.

Reading state-loss bugs is the foundation of every state-management technology. Flux, Redux, every observable store library exists because component-level state has predictable failure modes. If you can point at the line of code that explains why the cart empties — `private List<CartItem> cartItems = []` declared inside the wrong scope — you understand the problem the next four days will solve. Most "weird state bug" tickets in real codebases reduce to this exact misreading.

**You just watched the instructor demo this loss live** — add three items, navigate, find the cart empty, navigate back and find Products empty too. The demo gave you the *visual* reveal; this activity asks you to read the code that explains *why*.

You'll know you've gotten the point when you can answer Step 6 without re-reading: "What would need to change so both pages share the same cart data?" If you can name where the data should move and how each component would reach it, you're ready for Activity 2.

Open the `Day03Explore` project and run it (F5). Follow these 6 steps to investigate the state-loss problem from the inside.

The mechanism behind **state loss on navigation** — why it happens and what the fix looks like — is intentionally left out here. Activity 1 is a *discovery* sequence. Work through the six steps first, then read the reveal block after Step 6 to compare your reading with the mechanism.

### Step 1: Examine Products.razor

**Open:** `Components/Pages/Products.razor`

Find the `cartItems` field in the `@code` block.

**Questions to answer:**
- What type is `cartItems`? (Look at the declaration)
- Where does it live? (It's a `private` field inside the component)
- What scope does a `private` field have in a Blazor component?

**What you should see:** `private List<CartItem> cartItems = []` -- an empty list that belongs only to this component instance.

### Step 2: Examine Cart.razor

**Open:** `Components/Pages/Cart.razor`

Find its `cartItems` field in the `@code` block.

**Questions to answer:**
- Is this the same list as the Products page's `cartItems`?
- Is there any connection between the two lists?
- What would happen if you added items to one list -- would the other list know?

**What you should see:** Another `private List<CartItem> cartItems = []` -- a completely separate, independent list. Two components, two lists, no connection.

### Step 3: Run and Add Items

Run the app. Go to the **Products** page and click three *different* products (e.g., Wireless Mouse + USB-C Hub + Mechanical Keyboard) — the empty cart and the missing-state alert are clearest when each cart entry is unique.

Note the cart count in the blue alert at the bottom of the page. It should show your items with quantities and prices.

### Step 4: Navigate to Cart

Click **Shopping Cart** in the sidebar navigation.

**Question:** Is the cart empty or populated?

**What you should see:** The cart is empty. A warning alert explains why -- this is a different component with its own empty list.

### Step 5: Navigate Back to Products

Click **Products** in the sidebar navigation.

**Question:** Are your items still there, or is the cart count reset?

**What you should see:** The Products page is also empty. The original component instance (with your items) was destroyed when you navigated away. This is a brand new Products component with a fresh, empty list.

### Step 6: Discussion Question

Think about this before the class discussion:

> **What would need to change so that both Products and Cart share the same cart data?**

Consider:
- Where would the data need to live?
- How would both components access it?
- What would happen to the data when you navigate?

### Reveal: What the code says

Now that you have read both components and watched the cart empty itself, here is the mechanism in one paragraph. Compare it to what you wrote for Step 6.

When data is held inside a Blazor component — for example, the `private List<CartItem> cartItems` field on `Products.razor` — that data lives on the component *object*. Navigating to a different page destroys the component object, and the field goes with it. Coming back to the page later builds a brand new component object with a brand new empty list. The two components in this demo have two separate, independent lists for the same reason: each list belongs to a different component object. The bug is mechanical, not a logic error — the data was real, the field was real, the field is just gone because its owner is gone.

The fix is one of the things the rest of the week is about: move the data out of any single component and into something that outlives navigation. The **store** from your pre-class reading is that thing.

---

## Activity 2: Paper Flux Mapping (Pair Work, 25 min)

The hardest moment in adopting a state-management library isn't the syntax — it's the design decision *before* the code: what state lives where, what actions describe what users do, what each reducer's job is. Senior engineers do this on whiteboards before they touch a keyboard, because erasing a marker line costs nothing and refactoring a reducer costs a sprint.

Paper is the right tool for that reason. You're forcing yourself to externalize the shape of the data flow before any framework chooses it for you. The four parts you'll diagram — state shape, actions, reducer logic, component subscriptions — are the same four parts you'll write in C# tomorrow with Fluxor. The names match; the medium changes.

**The demo just showed you the bug** — two `private List<CartItem>` fields that should have been one shared store. Activity 1 made you locate the bug in code. Now you're sketching the design that fixes it: where the cart lives, what messages it accepts, who reads from it.

You'll know you're done when your diagram has four labelled boxes (Store, Actions, Reducer, Components), arrows showing the unidirectional cycle, and at least three actions with their data payloads. If a partner can read your paper and explain to a classmate how a button click eventually updates the Cart page, you've nailed it.

Work with your partner using paper, a whiteboard, or a shared document. No code -- this is a design exercise.

### Your Task

Using the shopping cart demo as your scenario, draw and label the complete Flux/Redux data flow. Your diagram should include four parts:

### Part 1: State Shape

Define what the centralized store holds. What properties does the cart state need?

Think about:
- What data did the Products page store locally?
- What would the Cart page need to display a full cart view?
- Are there any computed values (totals, counts) that can be derived from the base data?

**Format your answer like this:**
```
CartState {
    property: type    // what it holds
    property: type    // what it holds
}
```

### Part 2: Actions

What actions can users take? Name at least 3. For each action, list what data it carries.

Think about:
- What did the "Add to Cart" button do?
- What other operations would a shopping cart need?
- What data does each operation require?

**Format your answer like this:**
```
ActionName { data it carries }
ActionName { data it carries }
ActionName { }  // no data needed
```

### Part 3: Reducer Logic

For each action you defined, describe what the reducer does to the state. Use pseudocode or plain English.

**Remember:** Reducers return **new state** -- they don't modify the existing state. This is a key rule.

**Format your answer like this:**
```
When ActionName:
    If [condition]:
        return new state with [change]
    Else:
        return new state with [change]
```

### Part 4: Component Subscriptions

Which components need to read from the store? What data does each one use?

Think about:
- Does the Products page need cart data? For what?
- Does the Cart page need cart data? For what?
- Would the NavMenu benefit from cart data? For what?

### Draw the Cycle

Connect all four parts in a diagram showing the unidirectional data flow:

```
Component → dispatches → Action → processed by → Reducer → updates → Store → notifies → Component
```

### Checklist

Before you're done, verify your diagram has:
- [ ] A drawn cycle (arrows showing the flow direction)
- [ ] At least 3 actions, each with their data payload
- [ ] Reducer logic that creates *new* state (not "modify the existing list")
- [ ] Clear component subscriptions showing which component reads what

---

## Activity 3: Group Discussion -- State Audit (10 min)

Senior engineers audit codebases for state-management opportunities the way doctors do triage — quickly, with pattern recognition, looking for recurring symptoms. The skill is *generalization*: taking the specific bug you saw in the shopping cart and recognizing the same shape elsewhere in code you already know.

This is the move from "I see a bug" to "I see a class of bug." Once you can spot it, you'll start seeing state-loss everywhere — every multi-step form where typing on page 2 resets after a back-and-forward, every dashboard that re-fetches when you reopen a tab, every wizard that "forgets" progress on refresh. Today's the morning you learn to recognize the symptom.

**The demo gave you the canonical instance** (shopping cart, two private lists), and the Day 1-2 apps you've already built — nested layouts, route parameters, the registration/returns/survey wizards — contain real candidates. Look for places where data is entered or computed on one page and silently lost when the route changes.

You'll know it worked when your group can name 1-2 scenarios from Days 1-2 in one sentence each, and the class hears at least three different examples across groups. If you find yourself thinking "those apps don't have state issues" — re-read the demo. The same shape is hiding in plain sight.

**Activity 3 expands the pair into a group of 3-4** — find one more pair you have not worked with yet and combine.

### Your Task

Review the apps from Days 1 and 2 (nested routes, route parameters, NavigationManager, checkout wizard). Identify **1-2 scenarios** where centralized state management would improve the application.

For each scenario, answer:

1. **What data is currently lost or duplicated?**
2. **Which components need access to it?**
3. **What actions would you define?**

Be ready to share one scenario in one sentence with the class.

### Hints

Think about situations where:
- Data entered on one page is lost when navigating to another
- Multiple components need the same data but have no way to share it
- The user's context or progress resets unexpectedly

---

## Key Concepts from Today

| Concept | Definition |
|---------|-----------|
| **State loss on navigation** | Component-level state (private fields) is destroyed when you navigate away |
| **Prop drilling** | Passing data through intermediate components that don't use it |
| **Cross-component sync** | Multiple components needing the same data, kept in sync |
| **Action** | A data object describing *what happened* (like a C# event args class) |
| **Reducer** | A pure function that takes current state + action and returns *new* state |
| **Store** | A centralized container for application state that survives navigation |
| **Unidirectional flow** | Data flows one way: Component → Action → Reducer → Store → Component |
