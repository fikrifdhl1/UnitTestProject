using BackendService.Models.Domain;
using BackendService.Repositories;
using BackendService.Repositories.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace UnitTestProject.Repositories
{
    public class CartItemRepositoryTests
    {
        private readonly CartItemRepository _repository;
        private readonly CartDbContext _context;

        public CartItemRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<CartDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid().ToString())
                .Options;

            _context = new CartDbContext(options);
            _repository = new CartItemRepository(_context);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllCartItems()
        {
            var cartItem1 = new CartItem { Id = 1, CartId = 1, ProductId = 1, Quantity = 1, PriceUnit = 10.0f, TotalPrice = 10.0f, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var cartItem2 = new CartItem { Id = 2, CartId = 1, ProductId = 2, Quantity = 2, PriceUnit = 20.0f, TotalPrice = 40.0f, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.CartItesm.Add(cartItem1);
            _context.CartItesm.Add(cartItem2);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAll();

            Assert.Equal(2, result.Count());
            Assert.Contains(result, ci => ci.Id == 1);
            Assert.Contains(result, ci => ci.Id == 2);
        }

        [Fact]
        public async Task AddAsync_ShouldAddCartItem()
        {
            var cartItem = new CartItem { Id = 1, CartId = 1, ProductId = 1, Quantity = 1, PriceUnit = 10.0f, TotalPrice = 10.0f, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

            await _repository.AddAsync(cartItem);
            await _repository.SaveChangesAsync();

            var result = await _context.CartItesm.FindAsync(cartItem.Id);
            Assert.NotNull(result);
            Assert.Equal(1, result.ProductId);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCartItem()
        {
            var cartItem = new CartItem { Id = 1, CartId = 1, ProductId = 1, Quantity = 1, PriceUnit = 10.0f, TotalPrice = 10.0f, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.CartItesm.Add(cartItem);
            await _context.SaveChangesAsync();

            cartItem.Quantity = 2;
            await _repository.UpdateAsync(cartItem);
            await _repository.SaveChangesAsync();

            var result = await _context.CartItesm.FindAsync(cartItem.Id);
            Assert.NotNull(result);
            Assert.Equal(2, result.Quantity);
        }

        [Fact]
        public async Task RemoveAsync_ShouldRemoveCartItem()
        {
            var cartItem = new CartItem { Id = 1, CartId = 1, ProductId = 1, Quantity = 1, PriceUnit = 10.0f, TotalPrice = 10.0f, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.CartItesm.Add(cartItem);
            await _context.SaveChangesAsync();

            await _repository.RemoveAsync(cartItem);
            await _repository.SaveChangesAsync();

            var result = await _context.CartItesm.FindAsync(cartItem.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCartItemsByCartId_ShouldReturnCartItems()
        {
            var cartItem1 = new CartItem { Id = 1, CartId = 1, ProductId = 1, Quantity = 1, PriceUnit = 10.0f, TotalPrice = 10.0f, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var cartItem2 = new CartItem { Id = 2, CartId = 1, ProductId = 2, Quantity = 2, PriceUnit = 20.0f, TotalPrice = 40.0f, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.CartItesm.Add(cartItem1);
            _context.CartItesm.Add(cartItem2);
            await _context.SaveChangesAsync();

            var result = await _repository.GetCartItemsByCartId(1);

            Assert.Equal(2, result.Count());
            Assert.Contains(result, ci => ci.Id == 1);
            Assert.Contains(result, ci => ci.Id == 2);
        }

        [Fact]
        public async Task GetCartItemsByCartIdAndProductId_ShouldReturnCartItem()
        {
            var cartItem = new CartItem { Id = 1, CartId = 1, ProductId = 1, Quantity = 1, PriceUnit = 10.0f, TotalPrice = 10.0f, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.CartItesm.Add(cartItem);
            await _context.SaveChangesAsync();

            var result = await _repository.GetCartItemsByCartIdAndProductId(1, 1);

            Assert.NotNull(result);
            Assert.Equal(1, result.ProductId);
        }
    }
}
