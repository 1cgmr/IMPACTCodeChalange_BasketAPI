using BasketAPI.Exceptions;
using BasketAPI.Models;
using BasketAPI.Services;
using CodeChallengeApiClientNamespace;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpactCodeChallengeTestProject.ServiceTests
{
    public class CodeChallangeAPIServiceTests
    {
        private readonly Mock<IStorageService> _mockStorageService = new Mock<IStorageService>();
        private readonly Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();
        private readonly Mock<ICodeChallengeApiClient> _mockApiClient = new Mock<ICodeChallengeApiClient>();

        private readonly ICodeChallengeAPIService _codeChallengeAPIService;

        List<ProductResponse> mockProductList;
        public CodeChallangeAPIServiceTests()
        {
            //TinyMapper bindings
            //TinyMapper.Bind<BasketRequest, Basket>();
            //TinyMapper.Bind<BasketLineRequest, BasketLine>();
            //TinyMapper.Bind<BasketLine, OrderLine>();

            _codeChallengeAPIService = new CodeChallengeAPIService(_mockStorageService.Object, _mockConfiguration.Object, _mockApiClient.Object);

            mockProductList = new List<ProductResponse>
            {
                    new ProductResponse { Id = 1, Price = 1, Stars = 4 },
                    new ProductResponse { Id = 2, Price = 2, Stars = 3 },
                    new ProductResponse { Id = 3, Price = 3, Stars = 2 },
                    new ProductResponse { Id = 4, Price = 4, Stars = 1 }
            };
            _mockStorageService.Setup(x => x.GetProductList()).Returns(mockProductList);


        }

        [Fact]
        public async Task LoginAsync_ValidApiResponse()
        {
            // Arrange
            var mockedLoginResponse = new LoginResponse()
            {
                Token = "1"
            };
            _mockApiClient.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync(mockedLoginResponse);


            // Act
            LoginResponse loginResponse = await _codeChallengeAPIService.LoginAsync();

            // Assert
            _mockApiClient.Verify(mock => mock.LoginAsync(It.IsAny<LoginRequest>()), Times.Once);
            Assert.NotNull(loginResponse);
            Assert.Equal(mockedLoginResponse.Token, loginResponse.Token);
        }

        [Fact]
        public async Task LoginAsync_InvalidAPIResponse_ThrowsException()
        {
            // Arrange
            var mockedLoginResponse = new LoginResponse()
            {
                Token = ""
            };
            _mockApiClient.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync(mockedLoginResponse);

            // Act + Assert
            await Assert.ThrowsAsync<Exception>(() => _codeChallengeAPIService.LoginAsync());
        }

        [Fact]
        public async Task GetPaginatedProductsAsync_ValidRequest_ReturnsPaginatedData()
        {
            // Arrange
            var paginationParameters = new PaginationParameters
            {
                pageNumber = 1,
                pageSize = 3
            };

            // Act
            var productResponse = await _codeChallengeAPIService.GetPaginatedProductsAsync(paginationParameters);

            // Assert
            Assert.NotNull(productResponse);
            Assert.Equal(3, productResponse.Count);
        }

        [Fact]
        public async Task GetTopRankedProductsAsync_ValidRequest_ReturnsPaginatedData()
        {
            // Arrange

            // Act
            var productResponse = await _codeChallengeAPIService.GetTopRankedProductsAsync();

            // Assert
            Assert.NotNull(productResponse);
            Assert.Equal(4, productResponse.Count);
            var expectedList = mockProductList.OrderByDescending(x => x.Stars);
            Assert.True(expectedList.SequenceEqual(productResponse));
        }

        [Fact]
        public async Task CreateOrderAsync_ValidApiResponse()
        {
            // Arrange
            _mockStorageService.Setup(x => x.GetBasket(It.IsAny<Guid>())).Returns(new Basket() { basketLines = new List<BasketLine>() });
            _mockStorageService.Setup(x => x.DeleteBasket(It.IsAny<Guid>())).Returns(true);
            _mockApiClient.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>())).ReturnsAsync(new OrderResponse());
            var mockGuid = Guid.NewGuid();
            // Act
            var productResponse = await _codeChallengeAPIService.CreateOrderAsync(mockGuid);

            // Assert
            Assert.NotNull(productResponse);
        }

        [Fact]
        public async Task CreateOrderAsync_InvalidApiResponse_ThrowsException()
        {
            // Arrange
            _mockStorageService.Setup(x => x.GetBasket(It.IsAny<Guid>())).Returns(new Basket() { basketLines = new List<BasketLine>() });
            _mockStorageService.Setup(x => x.DeleteBasket(It.IsAny<Guid>())).Returns(true);
            _mockApiClient.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>())).Throws(new Exception());
            var mockGuid = Guid.NewGuid();

            // Act + Assert
            await Assert.ThrowsAsync<Exception>(() => _codeChallengeAPIService.CreateOrderAsync(mockGuid));
        }

        [Fact]
        public async Task GetOrderAsync_ValidApiResponse()
        {
            // Arrange
            _mockApiClient.Setup(x => x.GetOrderAsync(It.IsAny<string>())).ReturnsAsync(new OrderResponse());
            // Act
            var productResponse = await _codeChallengeAPIService.GetOrderAsync("");

            // Assert
            Assert.NotNull(productResponse);
        }

        [Fact]
        public async Task GetOrderAsync_InvalidApiResponse_ThrowsException()
        {
            // Arrange
            _mockApiClient.Setup(x => x.GetOrderAsync(It.IsAny<string>())).Throws(new Exception());

            // Act + Assert
            await Assert.ThrowsAsync<Exception>(() => _codeChallengeAPIService.GetOrderAsync(""));
        }

    }
}
