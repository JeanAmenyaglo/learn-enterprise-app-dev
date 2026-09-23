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

    // ============================================================
    // PRACTICE 1: Build the persistence + hydration pattern for "Recently Viewed".
    // In the demo you watched the instructor build this same pattern for the CART.
    // Apply it here, to a different feature, from scratch.
    //
    // TODO 1 -- Save on change (WORKED LAUNCH -- the instructor writes this one on screen):
    //   Add an [EffectMethod] for ViewProductAction that saves the list to localStorage.
    //   Name the method Handle<ActionName> like the demo's HandleAddToCartAction -- here that
    //   is HandleViewProductAction (full action name after the Handle prefix).
    //   Effect Timing Rule: the effect runs AFTER the reducer, so recentState.Value.Items is
    //   already the updated, de-duplicated, capped-at-5 list -- save that, not action.Item.
    //
    // TODO 2 -- Rehydrate on startup (INDEPENDENT -- you continue from here on your own):
    //   Add an [EffectMethod] for HydrateRecentlyViewedRequestAction that:
    //     - loads the saved list:
    //         var items = await localStorage.GetItemAsync<List<RecentlyViewedItem>>(RecentlyViewedStorageKey);
    //     - if items is not null and items.Count > 0, dispatch new HydrateRecentlyViewedAction(items)
    //     - wraps the load in try/catch; in catch (corrupt data) call
    //         await localStorage.RemoveItemAsync(RecentlyViewedStorageKey);
    //       and let the list start empty
    // ============================================================
}
