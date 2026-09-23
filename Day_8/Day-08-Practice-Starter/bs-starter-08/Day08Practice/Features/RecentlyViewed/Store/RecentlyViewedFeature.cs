using Fluxor;

namespace Day08Practice.Features.RecentlyViewed.Store;

public class RecentlyViewedFeature : Feature<RecentlyViewedState>
{
    public override string GetName() => "RecentlyViewed";

    protected override RecentlyViewedState GetInitialState() => new();
}
