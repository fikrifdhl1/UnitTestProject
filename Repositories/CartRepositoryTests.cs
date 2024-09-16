using BackendService.Models.Domain;
using BackendService.Repositories;
using BackendService.Repositories.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace UnitTestProject.Repositories
{
    public class CartRepositoryTests
    {
        private readonly CartRepository _repository;
        private readonly CartDbContext _context;

        public CartRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<CartDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid().ToString())
                .Options;

            _context = new CartDbContext(options);
            _repository = new CartRepository(_context);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllCarts()
        {
            var cart1 = new Cart { Id = 1, UserId = 1, TotalAmount = 100.0f, Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var cart2 = new Cart { Id = 2, UserId = 2, TotalAmount = 200.0f, Status = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.Carts.Add(cart1);
            _context.Carts.Add(cart2);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAll();

            Assert.Equal(2, result.Count());
            Assert.Contains(result, c => c.Id == 1);
            Assert.Contains(result, c => c.Id == 2);
        }

        [Fact]
        public async Task AddAsync_ShouldAddCart()
        {
            var cart = new Cart { Id = 1, UserId = 1, TotalAmount = 100.0f, Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

            await _repository.AddAsync(cart);
            await _repository.SaveChangesAsync();

            var result = await _context.Carts.FindAsync(cart.Id);
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCart()
        {
            var cart = new Cart { Id = 1, UserId = 1, TotalAmount = 100.0f, Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            cart.TotalAmount = 150.0f;
            await _repository.UpdateAsync(cart);
            await _repository.SaveChangesAsync();

            var result = await _context.Carts.FindAsync(cart.Id);
            Assert.NotNull(result);
            Assert.Equal(150.0f, result.TotalAmount);
        }

        [Fact]
        public async Task RemoveAsync_ShouldRemoveCart()
        {
            var cart = new Cart { Id = 1, UserId = 1, TotalAmount = 100.0f, Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            await _repository.RemoveAsync(cart);
            await _repository.SaveChangesAsync();

            var result = await _context.Carts.FindAsync(cart.Id);
            Assert.Null(result);
        }
    }
}
