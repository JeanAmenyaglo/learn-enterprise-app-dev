using Day08Demo.Models;

namespace Day08Demo.Features.Cart.Store;

public record CartState
{
    public IReadOnlyList<CartItem> Items { get; init; } = [];
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
}
