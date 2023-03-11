using BasketAPI.Models;

namespace BasketAPI.Services
{
    public interface IBasketService
    {
        Task<bool> ValidateBasketProducts(BasketRequest basket);
        Task<Basket> CreateBasket(BasketRequest basketRequest, Guid basketId = default);
        Task<Basket> PutBasket(Guid basketId, BasketRequest basketRequest);
        Task<Basket> PatchBasket(Guid basketId, BasketRequest basketRequest);

        bool DeleteBasket(Guid basketId);

        Basket DeleteBasketProducts(Guid basketId, List<int> productIdsToDelete);
    }
}
