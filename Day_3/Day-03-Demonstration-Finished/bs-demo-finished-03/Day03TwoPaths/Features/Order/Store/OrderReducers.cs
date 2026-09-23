namespace Day03TwoPaths.Features.Order.Store;

using Fluxor;

/// <summary>
/// Reducers for the order slice: the pure functions that turn (current state, action) into
/// the next state.
/// </summary>
/// <remarks>
/// Every method here is pure — same inputs, same output, no I/O, no mutation. The
/// <c>state with { ... }</c> expression copies the record and overrides one field, carrying
/// every other field forward untouched.
/// <para>
/// Fluxor routes each dispatched action to the <see cref="ReducerMethodAttribute"/> method
/// whose parameter type matches it. Naming follows the Fluxor convention of
/// <c>Reduce</c> + the full action name, so the method says exactly which action it handles.
/// </para>
/// </remarks>
public static class OrderReducers
{
    /// <summary>Applies <see cref="SetCustomerNameAction"/> to the order.</summary>
    /// <param name="state">The current order state.</param>
    /// <param name="action">The dispatched action carrying the new name.</param>
    /// <returns>A new state with the customer name replaced.</returns>
    [ReducerMethod]
    public static OrderState ReduceSetCustomerNameAction(OrderState state, SetCustomerNameAction action)
    {
        return state with { CustomerName = action.CustomerName };
    }

    /// <summary>Applies <see cref="SetCityAction"/> to the order.</summary>
    /// <param name="state">The current order state.</param>
    /// <param name="action">The dispatched action carrying the new city.</param>
    /// <returns>A new state with the delivery city replaced.</returns>
    [ReducerMethod]
    public static OrderState ReduceSetCityAction(OrderState state, SetCityAction action)
    {
        return state with { City = action.City };
    }

    /// <summary>Applies <see cref="SetQuantityAction"/> to the order.</summary>
    /// <param name="state">The current order state.</param>
    /// <param name="action">The dispatched action carrying the new quantity.</param>
    /// <returns>A new state with the quantity replaced.</returns>
    /// <remarks>
    /// The total is not updated here because it is never stored — it is derived from
    /// <c>Quantity</c> every time it is read, so it cannot fall out of step.
    /// </remarks>
    [ReducerMethod]
    public static OrderState ReduceSetQuantityAction(OrderState state, SetQuantityAction action)
    {
        return state with { Quantity = action.Quantity };
    }

    /// <summary>Applies <see cref="ResetOrderAction"/> by returning a fresh order.</summary>
    /// <param name="state">The current order state, unused — a reset ignores what came before.</param>
    /// <param name="action">The dispatched action.</param>
    /// <returns>A new order with the record's default values.</returns>
    [ReducerMethod]
    public static OrderState ReduceResetOrderAction(OrderState state, ResetOrderAction action)
    {
        return new OrderState();
    }
}
