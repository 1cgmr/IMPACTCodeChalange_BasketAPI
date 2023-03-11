using BasketAPI.Models;
using BasketAPI.Services;
using CodeChallengeApiClientNamespace;
using Microsoft.AspNetCore.Mvc;

namespace BasketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private IStorageService _storageService;
        private ICodeChallengeAPIService _codeChallengeAPIService;
        private IBasketService _basketService;

        public OrderController(IStorageService storageService, ICodeChallengeAPIService codeChallengeAPIService, IBasketService basketService)
        {
            _storageService = storageService;
            _codeChallengeAPIService = codeChallengeAPIService;
            _basketService = basketService;
        }

        [HttpPost("CreateOrder/{basketId}")]
        public async Task<ActionResult<OrderResponse>> CreateOrder(Guid basketId)
        {
            return StatusCode(StatusCodes.Status200OK, await _codeChallengeAPIService.CreateOrderAsync(basketId));
        }

        [HttpGet("GetOrder/{orderId}")]
        public async Task<ActionResult<OrderResponse>> GetOrder(string orderId)
        {
            return StatusCode(StatusCodes.Status200OK, await _codeChallengeAPIService.GetOrderAsync(orderId));
        }

    }
}
