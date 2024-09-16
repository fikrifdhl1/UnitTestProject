using Moq;
using Xunit;
using BackendService.Services;
using BackendService.Models.Domain;
using BackendService.Models.DTO;
using BackendService.Repositories;
using BackendService.Utils.Logger;

namespace BackendService.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICustomeLogger> _mockLogger;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockLogger = new Mock<ICustomeLogger>();
            _productService = new ProductService(_mockProductRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateProduct()
        {
            var productDto = new CreateProductDTO
            {
                Name = "Product1",
                Description = "Description1",
                Price = 100.0f,
                Stock = 10
            };

            var result = await _productService.CreateAsync(productDto);

            Assert.True(result);
            _mockProductRepository.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Once);
            _mockProductRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteProduct()
        {
            var productId = 1;
            var product = new Product { Id = productId };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);

            var result = await _productService.DeleteAsync(productId);

            Assert.True(result);
            _mockProductRepository.Verify(repo => repo.RemoveAsync(product), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnProducts()
        {
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product1" },
                new Product { Id = 2, Name = "Product2" }
            };

            _mockProductRepository.Setup(repo => repo.GetAll()).ReturnsAsync(products);

            var result = await _productService.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProduct()
        {
            var productId = 1;
            var product = new Product { Id = productId };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);

            var result = await _productService.GetByIdAsync(productId);

            Assert.Equal(productId, result.Id);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateProduct()
        {
            var productDto = new UpdateProductDTO
            {
                Id = 1,
                Name = "UpdatedProduct",
                Price = 150.0f
            };

            var existingProduct = new Product { Id = 1, Name = "OldName", Price = 100.0f };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productDto.Id)).ReturnsAsync(existingProduct);

            var result = await _productService.UpdateAsync(productDto);

            Assert.True(result);
            _mockProductRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStockBulkAsync_ShouldUpdateStock()
        {
            var updateStockDtos = new List<UpdateStockDTO>
            {
                new UpdateStockDTO { Id = 1, Quantity = 5 },
                new UpdateStockDTO { Id = 2, Quantity = 10 }
            };

            var product1 = new Product { Id = 1, Stock = 10 };
            var product2 = new Product { Id = 2, Stock = 20 };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product1);
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(2)).ReturnsAsync(product2);

            var result = await _productService.UpdateStockBulkAsync(updateStockDtos);

            Assert.True(result);
            _mockProductRepository.Verify(repo => repo.Update(It.IsAny<Product>()), Times.Exactly(updateStockDtos.Count));
            _mockProductRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }
    }
}
