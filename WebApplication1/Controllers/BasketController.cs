using BasketAPI.Models;
using BasketAPI.Services;
using CodeChallengeApiClientNamespace;
using Microsoft.AspNetCore.Mvc;

namespace BasketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasketController : ControllerBase
    {
        private IStorageService _storageService;
        private ICodeChallengeAPIService _codeChallengeAPIService;
        private IBasketService _basketService;

        public BasketController(IStorageService storageService, ICodeChallengeAPIService codeChallengeAPIService, IBasketService basketService)
        {
            _storageService = storageService;
            _codeChallengeAPIService = codeChallengeAPIService;
            _basketService = basketService;
        }

        [HttpGet("id")]
        public IActionResult GetBasketById(Guid id)
        {
            return StatusCode(StatusCodes.Status200OK, _storageService.GetBasket(id));
        }

        [HttpPost("Create")]
        public async Task<ActionResult<Basket>> CreateBasket(BasketRequest basketRequest)
        {
            return StatusCode(StatusCodes.Status200OK, await _basketService.CreateBasket(basketRequest));
        }

        [HttpPut("id")]
        public async Task<ActionResult<Basket>> PutBasket(Guid id, BasketRequest basketRequest)
        {
            //For this implemntation in the PATCH and PUT methods if the basket does not exists I will not create a new one.
            return StatusCode(StatusCodes.Status200OK, await _basketService.PutBasket(id, basketRequest));
        }

        [HttpPatch("id")]
        public async Task<ActionResult<Basket>> PatchBasket(Guid id, BasketRequest basketRequest)
        {
            //For this implemntation in the PATCH and PUT methods if the basket does not exists I will not create a new one.
            return StatusCode(StatusCodes.Status200OK, await _basketService.PatchBasket(id, basketRequest));
        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteBasket(Guid id)
        {
            return StatusCode(StatusCodes.Status200OK, _basketService.DeleteBasket(id));
        }

        [HttpDelete]
        [Route("DeleteProducts/id")]
        public async Task<IActionResult> DeleteProducts(Guid id, [FromQuery] List<int> productIds)
        {
            //For this implementation, if the products that we try to delete are not valid or present in the basket, no error is given.
            return StatusCode(StatusCodes.Status200OK, _basketService.DeleteBasketProducts(id, productIds));
        }

        [HttpPost("Commit/id")]
        public async Task<IActionResult> CommitBasket(Guid id)
        {
            return StatusCode(StatusCodes.Status200OK, "");

        }

    }
}
