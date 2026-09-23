using Blazored.LocalStorage;
using Day08Demo.Models;
using Fluxor;

namespace Day08Demo.Features.Cart.Store;

// Day-5/6 effects LOADED data from a service INTO state. These effects run the opposite
// direction: they react to the same Add/Remove/Clear actions the reducers handle and persist
// state OUT to localStorage. One dispatched action drives BOTH handlers independently — the
// reducer updates the in-memory cart, and the effect mirrors it to storage. The reducer never
// waits on the effect; they are two separate subscribers to the same action.
//
// Note we read cartState.Value.Items, NOT action — by the time an effect runs, the reducer has
// already produced the new state, so the injected IState<CartState> holds the cart we want to save.
public class CartPersistenceEffects(
    ILocalStorageService localStorage,
    IState<CartState> cartState)
{
    private const string CartStorageKey = "cart";

    // DEMONSTRATION Step 2: Save cart on every change.
    // Add and Remove both write the whole item list (localStorage stores a value, not a diff);
    // Clear removes the key entirely rather than writing an empty list, so a cleared cart leaves
    // no stale entry behind for hydration to find.

    [EffectMethod]
    public async Task HandleAddToCartAction(AddToCartAction action, IDispatcher dispatcher)
    {
        await localStorage.SetItemAsync(CartStorageKey, cartState.Value.Items);
    }

    [EffectMethod]
    public async Task HandleRemoveFromCartAction(RemoveFromCartAction action, IDispatcher dispatcher)
    {
        await localStorage.SetItemAsync(CartStorageKey, cartState.Value.Items);
    }

    [EffectMethod]
    public async Task HandleClearCartAction(ClearCartAction action, IDispatcher dispatcher)
    {
        await localStorage.RemoveItemAsync(CartStorageKey);
    }

    // DEMONSTRATION Step 5: Rehydrate cart from localStorage on startup
    // "Rehydrate" = restore the saved cart state from localStorage on startup.
    // An empty store is "dry"; loading the saved items back in "hydrates" it.

    [EffectMethod]
    public async Task HandleHydrateCartRequestAction(HydrateCartRequestAction action, IDispatcher dispatcher)
    {
        try
        {
            var items = await localStorage.GetItemAsync<List<CartItem>>(CartStorageKey);
            if (items is not null && items.Count > 0)
            {
                dispatcher.Dispatch(new HydrateCartAction(items));
            }
        }
        catch
        {
            // Blazored.LocalStorage throws JsonException on corrupt or mismatched JSON.
            // Bare catch is intentional here: any exception reading localStorage means the
            // data is unrecoverable -- the correct response in all failure modes is to clear
            // the key and start fresh.
            // Do not use bare catch in effects that have specific recoverable failure modes;
            // use the specific exception type there (e.g., catch (JsonException)).
            await localStorage.RemoveItemAsync(CartStorageKey);
        }
    }
}
