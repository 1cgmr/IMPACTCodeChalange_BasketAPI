using BasketAPI.Models;
using CodeChallengeApiClientNamespace;
namespace BasketAPI.Services
{
    public interface ICodeChallengeAPIService
    {
        Task<OrderResponse> CreateOrderAsync(Guid basketId);
        Task<ICollection<ProductResponse>> GetAllProductsAsync();
        Task<ICollection<ProductResponse>> GetPaginatedProductsAsync(PaginationParameters paginationParameters);
        Task<ICollection<ProductResponse>> GetTopRankedProductsAsync();
        Task<OrderResponse> GetOrderAsync(string orderId);
        Task<LoginResponse> LoginAsync();

    }
}