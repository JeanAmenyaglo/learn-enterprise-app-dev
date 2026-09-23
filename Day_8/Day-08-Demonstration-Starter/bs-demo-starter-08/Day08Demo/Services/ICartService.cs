using BYSResults;
using Day08Demo.Models;

namespace Day08Demo.Services;

public interface ICartService
{
    Task<Result<List<CartItem>>> GetCartItemsAsync();
}
