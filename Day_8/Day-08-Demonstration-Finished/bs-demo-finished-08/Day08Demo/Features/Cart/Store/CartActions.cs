using Day08Demo.Models;

namespace Day08Demo.Features.Cart.Store;

// Add / Remove / Clear are the Day-7 cart store, carried over unchanged.
public record AddToCartAction(CartItem Item);
public record RemoveFromCartAction(string ItemName);
public record ClearCartAction;

// Hydration is today's new surface — and it's deliberately TWO actions, not one.
// HydrateCartRequestAction is the *request* ("please load the saved cart"); it carries no
// payload because the data doesn't exist yet — reading localStorage is async I/O, which a
// reducer (pure, synchronous) is not allowed to do. The effect does that read, then dispatches
// HydrateCartAction carrying the loaded items, and the reducer copies them into state. Same
// request → effect → result-action split as the Day-5/6 async-load triad, applied to a local read.
public record HydrateCartRequestAction;
public record HydrateCartAction(IReadOnlyList<CartItem> Items);
