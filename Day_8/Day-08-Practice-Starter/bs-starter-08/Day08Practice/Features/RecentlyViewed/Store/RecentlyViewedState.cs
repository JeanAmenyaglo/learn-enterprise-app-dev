using Day08Practice.Models;

namespace Day08Practice.Features.RecentlyViewed.Store;

public record RecentlyViewedState
{
    public IReadOnlyList<RecentlyViewedItem> Items { get; init; } = [];
}
