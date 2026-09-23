using Day08Demo.Models;

namespace Day08Demo.Features.Cart.Store;

public record AddToCartAction(CartItem Item);
public record RemoveFromCartAction(string ItemName);
public record ClearCartAction;

// ============================================================
// DEMONSTRATION Step 3 (CODE LIVE): Add the hydration actions
//
// TODO: Add a HydrateCartRequestAction record (triggers the load)
// TODO: Add a HydrateCartAction record carrying IReadOnlyList<CartItem> Items
//       (carries the loaded items into the reducer)
// ============================================================
