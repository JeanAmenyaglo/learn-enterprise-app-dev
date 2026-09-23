# Demo Walkthrough -- Day 3: State Challenges -- Shopping Cart State Loss

**What this is:** A step-by-step companion for the instructor demo. Follow along as the instructor walks through a pre-built shopping cart and types the two lines of code that complete it.

**What you'll see:** A pre-scaffolded Products page and Cart page — each with its own private cart data. The instructor types two missing lines (one in each file), then navigates between the pages. The cart data disappears when navigating — demonstrating why component-level state doesn't work for data that needs to survive navigation.

> **How to use this walkthrough:** Open the starter project on your machine and keep it visible — but **don't type along during the demo**. Watch the instructor walk through each step; you'll get hands-on time in Activity 1 right after. If you're scanning ahead, the bullets in each step are what the instructor will narrate aloud.

---

## Step 1: The Starting Point

The instructor opens a starter Blazor project. **Most of the code is already there:**

- A `CartItem` model with four properties (ProductId, Name, Price, Quantity)
- A `Products` page with three product cards, an "Add to Cart" button on each, and an alert showing the cart count
- A `Cart` page with an empty-cart alert and a "Why did this happen?" explanation card
- A sidebar with Home, Products, and Shopping Cart links

When the instructor tries to build, **it fails** with compile errors shown by the editor — Visual Studio underlines the missing `cartItems` references in red. Six errors in `Products.razor` and two in `Cart.razor` — all asking for the same field. That's intentional. Those two missing field declarations are what the demo is about.

---

## Step 2: CartItem Model

The instructor opens `Models/CartItem.cs` and reads it aloud — it's already in the starter:

```csharp
namespace Day03Demo.Models;

public record CartItem(int ProductId, string Name, decimal Price, int Quantity);
```

A positional `record` with four values — the whole type is one line. Records are immutable: you make a changed copy with `with` instead of editing one in place. The interesting question is *where this data lives* once components start using it.

---

## Step 3: Products Page — Tour

The instructor opens `Components/Pages/Products.razor` and walks through the markup: three product cards in a Bootstrap row, an `AddToCart` method in the `@code` block, and an alert at the bottom showing the cart count.

Then the instructor scrolls to a comment marking the missing field:

```csharp
@code {
    // LIVE-CODE (instructor types this line during demo Step 4 — walkthrough numbering; script Step 2):
    //   private List<CartItem> cartItems = [];

    private List<Product> availableProducts = [ ... ];   // Product is a record declared in this @code block
    private void AddToCart(...) { ... }
}
```

The `AddToCart` method references `cartItems`. The markup above references `cartItems`. But there is no field declared.

---

## Step 4: Live-Type the Products Field

The instructor types one line:

```csharp
private List<CartItem> cartItems = [];
```

**Key thing to notice:** This is a **private field** inside the Products component. It belongs to *this component instance only*. No other component can access it.

As soon as the instructor saves the file, the red squigglies under `cartItems` in the markup and `AddToCart` disappear and the project builds. The instructor then runs the app, navigates to `/products`, and clicks "Add to Cart" two or three times. The alert at the bottom updates: "3 item(s)" with the details listed.

---

## Step 5: Cart Page — Tour

The instructor opens `Components/Pages/Cart.razor` and walks through the markup: an empty-state alert, a "Why did this happen?" card, and an else-branch table for populated carts.

Then scrolls to a second missing-field comment:

```csharp
@code {
    // LIVE-CODE (instructor types this line during demo Step 6 — walkthrough numbering; script Step 4):
    //   private List<CartItem> cartItems = [];
}
```

Same situation. Markup references `cartItems`. No field. **Notice this is a completely separate file from Products.razor.**

---

## Step 6: Live-Type the Cart Field

The instructor types the same line, in this different file:

```csharp
private List<CartItem> cartItems = [];
```

Same name. Same type. Same initializer. **But this is a different component**, so this is a completely separate, empty list. It does not share the Products page's list. There is no connection between the two.

As before, the red squigglies under `cartItems` in `Cart.razor` disappear on save and the project builds cleanly — the same visual payoff you saw in Step 4, now in the second component.

---

## Step 7: The Reveal -- State Loss in Action

This is the critical moment. Watch carefully:

1. **Products page:** The instructor adds 2-3 items. The alert shows "3 item(s)." The data exists in the Products component's private field.

2. **Navigate to Shopping Cart:** Click the sidebar link. The Cart page loads — **the cart is empty.**

3. **Navigate back to Products:** Click the sidebar link. The Products page loads — **also empty.** The items are gone.

### Why did this happen?

| Step | What happened |
|------|--------------|
| Items added on Products page | Stored in Products component's `private List<CartItem>` |
| Navigated to Cart page | Products component was **destroyed**. Its `cartItems` list was garbage collected. |
| Cart page loaded | A new Cart component was created with its **own empty** `cartItems` list. |
| Navigated back to Products | A **new** Products component was created with a fresh, empty `cartItems` list. |

> **If a squiggly persists after save:** the Razor language server can lag a second or two. Save the file once more, or press Ctrl+Shift+B to force a build. The squiggly will clear when the compiler catches up.

**The core problem:** Each Blazor component stores data in private fields. When you navigate away, the component is destroyed and those fields are gone. When you navigate back, a *new* component instance is created with default values.

**The two `cartItems` lines were identical, but they declared two independent storage locations.** That duplication is what makes the data un-shareable across pages.

---

## Step 8: The Solution Preview -- Flux/Redux

The Flux/Redux pattern solves this by moving state **outside** of components into a **centralized store**:

```
Component dispatches Action → Reducer processes it → Store updates → Components re-render
```

- The **store** holds cart data in a central location that survives navigation
- **Actions** describe what happened ("add item 42 to cart")
- **Reducers** process actions and return new state
- **Components** subscribe to the store and re-render when state changes

Both the Products page and the Cart page would subscribe to the same store, seeing the same cart data. Navigation doesn't affect the store — it lives independently of any component.

**Tomorrow (Day 4):** You'll implement this pattern with the Fluxor library in C#.

---

## Key Takeaways

1. **Component-level state (`private` fields in `@code`) does not survive navigation.** When a component is destroyed, its data is gone.

2. **Each component instance has independent state.** The Products page's `cartItems` and the Cart page's `cartItems` are two completely separate lists, even though the declarations are byte-for-byte identical.

3. **The Flux/Redux pattern centralizes state** in a store that lives outside any component, making it accessible to any component that subscribes to it.

4. **Not all state belongs in a store.** Local UI state (is a dropdown open? which tab is selected?) stays in the component. Only state that must survive navigation or be shared across components goes in the store.
