using BackendService.Models.Domain;
using BackendService.Repositories;
using BackendService.Repositories.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTestProject.Repositories
{
    public class TransactionRepositoryTests
    {
        private readonly TransactionRepository _repository;
        private readonly TransactionDbContext _context;

        public TransactionRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<TransactionDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid().ToString())
                .Options;

            _context = new TransactionDbContext(options);
            _repository = new TransactionRepository(_context);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllTransactions()
        {
            var transaction1 = new Transaction { Id = 1, UserId = 1, CartId = 1, TotalPrice = 100.0f, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var transaction2 = new Transaction { Id = 2, UserId = 2, CartId = 2, TotalPrice = 200.0f, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.Transactions.Add(transaction1);
            _context.Transactions.Add(transaction2);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAll();

            Assert.Equal(2, result.Count());
            Assert.Contains(result, t => t.Id == 1);
            Assert.Contains(result, t => t.Id == 2);
        }

        [Fact]
        public async Task AddAsync_ShouldAddTransaction()
        {
            var transaction = new Transaction { Id = 1, UserId = 1, CartId = 1, TotalPrice = 100.0f, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

            await _repository.AddAsync(transaction);
            await _repository.SaveChangesAsync();

            var result = await _context.Transactions.FindAsync(transaction.Id);
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTransaction()
        {
            var transaction = new Transaction { Id = 1, UserId = 1, CartId = 1, TotalPrice = 100.0f, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            transaction.TotalPrice = 150.0f;
            await _repository.UpdateAsync(transaction);
            await _repository.SaveChangesAsync();

            var result = await _context.Transactions.FindAsync(transaction.Id);
            Assert.NotNull(result);
            Assert.Equal(150.0f, result.TotalPrice);
        }

        [Fact]
        public async Task RemoveAsync_ShouldRemoveTransaction()
        {
            var transaction = new Transaction { Id = 1, UserId = 1, CartId = 1, TotalPrice = 100.0f, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            await _repository.RemoveAsync(transaction);
            await _repository.SaveChangesAsync();

            var result = await _context.Transactions.FindAsync(transaction.Id);
            Assert.Null(result);
        }
    }
}
