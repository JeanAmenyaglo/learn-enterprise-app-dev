using Fluxor;
using Day06Practice.Services;

namespace Day06Practice.Features.Product.Store;

// Practice 2 actions (used by ProductEffects)
public record LoadProductAction(int ProductId);
public record LoadProductSuccessAction(Services.Product Product);
public record LoadProductFailureAction(string ErrorMessage);

/// <summary>
/// Practice 2: After refactoring ProductService to return Result<T>,
/// update this effect to use result.IsSuccess instead of try/catch.
///
/// TODO: Replace the try/catch block with:
///   var result = await productService.GetByIdAsync(action.ProductId);
///   if (result.IsSuccess)
///       dispatcher.Dispatch(new LoadProductSuccessAction(result.Value!));   // ! -- the IsSuccess guard guarantees non-null
///   else
///       dispatcher.Dispatch(new LoadProductFailureAction(...));
/// </summary>
public class ProductEffects(IProductService productService)
{
   [EffectMethod]
public async Task HandleLoadProductAction(
    LoadProductAction action, IDispatcher dispatcher)
{
    var result = await productService.GetByIdAsync(action.ProductId);

    if (result.IsSuccess)
        dispatcher.Dispatch(new LoadProductSuccessAction(result.Value!));

    if (result.IsFailure)
        dispatcher.Dispatch(new LoadProductFailureAction(string.Join("; ", result.Errors.Select(e => e.Message))));
}
}
