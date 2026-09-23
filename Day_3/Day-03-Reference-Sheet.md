# Day 3 Reference Sheet -- State Challenges and Flux/Redux Pattern Intro

**Course:** SDEV 2351 | **Module:** 1 | **CO:** CO2
**Estimated Reading Time:** ~5 minutes

---

<img src="../assets/day-03-state-challenges.svg" alt="Concept map comparing state management without and with a centralized store. Left side shows three problems from scattered component state: (1) state loss on navigation when a ProductsPage cart field resets to empty after navigating to CartPage, (2) prop drilling threading cart data through Dashboard and NavBar to reach CartBadge, and (3) cross-component sync drift between HeaderCartBadge and CartSummary holding divergent cartCount values. Right side shows a Fluxor Store as the single source of truth holding an immutable CartState record, with ProductsPage, CartPage, and HeaderCartBadge subscribing to it via IState&lt;CartState&gt;. Footer rule: components never write to state directly — they dispatch actions and a reducer produces the next state." />

*(Diagram shorthand: `[A, B, C]` represents a `List<CartItem>` containing three items; full type omitted for diagram clarity.)*

**How to read this diagram:** Work left to right. Each amber card on the left is a problem you will hit without a store — state loss on navigation, prop drilling, and cross-component sync. The navy box on the right is the Fluxor Store: one immutable `CartState` that every component subscribes to via `IState<CartState>`. The three amber arrows across the middle say *"solved by store"* — one mechanism resolves all three pain points. Keep the key rule in mind: **components never write to the store directly** — they dispatch actions, and a reducer produces the next state.

---

## Key Concepts

- **State loss on navigation** -- component-level fields (`private List<T>`) are destroyed when the user leaves the page. When the user returns, the component is re-created with empty default values.
- **Centralized store** -- a single location outside any component that holds shared state. This store survives navigation (it is not destroyed when the user changes pages) and any component can access it.
- **Unidirectional data flow** -- data flows in one direction only: Component dispatches Action → Reducer returns new State → Store updates → Component re-renders
- **Pure reducers** -- a reducer takes the current state and an action, then returns *new* state. A reducer never modifies existing state and has no side effects.

## Terminology Mapping

| C# Concept You Know | Flux/Redux Equivalent | Role |
|---------------------|----------------------|------|
| Raising an event | Dispatching an action | Signals that something happened |
| Event args class | Action class | Carries data about what happened |
| Event handler | Reducer | Processes the action and produces new state |
| Returning a new object | Immutable state update | Ensures previous state is never modified |
| Singleton service | Store | Holds the single shared state (one authoritative copy of the data — one shared instance per user session, not one per component). |
| Event subscription | Component subscription | Components re-render when state changes |

> **Modern C# you'll meet here.** Today's demo code uses two constructs you may not have seen in first-year: **`record` types** (one-line data types like `CartItem`) and **`with` expressions** -- `existing with { Quantity = existing.Quantity + 1 }`, which makes *a copy with one field changed* and leaves the original untouched (the "return new state, never mutate" rule, in code). Both are explained in the course reference <strong><a href="/d2l/le/lessons/184456/units/6193672" target="_blank" rel="noopener">Modern C# You'll Meet Here</a></strong> -- Section 2.1 (positional records), Section 2.2 (`with` expressions), and Section 3.3 (init-only properties).

## Data Flow

```
    ┌─────────────┐     dispatch      ┌──────────┐
    │  Component   │ ───────────────► │  Action   │
    │ (subscribes) │                  │ (data)    │
    └──────▲───────┘                  └─────┬─────┘
           │                                │
       notifies                         processed by
           │                                │
    ┌──────┴───────┐                  ┌─────▼─────┐
    │    Store      │ ◄────────────── │  Reducer   │
    │ (state)       │   returns new   │ (pure fn)  │
    └──────────────┘     state        └───────────┘
```

**Data flows in one direction only.** Components never write to the Store directly.

## Flux Mapping Template (Activity 2)

Use this template during the paper Flux mapping exercise. Fill in each box for your shopping cart scenario.

```
┌─────────────────────────────────────────────┐
│  STATE SHAPE                                │
│                                             │
│  Type name: ____________________________    │
│  Property 1: _____________  Type: ________  │
│  Property 2: _____________  Type: ________  │
│  Property 3: _____________  Type: ________  │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│  ACTIONS (what happened)                    │
│                                             │
│  1. ________________  Data: ______________  │
│  2. ________________  Data: ______________  │
│  3. ________________  Data: ______________  │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│  REDUCERS (how state changes)               │
│                                             │
│  Action 1 → New state: ___________________  │
│  Action 2 → New state: ___________________  │
│  Action 3 → New state: ___________________  │
│  (Remember: return NEW state, don't mutate) │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│  SUBSCRIPTIONS (who reads what)             │
│                                             │
│  Component: _____________ reads: _________  │
│  Component: _____________ reads: _________  │
│  Component: _____________ reads: _________  │
└─────────────────────────────────────────────┘
```

> **Looking ahead to Day 4 — your paper diagram becomes code.** Everything you just mapped here on paper — the state shape, the action names, the reducer logic — becomes real C# tomorrow. Keep <strong><a href="Fluxor-Store-Anatomy.html?isCourseFile=true" target="_blank" rel="noopener">Anatomy of a Fluxor Store</a></strong> open as you build: it walks these same parts (action → state → reducer → feature) as working code, using the Counter you'll build and a product catalog. Today's drawing is the blueprint; that document is the build.

> **Two worked design examples.** Today you practised the whiteboard step — turning requirements into a store. For two full worked versions of that exact step, see <strong><a href="Whiteboarding-A-Turnstile-Counter.html?isCourseFile=true" target="_blank" rel="noopener">Whiteboarding a Turnstile Counter</a></strong> (the simplest store in the course — a lobby with IN/OUT turnstiles, start to finish) and <strong><a href="Whiteboarding-A-Ride-Share-Trip.html?isCourseFile=true" target="_blank" rel="noopener">Whiteboarding a Ride-Share Trip</a></strong> (a larger async store). Each one starts from "what must the app remember?" and "what can happen?", then asks "does this action have to wait?" — the same questions you used here.

## Common Misconceptions

| Misconception | Why It Is Wrong | What Is Actually True |
|--------------|----------------|---------------------|
| "The store is just a global variable" | A global variable has no rules about how it is modified | The store enforces modification only through actions and pure reducers -- this makes changes predictable and testable |
| "Everything belongs in the store" | Putting all state in the store adds unnecessary complexity | Only shared state or state that must survive navigation goes in the store; local UI state (for example, whether a dropdown is open) stays in the component |
| "Reducers can call APIs" | Reducers must be pure -- no side effects | Async work (API calls, timers) is handled by effects (covered on Day 5) |

## Quick Check

1. **What is the only way a component can change the store?**
   <details><summary>Answer</summary>By dispatching an action. Components never modify the store directly.</details>

2. **What does a reducer do?**
   <details><summary>Answer</summary>Takes the current state and an action, returns a new state. A reducer is a pure function -- the same inputs always produce the same output, with no side effects.</details>

3. **Why is the data flow called "unidirectional"?**
   <details><summary>Answer</summary>Data flows in one direction around the cycle: Component → Action → Reducer → Store → Component. There are no alternate paths or reverse directions.</details>

## Watch the state-loss problem in action

> **When to use this video:** Watch if you missed class, want a recap after homework, or need to re-orient before Activity 1 in tomorrow's lesson. The 16-second walkthrough mirrors the in-class demo — captions are auto-generated, transcript linked below for ESL/accessibility.

**Why does my cart disappear when I change pages?** A 16-second screen recording of the Day 3 demo — the Products page adds three items, you navigate to Cart, and the cart is empty. Proof that component-local state does not survive navigation.

<video controls preload="metadata" width="720" style="max-width:100%;height:auto;border:1px solid #DEE2E6;">
  <source src="../demo-videos/day-03-state-loss.mp4" type="video/mp4" />
  <track kind="captions" src="../demo-videos/day-03-state-loss.vtt" srclang="en" label="English" default />
  Your browser does not support embedded video. <a href="../demo-videos/day-03-state-loss.mp4">Download the MP4 (16 s, silent, captioned)</a>.
</video>

- Video file: [`day-03-state-loss.mp4`](../demo-videos/day-03-state-loss.mp4) (16 s, silent, 1280×720, H.264)
- Caption track: [`day-03-state-loss.vtt`](../demo-videos/day-03-state-loss.vtt) (WebVTT sidecar — WCAG 1.2.2)
- Full text transcript (screen-reader friendly): [`day-03-state-loss-transcript.md`](../demo-videos/day-03-state-loss-transcript.md) (WCAG 1.2.1)

The video has **no audio**. All narration is in the on-screen captions and the transcript. Nothing in Day 3 assessments requires watching the video — the transcript is a complete alternative.

---

*See also: [MS Learn: Blazor State Management](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/) | [Fluxor Docs](https://github.com/mrpmorris/Fluxor/blob/master/Docs/README.md) | Fluxor Quick Reference Guide (pre-class reading for Day 3) | Day 3 analysis and demonstration projects (download from Brightspace)*
