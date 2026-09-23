using Day08Practice.Models;

namespace Day08Practice.Features.RecentlyViewed.Store;

// PRE-BUILT (domain action): the Shop page dispatches this when a product is viewed.
public record ViewProductAction(RecentlyViewedItem Item);

// ============================================================
// PRACTICE 1: Hydration actions (you write these)
//
// You saw the cart use a trigger action + a result action in the demo.
// Add the same two actions for Recently Viewed:
//   TODO: HydrateRecentlyViewedRequestAction  -- the trigger; carries no data
//         (declare it parameterless: `public record HydrateRecentlyViewedRequestAction;`
//          -- no parentheses and no body is correct, not a typo)
//   TODO: HydrateRecentlyViewedAction(IReadOnlyList<RecentlyViewedItem> Items)  -- carries the loaded items
// ============================================================
