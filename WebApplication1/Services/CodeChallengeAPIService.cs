using BasketAPI.Models;
using CodeChallengeApiClientNamespace;
using Microsoft.Extensions.Caching.Memory;
using Nelibur.ObjectMapper;

namespace BasketAPI.Services
{
    public class CodeChallengeAPIService : ICodeChallengeAPIService
    {
        private CodeChallengeApiClient _apiClient;
        private readonly IStorageService _storageService;
        private readonly IConfiguration _configuration;

        public CodeChallengeAPIService(IStorageService storageService, IConfiguration configuration)
        {
            _apiClient = new CodeChallengeApiClient(new HttpClient());
            _apiClient.setDI(storageService, configuration);
            _storageService = storageService;
            _configuration = configuration;

        }

        public async Task<ICollection<ProductResponse>> GetAllProductsAsync()
        {
            return await GetProductList();
        }

        public async Task<LoginResponse> LoginAsync()
        {
            LoginRequest loginrequest = new LoginRequest()
            {
                Email = _configuration["LoginEmail"]
            };

            LoginResponse result = await _apiClient.LoginAsync(loginrequest);

            if (result == null || result.Token is null || result.Token == string.Empty)
                throw new Exception(String.Format("Failed to retrive authentication token for email: {0}", _configuration["LoginEmail"]));

            _storageService.StoreToken(result.Token);
            return result;
        }

        public async Task<ICollection<ProductResponse>> GetPaginatedProductsAsync(PaginationParameters paginationParameters)
        {
            ICollection<ProductResponse> products = await GetProductList();

            //Return a paginated result of the product catalog ordered by price in ascending order and properly paginated. 
            return products.OrderBy(p => p.Price)
                    .Skip((paginationParameters.pageNumber - 1) * paginationParameters.pageSize)
                    .Take(paginationParameters.pageSize)
                    .ToList();
        }

        public async Task<ICollection<ProductResponse>> GetTopRankedProducts()
        {
            ICollection<ProductResponse> products = await GetProductList();

            //Return the top ranked 100 products.
            return products.OrderByDescending(p => p.Stars).ThenBy(p => p.Price)
                    .Take(100)
                    .ToList();
        }

        private async Task<ICollection<ProductResponse>> GetProductList()
        {
            ICollection<ProductResponse> products = _storageService.GetProductList();
            if (products is null)
            {
                // List not in storage, get new Products list from API.
                products = await _apiClient.GetAllProductsAsync();
                _storageService.StoreProductList(products);
            }

            if (products is null || products.Count == 0)
            {
                throw new Exception("List of retrived products is null or empty.");
            }
            return products;
        }

        public async Task<OrderResponse> CreateOrderAsync(Guid basketId)
        {
            var basket = _storageService.GetBasket(basketId);

            var orderRequest = CreateOrderRequestFromBasket(basket);
            var orderResponse = await _apiClient.CreateOrderAsync(orderRequest);

            if (orderResponse is null)
                throw new Exception("Failed to create order");

            _storageService.DeleteBasket(basketId);

            return orderResponse;
        }

        private CreateOrderRequest CreateOrderRequestFromBasket(Basket basket)
        {
            CreateOrderRequest orderResquest = new CreateOrderRequest()
            {
                TotalAmount = (double)basket.totalAmount,
                UserEmail = basket.userEmail,
                OrderLines = new List<OrderLine>()
            };

            foreach(var line in basket.basketLines)
            {
                orderResquest.OrderLines.Add(new OrderLine()
                {
                    ProductId = line.productId,
                    ProductName = line.productName,
                    ProductSize = "",
                    ProductUnitPrice = (double)line.productUnitPrice,
                    Quantity = line.quantity,
                    TotalPrice = (double)line.totalPrice
                });
            }

            return orderResquest;
        }

        public async Task<OrderResponse> GetOrderAsync(string orderId)
        {
            OrderResponse result = await _apiClient.GetOrderAsync(orderId);
            return result;
        }
    }
}
