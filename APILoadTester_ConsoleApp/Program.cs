using BasketAPI.Services;
using CodeChallengeApiClientNamespace;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using BasketAPI.Models;
using Newtonsoft.Json;

Console.WriteLine("Hello, World!");

//DI initialization
    // Create a service collection
    var services = new ServiceCollection();

    // Add services to the collection

    // Build a service provider from the collection
    //setup our DI
    IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appSettings.json", false)
            .Build();

    var serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddMemoryCache()
        .AddHttpClient()
        .AddSingleton<IConfiguration>(configuration)
        .AddSingleton<ICodeChallengeApiClient, CodeChallengeApiClient>()
        .AddSingleton<ICodeChallengeAPIService, CodeChallengeAPIService>()
        .AddSingleton<IBasketService, BasketService>()
        .AddSingleton<IStorageService, MemoryCacheStorageService>()
        .BuildServiceProvider();


// Get an instance of ICodeChallengeAPIService from the service provider
var ccService = serviceProvider.GetService<ICodeChallengeAPIService>();

int NUM_ORDERS_TO_CREATE = 1000;
int MAX_NUM_ITEMS_PER_BASKET = 100;
int MIN_NUM_ITEMS_PER_BASKET = 1;

//Get the full list of 10000 products
var fullListOfProducts = await ccService.GetAllProductsAsync();


List<BasketRequest> listOfRandomBaskets = new List<BasketRequest>();
Random rnd = new Random();

for (int i = 0; i < NUM_ORDERS_TO_CREATE; i++)
{
    int n = rnd.Next(MIN_NUM_ITEMS_PER_BASKET, MAX_NUM_ITEMS_PER_BASKET + 1);
    List<ProductResponse> randomProducts = fullListOfProducts.OrderBy(x => rnd.Next()).Take(n).ToList();
    List<BasketLineRequest> basketLines = new List<BasketLineRequest>();

    foreach(var product in randomProducts)
    {
        basketLines.Add(new BasketLineRequest()
        {
            productId = product.Id,
            quantity = rnd.Next(1, 20)
        });
    }

    listOfRandomBaskets.Add(new BasketRequest()
    {
        basketLines = basketLines,
        userEmail = "cgmr.321@gmail.com"
    });
}

// Create RestClient instance with base URL
var client = new RestClient("https://localhost:7218/");
RestRequest request;
RestResponse response;

//this will store the IDs of the baskets returned by the API.
List<Guid> createdBasketsIds = new List<Guid>();

//This loop will perform the requests to create baskets from our list of basket requests.
foreach(var basketRequest in listOfRandomBaskets)
{
    // Create request object for POST method and add request body
    request = new RestRequest("/api/Basket/Create", Method.Post);
    request.AddJsonBody(basketRequest);

    // Execute the request and get response content
    response = client.Execute(request);

    //Extract basket ID from response
    if(response != null && response.Content != null)
    {
        Basket storedBasket = JsonConvert.DeserializeObject<Basket>(response.Content);
        createdBasketsIds.Add(storedBasket.basketId);
        Console.WriteLine(String.Format("Created basket on API with {0} products. ID: {1}", storedBasket.basketLines.Count, storedBasket.basketId));
    }
}

List<string> orderIds = new List<string>();

//this loop will ask the API to creat orders with the created baskets
foreach(Guid guid in createdBasketsIds)
{
    request = new RestRequest("/api/Order/CreateOrder/{basketId}", Method.Post);
    request.AddUrlSegment("basketId", guid);

    // Execute the request and get response content
    response = client.Execute(request);

    //Extract Order ID from response
    if (response != null && response.Content != null)
    {
        OrderResponse storedOrder = JsonConvert.DeserializeObject<OrderResponse>(response.Content);
        orderIds.Add(storedOrder.OrderId);
        Console.WriteLine(String.Format("Created order on IMPACT API with {0} products. ID: {1}", storedOrder.OrderLines.Count, storedOrder.OrderId));
    }
}


Console.ReadLine();