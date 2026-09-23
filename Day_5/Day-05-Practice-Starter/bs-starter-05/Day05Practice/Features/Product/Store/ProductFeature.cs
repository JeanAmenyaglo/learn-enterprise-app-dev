using Fluxor;

namespace Day05Practice.Features.Product.Store;

public class ProductFeature : Feature<ProductState>
{
    public override string GetName() => "Product";
    protected override ProductState GetInitialState() => new ProductState();
}