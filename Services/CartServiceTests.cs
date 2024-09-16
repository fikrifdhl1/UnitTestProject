using Moq;
using Xunit;
using BackendService.Services;
using BackendService.Models.Domain;
using BackendService.Models.DTO;
using BackendService.Repositories;
using BackendService.Utils.Logger;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using BackendService.Models.Enum;

namespace BackendService.Tests
{
    public class CartServiceTests
    {
        private readonly Mock<ICartRepository> _mockCartRepository;
        private readonly Mock<ICartItemRepository> _mockCartItemRepository;
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ICustomeLogger> _mockLogger;
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            _mockCartRepository = new Mock<ICartRepository>();
            _mockCartItemRepository = new Mock<ICartItemRepository>();
            _mockProductService = new Mock<IProductService>();
            _mockLogger = new Mock<ICustomeLogger>();
            _cartService = new CartService(
                _mockCartRepository.Object,
                _mockCartItemRepository.Object,
                _mockProductService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Create_ShouldCreateCart()
        {
            var cartDto = new CreateCartDTO { UserId = 1 };

            var result = await _cartService.Create(cartDto);

            Assert.True(result);
            _mockCartRepository.Verify(repo => repo.AddAsync(It.IsAny<Cart>()), Times.Once);
            _mockCartRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturnCartsWithItems()
        {
            var carts = new List<Cart>
            {
                new Cart { Id = 1, Items = new List<CartItem> { new CartItem { Id = 1, CartId = 1, Quantity = 2 } } },
                new Cart { Id = 2, Items = new List<CartItem> { new CartItem { Id = 2, CartId = 2, Quantity = 3 } } }
            };

            _mockCartRepository.Setup(repo => repo.GetAll()).ReturnsAsync(carts);
            _mockCartItemRepository.Setup(repo => repo.GetCartItemsByCartId(It.IsAny<int>())).ReturnsAsync((int cartId) =>
            {
                return carts.First(c => c.Id == cartId).Items;
            });

            var result = await _cartService.GetAll();

            Assert.Equal(2, result.Count());
            Assert.All(result, cart => Assert.NotEmpty(cart.Items));
        }

        [Fact]
        public async Task GetById_ShouldReturnCartWithItems()
        {
            var cartId = 1;
            var cart = new Cart { Id = cartId, Items = new List<CartItem> { new CartItem { Id = 1, CartId = cartId, Quantity = 2 } } };

            _mockCartRepository.Setup(repo => repo.GetByIdAsync(cartId)).ReturnsAsync(cart);
            _mockCartItemRepository.Setup(repo => repo.GetCartItemsByCartId(cartId)).ReturnsAsync(cart.Items);

            var result = await _cartService.GetById(cartId);

            Assert.Equal(cartId, result.Id);
            Assert.NotEmpty(result.Items);
        }

        [Fact]
        public async Task AddItemToCart_ShouldAddOrUpdateCartItem()
        {
            var cartItemDto = new CreateCartItemDTO
            {
                CartId = 1,
                ProductId = 1,
                Quantity = 5
            };

            var product = new Product { Id = 1, Stock = 10, Price = 100 };
            var existingCartItem = (CartItem)null;
            var cart = new Cart { Id = 1, TotalAmount = 0 };

            _mockProductService.Setup(p => p.GetByIdAsync(cartItemDto.ProductId)).ReturnsAsync(product);
            _mockCartItemRepository.Setup(repo => repo.GetCartItemsByCartIdAndProductId(cartItemDto.CartId, cartItemDto.ProductId)).ReturnsAsync(existingCartItem);
            _mockCartRepository.Setup(repo => repo.GetByIdAsync(cartItemDto.CartId)).ReturnsAsync(cart);

            var result = await _cartService.AddItemToCart(cartItemDto);

            Assert.True(result);
            _mockCartItemRepository.Verify(repo => repo.AddAsync(It.IsAny<CartItem>()), Times.Once);
            _mockCartRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Cart>()), Times.Once);
        }

        [Fact]
        public async Task CheckoutCart_ShouldUpdateCartStatusAndStock()
        {
            var checkoutDto = new CheckoutCartDTO { CartId = 1, UserId = 1 };
            var cart = new Cart
            {
                Id = 1,
                UserId = 1,
                Status = (int)WorkFlowCart.Active,
                Items = new List<CartItem>
                {
                    new CartItem { ProductId = 1, Quantity = 2 }
                }
            };

            _mockCartRepository.Setup(repo => repo.GetByIdAsync(checkoutDto.CartId)).ReturnsAsync(cart);
            _mockProductService.Setup(p => p.UpdateStockBulkAsync(It.IsAny<List<UpdateStockDTO>>())).ReturnsAsync(true);

            var result = await _cartService.CheckoutCart(checkoutDto);

            Assert.True(result);
            _mockCartRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Cart>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCartItem_ShouldUpdateExistingCartItem()
        {
            var updateDto = new UpdateCartItemDTO
            {
                Id = 1,
                CartId = 1,
                Quantity = 5
            };

            var cartItem = new CartItem { Id = 1, CartId = 1, Quantity = 3, TotalPrice = 300 };
            var product = new Product { Id = 1, Stock = 10, Price = 100 };
            var cart = new Cart { Id = 1, TotalAmount = 300 };

            _mockCartItemRepository.Setup(repo => repo.GetByIdAsync(updateDto.Id)).ReturnsAsync(cartItem);
            _mockCartRepository.Setup(repo => repo.GetByIdAsync(updateDto.CartId)).ReturnsAsync(cart);
            _mockProductService.Setup(p => p.GetByIdAsync(cartItem.ProductId)).ReturnsAsync(product);

            var result = await _cartService.UpdateCartItem(updateDto);

            Assert.True(result);
            _mockCartItemRepository.Verify(repo => repo.UpdateAsync(It.IsAny<CartItem>()), Times.Once);
            _mockCartRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Cart>()), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldRemoveCart()
        {
            var cartId = 1;
            var cart = new Cart { Id = cartId };

            _mockCartRepository.Setup(repo => repo.GetByIdAsync(cartId)).ReturnsAsync(cart);

            var result = await _cartService.Delete(cartId);

            Assert.True(result);
            _mockCartRepository.Verify(repo => repo.RemoveAsync(cart), Times.Once);
        }
    }
}
