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

    [ReducerMethod(typeof(ClearCartAction))]
    public static CartState ReduceClearCartAction(CartState state)
    {
        return state with { Items = [] };
    }

    // DEMONSTRATION Step 4: Hydration reducer.
    // This is the pure, synchronous end of hydration — the effect already did the async
    // localStorage read and handed the loaded items in on the action. We REPLACE Items
    // wholesale (not append): the saved cart IS the cart at startup, so merging it with the
    // current (empty) Items would be wrong if we ever hydrated over a non-empty cart.
    [ReducerMethod]
    public static CartState ReduceHydrateCartAction(CartState state, HydrateCartAction action)
    {
        return state with { Items = action.Items };
    }
}
