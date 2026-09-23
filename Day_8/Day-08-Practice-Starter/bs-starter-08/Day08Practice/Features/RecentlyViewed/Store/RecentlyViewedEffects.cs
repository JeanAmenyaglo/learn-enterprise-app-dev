using Blazored.LocalStorage;
using Day08Practice.Models;
using Fluxor;

namespace Day08Practice.Features.RecentlyViewed.Store;

// PRE-BUILT plumbing: the DI wiring (ILocalStorageService + IState<RecentlyViewedState>)
// and the storage-key constant are provided. Visual Studio will show a "parameter is unread"
// hint on localStorage / recentState until you use them in the effects you write below --
// that hint is expected, and it disappears once your two effects are in place.
public class RecentlyViewedEffects(
    ILocalStorageService localStorage,
    IState<RecentlyViewedState> recentState)
{
    private const string RecentlyViewedStorageKey = "recentlyViewed";

       [EffectMethod]
    public async Task HandleViewProductAction(ViewProductAction action, IDispatcher dispatcher)
    {
        await localStorage.SetItemAsync(RecentlyViewedStorageKey, recentState.Value.Items);
    }

    [EffectMethod]
    public async Task HandleHydrateRecentlyViewedRequestAction(HydrateRecentlyViewedRequestAction action, IDispatcher dispatcher)
    {
        try
        {
            var items = await localStorage.GetItemAsync<List<RecentlyViewedItem>>(RecentlyViewedStorageKey);

            if (items is not null && items.Count > 0)
            {
                dispatcher.Dispatch(new HydrateRecentlyViewedAction(items));
            }
        }
        catch
        {
            await localStorage.RemoveItemAsync(RecentlyViewedStorageKey);
        }
    }
}
