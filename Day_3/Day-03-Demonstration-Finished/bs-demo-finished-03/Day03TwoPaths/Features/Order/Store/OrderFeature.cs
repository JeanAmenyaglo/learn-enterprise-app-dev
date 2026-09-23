namespace Day03TwoPaths.Features.Order.Store;

using Fluxor;

/// <summary>
/// Registers the order slice with the Fluxor store and supplies its starting value.
/// </summary>
/// <remarks>
/// Fluxor finds this class by assembly scanning (see <c>AddFluxor</c> in <c>Program.cs</c>),
/// so there is nothing to wire up by hand. One feature class per slice is the convention.
/// </remarks>
public class OrderFeature : Feature<OrderState>
{
    /// <summary>
    /// Names this slice in the store and in Redux DevTools.
    /// </summary>
    /// <returns>The slice name.</returns>
    public override string GetName() => "Order";

    /// <summary>
    /// Supplies the state that exists before any action has been dispatched.
    /// </summary>
    /// <returns>A fresh order with the record's default values.</returns>
    /// <remarks>
    /// This is also the value the store falls back to after a full page reload, because the
    /// store lives in memory for the current connection and a reload starts a new one.
    /// </remarks>
    protected override OrderState GetInitialState()
    {
        return new OrderState();
    }
}
