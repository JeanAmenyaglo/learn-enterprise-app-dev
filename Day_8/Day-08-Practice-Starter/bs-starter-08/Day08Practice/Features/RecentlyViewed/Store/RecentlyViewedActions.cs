using Day08Practice.Models;

namespace Day08Practice.Features.RecentlyViewed.Store;

// PRE-BUILT (domain action): the Shop page dispatches this when a product is viewed.
public record ViewProductAction(RecentlyViewedItem Item);

public record HydrateRecentlyViewedRequestAction;
public record HydrateRecentlyViewedAction(IReadOnlyList<RecentlyViewedItem> Items);