using Fluxor;

namespace Day08Demo.Features.Cart.Store;

public static class CartReducers
{
    [ReducerMethod]
    public static CartState ReduceAddToCartAction(CartState state, AddToCartAction action)
    {
        var existingItem = state.Items.FirstOrDefault(i => i.Name == action.Item.Name);
        if (existingItem is not null)
        {
            var updatedItems = state.Items
                .Select(i => i.Name == action.Item.Name
                    ? i with { Quantity = i.Quantity + action.Item.Quantity }
                    : i)
                .ToList();
            return state with { Items = updatedItems };
        }

        return state with { Items = [.. state.Items, action.Item] };
    }

    [ReducerMethod]
    public static CartState ReduceRemoveFromCartAction(CartState state, RemoveFromCartAction action)
    {
        var updatedItems = state.Items
            .Where(i => i.Name != action.ItemName)
            .ToList();
        return state with { Items = updatedItems };
    }

    // typeof(...) form: use when the reducer doesn't need the action's data (no action parameter).
    // When you need the action's data (like action.Items), use the standard form above.
    [ReducerMethod(typeof(ClearCartAction))]
    public static CartState ReduceClearCartAction(CartState state)
    {
        return state with { Items = [] };
    }

    // ============================================================
    // DEMONSTRATION Step 4 (CODE LIVE): Add the hydration reducer
    //
    // TODO: Add a [ReducerMethod] for HydrateCartAction that returns
    //       state with { Items = action.Items }
    // ============================================================
}
