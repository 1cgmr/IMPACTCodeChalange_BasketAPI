using BasketAPI.Exceptions;
using BasketAPI.Models;
using Nelibur.ObjectMapper;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace BasketAPI.Services
{
    public class BasketService : IBasketService
    {
        private readonly IStorageService _storageService;
        private readonly ICodeChallengeAPIService _codeChallengeAPIService;
        public BasketService(IStorageService storageService, ICodeChallengeAPIService codeChallengeAPIService)
        {
            _storageService = storageService;
            _codeChallengeAPIService = codeChallengeAPIService;
        }

        public async Task<Basket> CreateBasket(BasketRequest basketRequest, Guid basketId = default)
        {
            await ValidateBasketProducts(basketRequest);
            var createdBasket = await ConvertRequestBasketToBasketAsync(basketRequest);
            createdBasket.basketId = basketId == (Guid)default ? Guid.NewGuid() : basketId;            
            return _storageService.StoreBasket(createdBasket);
        }

        public async Task<Basket> PatchBasket(Guid basketId, BasketRequest basketRequest)
        {
            Basket storedBasket = _storageService.GetBasket(basketId);
            await ValidateBasketProducts(basketRequest);
            Basket requestBasket = await ConvertRequestBasketToBasketAsync(basketRequest);
            requestBasket.basketId = basketId;
            Basket patchedBasket = await PatchBasketWithBasketAsync(storedBasket, requestBasket);
            return _storageService.StoreBasket(patchedBasket);
        }

        public async Task<Basket> PutBasket(Guid basketId, BasketRequest basketRequest)
        {
            _storageService.DeleteBasket(basketId);
            return await CreateBasket(basketRequest, basketId);
        }

        public async Task<bool> ValidateBasketProducts(BasketRequest basket)
        {
            if(basket.basketLines is null || basket.basketLines.Count == 0) { return true; }
            var basketLinesIds = basket.basketLines.Select(x => x.productId).ToList();
            var products = await _codeChallengeAPIService.GetAllProductsAsync();
            var productIds = products.Select(x => x.Id).ToList();
            var isProductListValid = basketLinesIds.All(id => productIds.Contains(id));
            if (!isProductListValid)
                throw new InvalidProductListException();
            return true;
        }

        public bool DeleteBasket(Guid basketId)
        {
            return _storageService.DeleteBasket(basketId);
        }

        public Basket DeleteBasketProducts(Guid basketId, List<int> productIdsToDelete)
        {
            var storedBasket = _storageService.GetBasket(basketId);
            //Delete products from basket list
            storedBasket.basketLines = storedBasket.basketLines.Where(p => !productIdsToDelete.Contains(p.productId)).ToList();
            //Recalculate basket total price
            storedBasket.totalAmount = storedBasket.basketLines.Sum(p => p.totalPrice);
            return _storageService.StoreBasket(storedBasket);
        }

        //----------------------------

        //TODO: Make a more general purpose method using perhaps reflection. 
        private T UpdateObjectWithDifferences<T>(T originalObject, T diffObject)
        {
            string jsonStringOriginalObject = JsonSerializer.Serialize(originalObject);
            string jsonStringDiffObject = JsonSerializer.Serialize(diffObject);

            JObject o1 = JObject.Parse(jsonStringOriginalObject);
            JObject o2 = JObject.Parse(jsonStringDiffObject);

            foreach (var property in o2.Properties())
            {
                string name = property.Name;
                JToken value = property.Value;
                JToken oldValue = o1[name];
                if (oldValue == null)
                {
                    // The property doesn't exist in the old object, so add it.
                    o1.Add(name, value);
                }
                else if (value is JArray arrayValue && oldValue is JArray arrayOldValue)
                {
                    // The property is an array, so merge the items of the new array into the old array.
                    foreach (var item in arrayValue)
                    {
                        // Find the item with the same ID in the old array, or add a new item if it doesn't exist.
                        string itemId = (string)item["productId"];
                        JToken oldItem = arrayOldValue.FirstOrDefault(i => (string)i["productId"] == itemId);
                        if (oldItem == null)
                        {
                            arrayOldValue.Add(item);
                        }
                        else
                        {
                            // Update the properties of the old item with the properties of the new item.
                            foreach (var prop in item)
                            {
                                var key = ((JProperty)(prop)).Name;
                                var jvalue = ((JProperty)(prop)).Value;
                                oldItem[key] = jvalue;
                            }
                        }
                    }
                }
                else if (!JToken.DeepEquals(value, oldValue))
                {
                    // The property exists in the old object, but has a different value, so update it.
                    o1[name] = value;
                }
            }
            T jsonString = JsonSerializer.Deserialize<T>(o1.ToString());
            return jsonString;
        }

        private async Task<Basket> PatchBasketWithBasketAsync(Basket storedBasket, Basket basketUpdate)
        {
            var productsList = await _codeChallengeAPIService.GetAllProductsAsync();

            foreach (var product in basketUpdate.basketLines)
            {
                var storedProduct = storedBasket.basketLines.FirstOrDefault(p => p.productId == product.productId);

                if(storedProduct is not null)
                {
                    storedProduct.quantity = product.quantity;
                    storedProduct.totalPrice = storedProduct.quantity * storedProduct.productUnitPrice;

                } else
                {
                    storedBasket.basketLines.Add(product);
                }
            }
            storedBasket.totalAmount = storedBasket.basketLines.Sum(p => p.totalPrice);
            return storedBasket;
        }
    
        private async Task<Basket> ConvertRequestBasketToBasketAsync(BasketRequest basketRequest)
        {
            var productsList = await _codeChallengeAPIService.GetAllProductsAsync();
            var basket = TinyMapper.Map<Basket>(basketRequest);

            foreach(var product in basket.basketLines)
            {
                var productInCatalog = productsList.Where(x => x.Id == product.productId).FirstOrDefault();
                product.productUnitPrice = (decimal)productInCatalog.Price;
                product.totalPrice = (decimal) (productInCatalog.Price * product.quantity) ;
                product.productName = productInCatalog.Name;
            }

            basket.totalAmount = basket.basketLines.Sum(p => p.totalPrice);
            return basket;
        }
    }
}
