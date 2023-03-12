using BasketAPI.Exceptions;
using BasketAPI.Models;
using BasketAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpactCodeChallengeTestProject.ServiceTests
{
    public class StorageServiceTests
    {
        private readonly Mock<IMemoryCache> _mockMemoryCache = new Mock<IMemoryCache>();

        private readonly IStorageService _storageService;
        delegate void OutDelegate<TIn, TOut>(TIn input, out TOut output);
        public StorageServiceTests()
        {
            _storageService = new MemoryCacheStorageService(_mockMemoryCache.Object);

            //This is the basket that was stored in memory

            //Make the StoreBasket return the same object as given in argument
            //_mockMemoryCache.Setup(x => x.TryGetValue(basket.basketId, out basket)).Returns(true);
            //_mockMemoryCache.Setup(x => x.Set(basket.basketId, basket, TimeSpan.FromDays(60))).Returns(basket);
        }

        [Fact]
        public void StoreBasket_ValidRequest_StoresAndReturnsBasket()
        {
            // Arrange
            //This is the basket that was stored in memory

            Basket basket = new Basket
            {
                basketId = Guid.NewGuid(),
                userEmail = "ola@gmail.com",
                basketLines = new List<BasketLine>
                {
                    new BasketLine { productId = 3, quantity = 2, productUnitPrice = 3, totalPrice = 6 },
                    new BasketLine { productId = 4, quantity = 2, productUnitPrice = 4, totalPrice = 8 },
                }
            };
            var cachEntry = Mock.Of<ICacheEntry>();
            _mockMemoryCache.Setup(m => m.CreateEntry(It.IsAny<object>()))
                            .Returns(cachEntry);

            // Act
            Basket storedBasket = _storageService.StoreBasket(basket);

            // Assert
            Assert.NotNull(storedBasket);
            Assert.Equal(basket, storedBasket);
        }

        [Fact]
        public void GetBasket_ValidRequest_StoresAndReturnsBasket()
        {
            // Arrange
            //This is the basket that was stored in memory
            Guid guid = Guid.NewGuid();
            Basket basket = new Basket
            {
                basketId = guid,
                userEmail = "ola@gmail.com",
                basketLines = new List<BasketLine>
                {
                    new BasketLine { productId = 3, quantity = 2, productUnitPrice = 3, totalPrice = 6 },
                    new BasketLine { productId = 4, quantity = 2, productUnitPrice = 4, totalPrice = 8 },
                }
            };
            object expected = null;
            _mockMemoryCache.Setup(mc => mc.TryGetValue(It.IsAny<object>(), out expected))
                            .Callback(new OutDelegate<object, object>((object k, out object v) =>
                                v = basket)) // mocked value here
                            .Returns(true);

            // Act
            Basket storedBasket = _storageService.GetBasket(guid);

            // Assert
            Assert.NotNull(storedBasket);
            Assert.Equal(basket, storedBasket);
        }

        [Fact]
        public void GetBasket_ThrowException_Basket_NotFound()
        {
            // Arrange
            //This is the basket that was stored in memory
            Guid guid = Guid.NewGuid();

            object expected = null;
            _mockMemoryCache.Setup(mc => mc.TryGetValue(It.IsAny<object>(), out expected))
                            .Callback(new OutDelegate<object, object>((object k, out object v) =>
                                v = null)) // mocked value here
                            .Returns(false);

            // Act + Assert
             Assert.Throws<BasketNotFoundException>(() => _storageService.GetBasket(guid));
        }
    }
}
