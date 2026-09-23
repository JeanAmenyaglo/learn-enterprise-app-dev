using BYSResults;

namespace Day06Practice.Services;

public class ProductService : IProductService
{
    private readonly List<Product> _products =
    [
        new Product(1, "Wireless Mouse", 29.99m),
        new Product(2, "USB-C Hub", 49.99m),
        new Product(3, "Mechanical Keyboard", 89.99m)
    ];

    public async Task<Result<Product>> GetByIdAsync(int id)
    {
        await Task.Delay(100);

        var result = new Result<Product>();

        if (id <= 0)
        {
            result.AddError(new Error("Validation", "Product ID must be greater than zero"));
            return result;
        }

        var product = _products.FirstOrDefault(p => p.Id == id);

        if (product is null)
        {
            result.AddError(new Error("Not Found", $"Product with ID {id} not found"));
            return result;
        }

        return result.WithValue(product);
    }

    public async Task<Result<Product>> AddAsync(Product product)
    {
        await Task.Delay(100);

        var result = new Result<Product>();

        if (string.IsNullOrWhiteSpace(product.Name))
            result.AddError(new Error("Validation", "Product name is required"));

        if (product.Price <= 0)
            result.AddError(new Error("Validation", "Price must be greater than zero"));

        if (result.IsFailure)
            return result;

        var newProduct = product with { Id = _products.Max(p => p.Id) + 1 };
        _products.Add(newProduct);

        return result.WithValue(newProduct);
    }

    public async Task<Result<Product>> UpdateAsync(Product product)
    {
        await Task.Delay(100);

        var result = new Result<Product>();

        if (product.Id <= 0)
            result.AddError(new Error("Validation", "Product ID must be greater than zero"));

        if (string.IsNullOrWhiteSpace(product.Name))
            result.AddError(new Error("Validation", "Product name is required"));

        if (result.IsFailure)
            return result;

        var existing = _products.FirstOrDefault(p => p.Id == product.Id);

        if (existing is null)
        {
            result.AddError(new Error("Not Found", $"Product with ID {product.Id} not found"));
            return result;
        }

        _products.Remove(existing);
        _products.Add(product);

        return result.WithValue(product);
    }
}