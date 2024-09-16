using Moq;
using Xunit;
using BackendService.Services;
using BackendService.Models.Domain;
using BackendService.Models.DTO;
using BackendService.Repositories;
using BackendService.Utils.Logger;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;

namespace BackendService.Tests
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _mockTransactionRepository;
        private readonly Mock<ICartService> _mockCartService;
        private readonly Mock<ICustomeLogger> _mockLogger;
        private readonly TransactionService _transactionService;

        public TransactionServiceTests()
        {
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _mockCartService = new Mock<ICartService>();
            _mockLogger = new Mock<ICustomeLogger>();
            _transactionService = new TransactionService(
                _mockTransactionRepository.Object,
                _mockLogger.Object,
                _mockCartService.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateTransaction()
        {
            var createTransactionDto = new CreateTransactionDTO { UserId = 1, CartId = 1 };
            var cart = new Cart { Id = 1, TotalAmount = 100 };

            _mockCartService.Setup(cs => cs.GetById(createTransactionDto.CartId)).ReturnsAsync(cart);
            _mockCartService.Setup(cs => cs.CheckoutCart(It.IsAny<CheckoutCartDTO>())).ReturnsAsync(true);

            var newTransaction = new Transaction
            {
                CartId = createTransactionDto.CartId,
                UserId = createTransactionDto.UserId,
                TotalPrice = cart.TotalAmount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockTransactionRepository.Setup(repo => repo.AddAsync(It.IsAny<Transaction>())).Returns(Task.CompletedTask);
            _mockTransactionRepository.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _transactionService.CreateAsync(createTransactionDto);

            Assert.True(result);
            _mockTransactionRepository.Verify(repo => repo.AddAsync(It.Is<Transaction>(t =>
                t.CartId == createTransactionDto.CartId &&
                t.UserId == createTransactionDto.UserId &&
                t.TotalPrice == cart.TotalAmount
            )), Times.Once);
            _mockTransactionRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnTransactions()
        {
            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, UserId = 1, CartId = 1, TotalPrice = 100 },
                new Transaction { Id = 2, UserId = 2, CartId = 2, TotalPrice = 200 }
            };

            _mockTransactionRepository.Setup(repo => repo.GetAll()).ReturnsAsync(transactions);

            var result = await _transactionService.GetAllAsync();

            Assert.Equal(2, result.Count());
            Assert.Contains(result, t => t.Id == 1);
            Assert.Contains(result, t => t.Id == 2);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTransaction()
        {
            var transactionId = 1;
            var transaction = new Transaction { Id = transactionId, UserId = 1, CartId = 1, TotalPrice = 100 };

            _mockTransactionRepository.Setup(repo => repo.GetByIdAsync(transactionId)).ReturnsAsync(transaction);

            var result = await _transactionService.GetByIdAsync(transactionId);

            Assert.Equal(transactionId, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowException_WhenNotFound()
        {
            var transactionId = 1;
            _mockTransactionRepository.Setup(repo => repo.GetByIdAsync(transactionId)).ReturnsAsync((Transaction)null);

            await Assert.ThrowsAsync<BadHttpRequestException>(() => _transactionService.GetByIdAsync(transactionId));
        }
    }
}
