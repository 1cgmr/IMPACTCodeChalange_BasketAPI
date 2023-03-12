using BasketAPI.Models;
using BasketAPI.Services;
using CodeChallengeApiClientNamespace;
using Microsoft.AspNetCore.Mvc;

namespace BasketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private ICodeChallengeAPIService _codeChallengeAPIService;

        public ProductsController(ICodeChallengeAPIService codeChallengeAPIService)
        {
            _codeChallengeAPIService = codeChallengeAPIService;
        }

        [HttpGet]
        public async Task<ActionResult<ICollection<ProductResponse>>> GetProducts([FromQuery] PaginationParameters paginationParameters)
        {
            return StatusCode(StatusCodes.Status200OK, await _codeChallengeAPIService.GetPaginatedProductsAsync(paginationParameters));

        }

        [HttpGet("ranked")]
        public async Task<ActionResult<ICollection<ProductResponse>>> GetTopRankedProducts()
        {
            return StatusCode(StatusCodes.Status200OK, await _codeChallengeAPIService.GetTopRankedProductsAsync());

        }

    }
}
