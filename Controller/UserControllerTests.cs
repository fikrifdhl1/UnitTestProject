using BackendService.Controllers;
using BackendService.Models.DTO;
using BackendService.Services;
using BackendService.Utils.Logger;
using BackendService.Validators.User;
using BPKBBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;


namespace UnitTestProject.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ICustomeLogger> _loggerMock;
        private readonly Mock<IUserValidator> _validatorMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _loggerMock = new Mock<ICustomeLogger>();
            _validatorMock = new Mock<IUserValidator>();
            _controller = new UserController(_userServiceMock.Object, _loggerMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenValidRequest()
        {
            var loginDto = new LoginRequestDTO { Username = "user1", Password = "password" };
            var token = "valid-token";

            _validatorMock.Setup(v => v.Login().Validate(loginDto)).Returns(new FluentValidation.Results.ValidationResult());
            _userServiceMock.Setup(s => s.Login(loginDto)).ReturnsAsync(token);

            var result = await _controller.Login(loginDto) as OkObjectResult;
            var response = result?.Value as ApiResponse<LoginResponseDTO>;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result?.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(token, response?.Data?.Token);
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenInvalidRequest()
        {
            var loginDto = new LoginRequestDTO { Username = "user1" };

            _validatorMock.Setup(v => v.Login().Validate(loginDto)).Returns(new FluentValidation.Results.ValidationResult(new[] { new FluentValidation.Results.ValidationFailure("Username", "Username is required") }));

            var result = await _controller.Login(loginDto) as BadRequestObjectResult;
            var response = result?.Value as ApiResponse<object>;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result?.StatusCode);
            Assert.Equal("Validation Error", response?.Message);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnSuccess_WhenValidRequest()
        {
            var userDto = new UserToCreateDTO { Username = "user1", Password = "password", Email = "user1@example.com", Role = "Admin" };

            _validatorMock.Setup(v => v.CreateUser().Validate(userDto)).Returns(new FluentValidation.Results.ValidationResult());
            _userServiceMock.Setup(s => s.Create(userDto)).ReturnsAsync(true);

            var result = await _controller.CreateUser(userDto) as OkObjectResult;
            var response = result?.Value as ApiResponse<object>;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result?.StatusCode);
            Assert.Equal("Success creating user", response?.Message);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnSuccess_WhenValidRequest()
        {
            var userDto = new UserToUpdateDTO { Id = 1, Email = "newemail@example.com", Password = "newpassword" };

            _validatorMock.Setup(v => v.UpdateUser().Validate(userDto)).Returns(new FluentValidation.Results.ValidationResult());
            _userServiceMock.Setup(s => s.Update(userDto)).ReturnsAsync(true);

            var result = await _controller.UpdateUser(1, userDto) as OkObjectResult;
            var response = result?.Value as ApiResponse<object>;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result?.StatusCode);
            Assert.Equal("Success updating user", response?.Message);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnSuccess()
        {
            var userId = 1;

            _userServiceMock.Setup(s => s.Delete(userId)).ReturnsAsync(true);

            var result = await _controller.DeleteUser(userId) as OkObjectResult;
            var response = result?.Value as ApiResponse<object>;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result?.StatusCode);
            Assert.Equal("Success deleting user", response?.Message);
        }

    }
}
