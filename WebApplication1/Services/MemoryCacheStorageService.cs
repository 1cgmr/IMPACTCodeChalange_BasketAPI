using BasketAPI.Exceptions;
using BasketAPI.Models;
using CodeChallengeApiClientNamespace;
using Microsoft.Extensions.Caching.Memory;
using Nelibur.ObjectMapper;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text.Json;

namespace BasketAPI.Services
{
    public class MemoryCacheStorageService : IStorageService
    {
        private readonly IMemoryCache _memoryCache;
        public MemoryCacheStorageService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache; 
        }


        public Basket StoreBasket(Basket basket)
        {
            return _memoryCache.Set(basket.basketId, basket, TimeSpan.FromDays(60));
        }

        public Basket GetBasket(Guid id)
        {
            Basket basket;
            if (!_memoryCache.TryGetValue(id, out basket))
                throw new BasketNotFoundException(id);            
            
            return basket;

        }

        public bool DeleteBasket(Guid basketId)
        {
            if (!_memoryCache.TryGetValue(basketId, out _))
                throw new BasketNotFoundException(basketId);
            _memoryCache.Remove(basketId);
            return true;
        }

        public string GetToken()
        {
            string token = string.Empty;
            _memoryCache.TryGetValue("Token", out token);
            return token;
        }

        public ICollection<ProductResponse> GetProductList()
        {
            ICollection<ProductResponse> products;
            _memoryCache.TryGetValue("Products", out products);
            return products;
        }

        public bool StoreProductList(ICollection<ProductResponse> products)
        {
            _memoryCache.Set("Products", products, TimeSpan.FromMinutes(60));
            return true;
        }

        public bool StoreToken(string token)
        {
            var result = _memoryCache.Set("Token", token, TimeSpan.FromMinutes(60));
            return result == token;
        }

        public bool DeleteProductList()
        {
            throw new NotImplementedException();
        }
    }
}
