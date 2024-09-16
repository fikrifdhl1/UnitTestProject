using BackendService.Models.Domain;
using BackendService.Repositories;
using BackendService.Repositories.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BackendService.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private readonly UserRepository _repository;
        private readonly UserDbContex _context;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<UserDbContex>()
                .UseInMemoryDatabase(databaseName: "UserDatabase" + Guid.NewGuid().ToString())
                .Options;

            _context = new UserDbContex(options);
            _repository = new UserRepository(_context);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllUsers()
        {
            var user1 = new User { Id = 1, Username = "user1", Email = "user1@example.com", HashedPassword = "hashedPassword1", Role = "Admin" };
            var user2 = new User { Id = 2, Username = "user2", Email = "user2@example.com", HashedPassword = "hashedPassword2", Role = "User" };
            _context.Users.Add(user1);
            _context.Users.Add(user2);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAll();

            Assert.Equal(2, result.Count());
            Assert.Contains(result, u => u.Username == "user1");
            Assert.Contains(result, u => u.Username == "user2");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser()
        {
            var user = new User { Id = 3, Username = "user1", Email = "user1@example.com", HashedPassword = "hashedPassword1", Role = "Admin" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal("user1", result.Username);
        }

        [Fact]
        public async Task AddAsync_ShouldAddUser()
        {
            var user = new User { Id = 4, Username = "user1", Email = "user1@example.com", HashedPassword = "hashedPassword1", Role = "Admin" };

            await _repository.AddAsync(user);
            await _repository.SaveChangesAsync();

            var result = await _context.Users.FindAsync(user.Id);
            Assert.NotNull(result);
            Assert.Equal("user1", result.Username);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateUser()
        {
            var user = new User { Id = 6, Username = "user1", Email = "user1@example.com", HashedPassword = "hashedPassword1", Role = "Admin" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.Username = "updatedUser";
            await _repository.UpdateAsync(user);

            var result = await _context.Users.FindAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal("updatedUser", result.Username);
        }

        [Fact]
        public async Task RemoveAsync_ShouldRemoveUser()
        {
            var user = new User { Id = 7, Username = "user1", Email = "user1@example.com", HashedPassword = "hashedPassword1", Role = "Admin" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _repository.RemoveAsync(user);
            await _repository.SaveChangesAsync();

            var result = await _context.Users.FindAsync(user.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldReturnUser()
        {
            var user = new User { Id = 5, Username = "user1", Email = "user1@example.com", HashedPassword = "hashedPassword1", Role = "Admin" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByUsernameAsync("user1");

            Assert.NotNull(result);
            Assert.Equal("user1", result.Username);
        }
    }
}
