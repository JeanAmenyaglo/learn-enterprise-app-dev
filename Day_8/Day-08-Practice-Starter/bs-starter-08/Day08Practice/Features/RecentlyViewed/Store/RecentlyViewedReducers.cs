using Fluxor;

namespace Day08Practice.Features.RecentlyViewed.Store;

public static class RecentlyViewedReducers
{
    // PRE-BUILT (domain reducer): prepend the just-viewed product, drop any earlier
    // duplicate of it, and keep only the 5 most-recent. This is a pure function with no
    // I/O -- your persistence work lives in the effects, not here.
    [ReducerMethod]
    public static RecentlyViewedState ReduceViewProductAction(RecentlyViewedState state, ViewProductAction action)
    {
        var withoutDuplicate = state.Items.Where(i => i.ProductId != action.Item.ProductId);
        var updated = new[] { action.Item }.Concat(withoutDuplicate).Take(5).ToList();
        return state with { Items = updated };
    }

    // TODO (Practice 1): Add a [ReducerMethod] for HydrateRecentlyViewedAction
    // that returns state with { Items = action.Items }
}
