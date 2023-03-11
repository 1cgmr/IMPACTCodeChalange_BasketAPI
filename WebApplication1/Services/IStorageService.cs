using BasketAPI.Models;
using CodeChallengeApiClientNamespace;

namespace BasketAPI.Services
{
    public interface IStorageService
    {
        bool StoreProductList(ICollection<ProductResponse> products);
        ICollection<ProductResponse> GetProductList();
        bool DeleteProductList();
        Basket StoreBasket(Basket basket);
        Basket GetBasket(Guid id);
        bool DeleteBasket(Guid basketId);
        string GetToken();
        bool StoreToken(string token);

    }
}
