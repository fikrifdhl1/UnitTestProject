using BackendService.Models.Domain;
using BackendService.Models.DTO;
using BackendService.Repositories;
using BackendService.Services;
using BackendService.Utils;
using BackendService.Utils.Logger;
using BPKBBackend.Models;
using BPKBBackend.Utils;
using Microsoft.AspNetCore.Http;
using Moq;


namespace UnitTestProject.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICustomeLogger> _loggerMock;
        private readonly Mock<ICustomeHash> _hashMock;
        private readonly Mock<IJwtHelper> _jwtHelperMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ICustomeLogger>();
            _hashMock = new Mock<ICustomeHash>();
            _jwtHelperMock = new Mock<IJwtHelper>();
            _userService = new UserService(_userRepositoryMock.Object, _loggerMock.Object, _hashMock.Object, _jwtHelperMock.Object);
        }

        [Fact]
        public async Task Create_ShouldAddUser()
        {
            var userDto = new UserToCreateDTO { Username = "user1", Password = "password", Email = "user1@example.com", Role = "Admin" };
            _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(userDto.Username)).ReturnsAsync((User)null);
            _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _userRepositoryMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);
            _hashMock.Setup(hash => hash.Hash(userDto.Password)).Returns("hashedPassword");

            var result = await _userService.Create(userDto);

            Assert.True(result);
            _userRepositoryMock.Verify(repo => repo.AddAsync(It.Is<User>(u => u.Username == userDto.Username)), Times.Once);
        }

        [Fact]
        public async Task Create_ShouldThrowExceptionIfUserExists()
        {
            var userDto = new UserToCreateDTO { Username = "user1" };
            _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(userDto.Username)).ReturnsAsync(new User());

            await Assert.ThrowsAsync<BadHttpRequestException>(() => _userService.Create(userDto));
        }

        [Fact]
        public async Task Delete_ShouldRemoveUser()
        {
            var userId = 1;
            var user = new User { Id = userId };
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.RemoveAsync(user)).Returns(Task.CompletedTask);

            var result = await _userService.Delete(userId);

            Assert.True(result);
            _userRepositoryMock.Verify(repo => repo.RemoveAsync(user), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllUsers()
        {
            var users = new List<User>
            {
                new User { Id = 1, Username = "user1" },
                new User { Id = 2, Username = "user2" }
            };
            _userRepositoryMock.Setup(repo => repo.GetAll()).ReturnsAsync(users);

            var result = await _userService.GetAll();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetById_ShouldReturnUser()
        {
            var userId = 1;
            var user = new User { Id = userId, Username = "user1" };
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            var result = await _userService.GetById(userId);

            Assert.NotNull(result);
            Assert.Equal("user1", result.Username);
        }

        [Fact]
        public async Task Login_ShouldReturnToken()
        {
            var loginDto = new LoginRequestDTO { Username = "user1", Password = "password" };
            var user = new User { Username = loginDto.Username, HashedPassword = "hashedPassword" };
            _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(loginDto.Username)).ReturnsAsync(user);
            _hashMock.Setup(hash => hash.Compare(loginDto.Password, user.HashedPassword)).Returns(true);
            _jwtHelperMock.Setup(jwt => jwt.GenerateToken(loginDto.Username, user.Id)).Returns("token");

            var result = await _userService.Login(loginDto);

            Assert.Equal("token", result);
        }

        [Fact]
        public async Task Login_ShouldThrowExceptionForInvalidCredentials()
        {
            var loginDto = new LoginRequestDTO { Username = "user1", Password = "password" };
            _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(loginDto.Username)).ReturnsAsync((User)null);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.Login(loginDto));
        }

        [Fact]
        public async Task Update_ShouldUpdateUser()
        {
            var userDto = new UserToUpdateDTO { Id = 1, Email = "newemail@example.com", Password = "newpassword" };
            var existingUser = new User { Id = userDto.Id, Email = "oldemail@example.com", HashedPassword = "oldhashedpassword" };
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userDto.Id)).ReturnsAsync(existingUser);
            _hashMock.Setup(hash => hash.Hash(userDto.Password)).Returns("newhashedpassword");
            _userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var result = await _userService.Update(userDto);

            Assert.True(result);
            Assert.Equal("newemail@example.com", existingUser.Email);
            Assert.Equal("newhashedpassword", existingUser.HashedPassword);
        }

        [Fact]
        public async Task Update_ShouldNotChangePasswordIfNotProvided()
        {
            var userDto = new UserToUpdateDTO { Id = 1, Email = "newemail@example.com" };
            var existingUser = new User { Id = userDto.Id, Email = "oldemail@example.com", HashedPassword = "oldhashedpassword" };
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userDto.Id)).ReturnsAsync(existingUser);
            _userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var result = await _userService.Update(userDto);

            Assert.True(result);
            Assert.Equal("newemail@example.com", existingUser.Email);
            Assert.Equal("oldhashedpassword", existingUser.HashedPassword);
        }
    }
}
