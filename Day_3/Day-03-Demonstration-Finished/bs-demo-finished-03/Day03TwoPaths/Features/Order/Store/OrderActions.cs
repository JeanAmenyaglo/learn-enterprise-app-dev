namespace Day03TwoPaths.Features.Order.Store;

// Actions are immutable record "messages" describing something that should happen. They carry
// data but never logic, and dispatching one is the ONLY way a page can change the store — no
// page in this app writes to state directly.

/// <summary>Requests that the customer name be changed.</summary>
/// <param name="CustomerName">The new customer name.</param>
public record SetCustomerNameAction(string CustomerName);

/// <summary>Requests that the delivery city be changed.</summary>
/// <param name="City">The new delivery city.</param>
public record SetCityAction(string City);

/// <summary>Requests that the ordered quantity be changed.</summary>
/// <param name="Quantity">The new quantity.</param>
/// <remarks>
/// Every step of the Flux wizard dispatches this on each keystroke, which is why the number
/// is already in the store before the user navigates anywhere.
/// </remarks>
public record SetQuantityAction(int Quantity);

/// <summary>Requests that the order be returned to its initial values.</summary>
/// <remarks>
/// Parameterless because it carries no data — the reducer already knows what "empty" means.
/// </remarks>
public record ResetOrderAction;
