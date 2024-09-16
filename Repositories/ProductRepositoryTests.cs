using BackendService.Models.Domain;
using BackendService.Repositories.DbContexts;
using BackendService.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject.Repositories
{
    public class ProductRepositoryTests
    {
        private readonly ProductRepository _repository;
        private readonly ProductDbContext _context;

        public ProductRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid().ToString())
                .Options;

            _context = new ProductDbContext(options);
            _repository = new ProductRepository(_context);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllProducts()
        {
            var product1 = new Product { Id = 1, Name = "Product1", Description = "Description1", Price = 10.0f, Stock = 100, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var product2 = new Product { Id = 2, Name = "Product2", Description = "Description2", Price = 20.0f, Stock = 200, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.Products.Add(product1);
            _context.Products.Add(product2);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAll();

            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.Name == "Product1");
            Assert.Contains(result, p => p.Name == "Product2");
        }

        [Fact]
        public async Task AddAsync_ShouldAddProduct()
        {
            var product = new Product { Id = 1, Name = "Product1", Description = "Description1", Price = 10.0f, Stock = 100, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

            await _repository.AddAsync(product);
            await _repository.SaveChangesAsync();

            var result = await _context.Products.FindAsync(product.Id);
            Assert.NotNull(result);
            Assert.Equal("Product1", result.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateProduct()
        {
            var product = new Product { Id = 1, Name = "Product1", Description = "Description1", Price = 10.0f, Stock = 100, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            product.Name = "UpdatedProduct";
            await _repository.UpdateAsync(product);
            await _repository.SaveChangesAsync();

            var result = await _context.Products.FindAsync(product.Id);
            Assert.NotNull(result);
            Assert.Equal("UpdatedProduct", result.Name);
        }

        [Fact]
        public async Task RemoveAsync_ShouldRemoveProduct()
        {
            var product = new Product { Id = 1, Name = "Product1", Description = "Description1", Price = 10.0f, Stock = 100, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _repository.RemoveAsync(product);
            await _repository.SaveChangesAsync();

            var result = await _context.Products.FindAsync(product.Id);
            Assert.Null(result);
        }
    }
}
