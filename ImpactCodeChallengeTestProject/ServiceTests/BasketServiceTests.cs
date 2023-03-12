using BasketAPI.Exceptions;
using BasketAPI.Models;
using BasketAPI.Services;
using CodeChallengeApiClientNamespace;
using Moq;
using Nelibur.ObjectMapper;

namespace ImpactCodeChallengeTestProject.ServiceTests
{
    public class BasketServiceTests
    {
        private readonly Mock<IStorageService> _mockStorageService = new Mock<IStorageService>();
        private readonly Mock<ICodeChallengeAPIService> _mockCodeChallengeAPIService = new Mock<ICodeChallengeAPIService>();

        private readonly IBasketService _basketService;

        private IList<ProductResponse> mockProductList;

        public BasketServiceTests()
        {
            //TinyMapper bindings
            TinyMapper.Bind<BasketRequest, Basket>();
            TinyMapper.Bind<BasketLineRequest, BasketLine>();
            TinyMapper.Bind<BasketLine, OrderLine>();

            _basketService = new BasketService(_mockStorageService.Object, _mockCodeChallengeAPIService.Object);

            //Make the StoreBasket return the same object as given in argument
            _mockStorageService.Setup(x => x.StoreBasket(It.IsAny<Basket>())).Returns((Basket basket) => basket);
        }

        [Fact]
        public async Task CreateBasket_ValidRequest_CreatesAndStoresBasket()
        {
            // Arrange

            //Now the GetAllProductsAsync returns a moked list of products 
            mockProductList = new List<ProductResponse>
            {
                    new ProductResponse { Id = 1, Price = 1 },
                    new ProductResponse { Id = 2, Price = 2 }
            };
            _mockCodeChallengeAPIService.Setup(x => x.GetAllProductsAsync()).ReturnsAsync(mockProductList);

            var basketRequest = new BasketRequest
            {
                userEmail = "ola@gmail.com",
                basketLines = new List<BasketLineRequest>
                {
                    new BasketLineRequest { productId = 1, quantity = 1 },
                    new BasketLineRequest { productId = 2, quantity = 2 },
                }
            };

            // Act
            Basket createdBasket = await _basketService.CreateBasket(basketRequest);

            // Assert
            _mockStorageService.Verify(mock => mock.StoreBasket(It.IsAny<Basket>()), Times.Once);
            Assert.NotNull(createdBasket);
            Assert.Equal(basketRequest.userEmail, createdBasket.userEmail);
            Assert.Equal(basketRequest.basketLines.Count, createdBasket.basketLines.Count);
            foreach (var basketLineRequest in basketRequest.basketLines)
            {
                var matchingBasketLine = createdBasket.basketLines.FirstOrDefault(bl => bl.productId == basketLineRequest.productId);
                Assert.NotNull(matchingBasketLine);
                Assert.Equal(basketLineRequest.quantity, matchingBasketLine.quantity);
            }
            //total price of the basket is 5
            Assert.Equal(5, createdBasket.totalAmount);
        }

        [Fact]
        public async Task CreateBasket_InvalidProductList_ThrowsException()
        {
            // Arrange
            //Now the GetAllProductsAsync returns a moked list of products 
            mockProductList = new List<ProductResponse>
            {
                    new ProductResponse { Id = 1, Price = 1 },
                    new ProductResponse { Id = 99, Price = 2 }
            };
            _mockCodeChallengeAPIService.Setup(x => x.GetAllProductsAsync()).ReturnsAsync(mockProductList);

            var basketRequest = new BasketRequest
            {
                userEmail = "ola@gmail.com",
                basketLines = new List<BasketLineRequest>
                {
                    new BasketLineRequest { productId = 1, quantity = 1 },
                    new BasketLineRequest { productId = 2, quantity = 2 },
                }
            };

            // Act + Assert
            await Assert.ThrowsAsync<InvalidProductListException>(() => _basketService.CreateBasket(basketRequest));
            _mockStorageService.Verify(x => x.StoreBasket(It.IsAny<Basket>()), Times.Never);
            _mockCodeChallengeAPIService.Verify(x => x.GetAllProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task PatchBasket_ValidRequest_PatchesAndStoresBasket()
        {
            // Arrange
            //Now the GetAllProductsAsync returns a moked list of products 
            mockProductList = new List<ProductResponse>
            {
                    new ProductResponse { Id = 1, Price = 1 },
                    new ProductResponse { Id = 2, Price = 2 },
                    new ProductResponse { Id = 3, Price = 3 },
                    new ProductResponse { Id = 4, Price = 4 }
            };
            _mockCodeChallengeAPIService.Setup(x => x.GetAllProductsAsync()).ReturnsAsync(mockProductList);

            //This is the basket sent with the patch updates
            var basketRequest = new BasketRequest
            {
                userEmail = "ola@gmail.com",
                basketLines = new List<BasketLineRequest>
                {
                    new BasketLineRequest { productId = 1, quantity = 1 },
                    new BasketLineRequest { productId = 2, quantity = 2 },
                }
            };

            //This is the basket that was stored in memory
            Basket basket = new Basket
            {
                userEmail = "ola@gmail.com",
                basketLines = new List<BasketLine>
                {
                    new BasketLine { productId = 3, quantity = 2, productUnitPrice = 3, totalPrice = 6 },
                    new BasketLine { productId = 4, quantity = 2, productUnitPrice = 4, totalPrice = 8 },
                }
            };
            _mockStorageService.Setup(x => x.GetBasket(It.IsAny<Guid>())).Returns(basket);
            var guid = Guid.NewGuid();

            // Act
            Basket patchedBasket = await _basketService.PatchBasket(guid, basketRequest);

            // Assert
            _mockStorageService.Verify(mock => mock.StoreBasket(It.IsAny<Basket>()), Times.Once);
            _mockStorageService.Verify(mock => mock.GetBasket(It.IsAny<Guid>()), Times.Once);
            Assert.NotNull(patchedBasket);
            Assert.Equal(basketRequest.userEmail, patchedBasket.userEmail);
            foreach (var basketLineRequest in basketRequest.basketLines)
            {
                var matchingBasketLine = patchedBasket.basketLines.FirstOrDefault(bl => bl.productId == basketLineRequest.productId);
                Assert.NotNull(matchingBasketLine);
                Assert.Equal(basketLineRequest.quantity, matchingBasketLine.quantity);
            }

            //total price of the basket is now 10
            Assert.Equal(19, patchedBasket.totalAmount);
        }

        [Fact]
        public async Task PatchBasket_InvalidProductList_ThrowsException()
        {
            // Arrange

            //Now the GetAllProductsAsync returns a moked list of products 
            mockProductList = new List<ProductResponse>
            {
                    new ProductResponse { Id = 1, Price = 1 },
                    new ProductResponse { Id = 99, Price = 2 }
            };
            _mockCodeChallengeAPIService.Setup(x => x.GetAllProductsAsync()).ReturnsAsync(mockProductList);

            var basketRequest = new BasketRequest
            {
                userEmail = "ola@gmail.com",
                basketLines = new List<BasketLineRequest>
                {
                    new BasketLineRequest { productId = 1, quantity = 1 },
                    new BasketLineRequest { productId = 2, quantity = 2 },
                }
            };
            var basket = TinyMapper.Map<Basket>(basketRequest);
            _mockStorageService.Setup(x => x.GetBasket(It.IsAny<Guid>())).Returns(basket);
            var guid = Guid.NewGuid();

            // Act + Assert
            await Assert.ThrowsAsync<InvalidProductListException>(() => _basketService.PatchBasket(guid, basketRequest));
            //_mockStorageService.Verify(mock => mock.StoreBasket(It.IsAny<Basket>()), Times.Once);
            _mockStorageService.Verify(mock => mock.GetBasket(It.IsAny<Guid>()), Times.Once);
            _mockCodeChallengeAPIService.Verify(x => x.GetAllProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task PutBasket_ValidProductList_ReplacesBasket()
        {
            // Arrange
            //Now the GetAllProductsAsync returns a moked list of products 
            mockProductList = new List<ProductResponse>
            {
                    new ProductResponse { Id = 1, Price = 1 },
                    new ProductResponse { Id = 2, Price = 2 }
            };
            _mockCodeChallengeAPIService.Setup(x => x.GetAllProductsAsync()).ReturnsAsync(mockProductList);

            var basketRequest = new BasketRequest
            {
                userEmail = "ola@gmail.com",
                basketLines = new List<BasketLineRequest>
                {
                    new BasketLineRequest { productId = 1, quantity = 1 },
                    new BasketLineRequest { productId = 2, quantity = 2 },
                }
            };
            var basket = TinyMapper.Map<Basket>(basketRequest);
            _mockStorageService.Setup(x => x.DeleteBasket(It.IsAny<Guid>())).Returns(true);
            var guid = Guid.NewGuid();

            // Act
            Basket createdBasket = await _basketService.PutBasket(guid, basketRequest);

            // Assert
            _mockStorageService.Verify(mock => mock.StoreBasket(It.IsAny<Basket>()), Times.Once);
            _mockStorageService.Verify(mock => mock.DeleteBasket(It.IsAny<Guid>()), Times.Once);
            Assert.NotNull(createdBasket);
            Assert.Equal(basketRequest.userEmail, createdBasket.userEmail);
            Assert.Equal(basketRequest.basketLines.Count, createdBasket.basketLines.Count);
            foreach (var basketLineRequest in basketRequest.basketLines)
            {
                var matchingBasketLine = createdBasket.basketLines.FirstOrDefault(bl => bl.productId == basketLineRequest.productId);
                Assert.NotNull(matchingBasketLine);
                Assert.Equal(basketLineRequest.quantity, matchingBasketLine.quantity);
            }
            //total price of the basket is 5
            Assert.Equal(5, createdBasket.totalAmount);
        }

        [Fact]
        public async Task PutBasket_InvalidProductList_ThrowsException()
        {
            // Arrange
            //Now the GetAllProductsAsync returns a moked list of products 
            mockProductList = new List<ProductResponse>
            {
                    new ProductResponse { Id = 1, Price = 1 },
                    new ProductResponse { Id = 99, Price = 2 }
            };
            _mockCodeChallengeAPIService.Setup(x => x.GetAllProductsAsync()).ReturnsAsync(mockProductList);

            var basketRequest = new BasketRequest
            {
                userEmail = "ola@gmail.com",
                basketLines = new List<BasketLineRequest>
                {
                    new BasketLineRequest { productId = 1, quantity = 1 },
                    new BasketLineRequest { productId = 2, quantity = 2 },
                }
            };
            var basket = TinyMapper.Map<Basket>(basketRequest);
            _mockStorageService.Setup(x => x.DeleteBasket(It.IsAny<Guid>())).Returns(true);
            var guid = Guid.NewGuid();


            // Act + Assert
            await Assert.ThrowsAsync<InvalidProductListException>(() => _basketService.PutBasket(guid, basketRequest));
            //_mockStorageService.Verify(mock => mock.StoreBasket(It.IsAny<Basket>()), Times.Once);
            _mockStorageService.Verify(mock => mock.DeleteBasket(It.IsAny<Guid>()), Times.Once);
            _mockCodeChallengeAPIService.Verify(x => x.GetAllProductsAsync(), Times.Once);
        }

        [Fact]
        public void DeleteBasketProducts_CheckResult()
        {
            // Arrange

            //This is the basket that was stored in memory
            Basket basket = new Basket
            {
                userEmail = "ola@gmail.com",
                basketLines = new List<BasketLine>
                {
                    new BasketLine { productId = 3, quantity = 2, productUnitPrice = 3, totalPrice = 6 },
                    new BasketLine { productId = 4, quantity = 2, productUnitPrice = 4, totalPrice = 8 },
                }
            };
            _mockStorageService.Setup(x => x.GetBasket(It.IsAny<Guid>())).Returns(basket);
            var guid = Guid.NewGuid();
            List<int> productIdsToDelete = new List<int> { 4 };

            // Act
            Basket editedBasket = _basketService.DeleteBasketProducts(guid, productIdsToDelete);

            // Assert
            _mockStorageService.Verify(mock => mock.StoreBasket(It.IsAny<Basket>()), Times.Once);
            _mockStorageService.Verify(mock => mock.GetBasket(It.IsAny<Guid>()), Times.Once);
            Assert.NotNull(editedBasket);
            Assert.NotNull(editedBasket.basketLines.FirstOrDefault());
            Assert.Equal(1, editedBasket.basketLines.Count);
            Assert.Equal(3, editedBasket.basketLines.FirstOrDefault().productId);
        }
    }
}