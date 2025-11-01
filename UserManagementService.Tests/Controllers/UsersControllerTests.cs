using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Net;
using UserManagementService.Api.Controllers;
using UserManagementService.Core.DTOs;
using UserManagementService.Services.Exceptions;
using UserManagementService.Services.Interfaces;

namespace UserManagementService.Tests.Controllers;

[TestFixture]
public class UsersControllerTests
{
    private Mock<IUserService> _mockUserService;
    private Mock<ILogger<UsersController>> _mockLogger;
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private UsersController _controller;
    private DefaultHttpContext _httpContext;

    [SetUp]
    public void SetUp()
    {
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UsersController>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        _httpContext = new DefaultHttpContext();
        _httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        _httpContext.Items["ClientId"] = 1;
        _httpContext.Items["ClientName"] = "TestClient";

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext);

        _controller = new UsersController(
            _mockUserService.Object,
            _mockLogger.Object,
            _mockHttpContextAccessor.Object);
    }

    [Test]
    public async Task CreateUser_ValidRequest_ReturnsCreatedAtActionWithUser()
    {
        var request = new CreateUserRequest
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        var expectedUser = new UserResponse
        {
            Id = 1,
            UserName = "testuser",
            Email = "test@example.com"
        };

        _mockUserService.Setup(x => x.CreateUserAsync(request, 1))
            .ReturnsAsync(expectedUser);

        var result = await _controller.CreateUser(request);

        var createdAtActionResult = result.Result as CreatedAtActionResult;
        Assert.That(createdAtActionResult, Is.Not.Null);
        Assert.That(createdAtActionResult.StatusCode, Is.EqualTo(201));
        Assert.That(createdAtActionResult.ActionName, Is.EqualTo(nameof(_controller.GetUser)));
        Assert.That(createdAtActionResult.Value, Is.EqualTo(expectedUser));
    }

    [Test]
    public async Task CreateUser_DuplicateUser_ReturnsBadRequest()
    {
        var request = new CreateUserRequest
        {
            UserName = "duplicate",
            Email = "duplicate@example.com",
            Password = "Password123!"
        };

        _mockUserService.Setup(x => x.CreateUserAsync(request, 1))
            .ThrowsAsync(new DuplicateUserException("User already exists"));

        var result = await _controller.CreateUser(request);

        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
        Assert.That(badRequestResult.Value, Is.EqualTo("User already exists"));
    }

    [Test]
    public async Task CreateUser_ServiceThrowsException_ReturnsInternalServerError()
    {
        var request = new CreateUserRequest
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        _mockUserService.Setup(x => x.CreateUserAsync(request, 1))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.CreateUser(request);

        var statusCodeResult = result.Result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(500));
        Assert.That(statusCodeResult.Value, Is.EqualTo("An error occurred while creating the user"));
    }

    [Test]
    public async Task GetUser_ExistingUser_ReturnsOkWithUser()
    {
        var expectedUser = new UserResponse
        {
            Id = 1,
            UserName = "testuser",
            Email = "test@example.com"
        };

        _mockUserService.Setup(x => x.GetUserAsync(1, 1))
            .ReturnsAsync(expectedUser);

        var result = await _controller.GetUser(1);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.EqualTo(expectedUser));
    }

    [Test]
    public async Task GetUser_NonExistingUser_ReturnsNotFound()
    {
        _mockUserService.Setup(x => x.GetUserAsync(-1, 1))
            .ThrowsAsync(new UserNotFoundException("User not found"));

        var result = await _controller.GetUser(-1);

        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
        Assert.That(notFoundResult.Value, Is.EqualTo("User not found"));
    }

    [Test]
    public async Task GetUser_ServiceThrowsException_ReturnsInternalServerError()
    {
        _mockUserService.Setup(x => x.GetUserAsync(1, 1))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetUser(1);

        var statusCodeResult = result.Result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(500));
        Assert.That(statusCodeResult.Value, Is.EqualTo("An error occurred while retrieving the user"));
    }

    [Test]
    public async Task GetAllUsers_UsersExist_ReturnsOkWithUserList()
    {
        var expectedUsers = new List<UserResponse>
        {
            new UserResponse { Id = 1, UserName = "user1", Email = "user1@example.com" },
            new UserResponse { Id = 2, UserName = "user2", Email = "user2@example.com" }
        };

        _mockUserService.Setup(x => x.GetAllUsersAsync(1))
            .ReturnsAsync(expectedUsers);

        var result = await _controller.GetAllUsers();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.EqualTo(expectedUsers));
    }

    [Test]
    public async Task GetAllUsers_NoUsers_ReturnsOkWithEmptyList()
    {
        var expectedUsers = new List<UserResponse>();

        _mockUserService.Setup(x => x.GetAllUsersAsync(1))
            .ReturnsAsync(expectedUsers);

        var result = await _controller.GetAllUsers();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        var users = okResult.Value as List<UserResponse>;
        Assert.That(users, Is.Not.Null);
        Assert.That(users.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetAllUsers_ServiceThrowsException_ReturnsInternalServerError()
    {
        _mockUserService.Setup(x => x.GetAllUsersAsync(1))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetAllUsers();

        var statusCodeResult = result.Result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(500));
        Assert.That(statusCodeResult.Value, Is.EqualTo("An error occurred while retrieving users"));
    }

    [Test]
    public async Task UpdateUser_ValidRequest_ReturnsOkWithUpdatedUser()
    {
        var request = new UpdateUserRequest
        {
            FullName = "updateduser",
            Email = "updated@example.com"
        };

        var expectedUser = new UserResponse
        {
            Id = 1,
            FullName = "updateduser",
            Email = "updated@example.com"
        };

        _mockUserService.Setup(x => x.UpdateUserAsync(1, request, 1))
            .ReturnsAsync(expectedUser);

        var result = await _controller.UpdateUser(1, request);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.EqualTo(expectedUser));
    }

    [Test]
    public async Task UpdateUser_NonExistingUser_ReturnsNotFound()
    {
        var request = new UpdateUserRequest
        {
            FullName = "updateduser",
            Email = "updated@example.com"
        };

        _mockUserService.Setup(x => x.UpdateUserAsync(-1, request, 1))
            .ThrowsAsync(new UserNotFoundException("User not found"));

        var result = await _controller.UpdateUser(-1, request);

        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
        Assert.That(notFoundResult.Value, Is.EqualTo("User not found"));
    }

    [Test]
    public async Task UpdateUser_DuplicateEmail_ReturnsBadRequest()
    {
        var request = new UpdateUserRequest
        {
            FullName = "updateduser",
            Email = "duplicate@example.com"
        };

        _mockUserService.Setup(x => x.UpdateUserAsync(1, request, 1))
            .ThrowsAsync(new DuplicateUserException("Email already exists"));

        var result = await _controller.UpdateUser(1, request);

        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
        Assert.That(badRequestResult.Value, Is.EqualTo("Email already exists"));
    }

    [Test]
    public async Task UpdateUser_ServiceThrowsException_ReturnsInternalServerError()
    {
        var request = new UpdateUserRequest
        {
            FullName = "updateduser",
            Email = "updated@example.com"
        };

        _mockUserService.Setup(x => x.UpdateUserAsync(1, request, 1))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.UpdateUser(1, request);

        var statusCodeResult = result.Result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(500));
        Assert.That(statusCodeResult.Value, Is.EqualTo("An error occurred while updating the user"));
    }

    [Test]
    public async Task DeleteUser_ExistingUser_ReturnsNoContent()
    {
        _mockUserService.Setup(x => x.DeleteUserAsync(1, 1))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeleteUser(1);

        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult, Is.Not.Null);
        Assert.That(noContentResult.StatusCode, Is.EqualTo(204));
    }

    [Test]
    public async Task DeleteUser_NonExistingUser_ReturnsNotFound()
    {
        _mockUserService.Setup(x => x.DeleteUserAsync(-1, 1))
            .ThrowsAsync(new UserNotFoundException("User not found"));

        var result = await _controller.DeleteUser(-1);

        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
        Assert.That(notFoundResult.Value, Is.EqualTo("User not found"));
    }

    [Test]
    public async Task DeleteUser_ServiceThrowsException_ReturnsInternalServerError()
    {
        _mockUserService.Setup(x => x.DeleteUserAsync(1, 1))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.DeleteUser(1);

        var statusCodeResult = result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(500));
        Assert.That(statusCodeResult.Value, Is.EqualTo("An error occurred while deleting the user"));
    }

    [Test]
    public async Task ValidatePassword_ValidPassword_ReturnsOkWithValidResult()
    {
        var request = new ValidatePasswordRequest
        {
            Password = "Password123!"
        };

        var expectedResponse = new ValidatePasswordResponse
        {
            IsValid = true
        };

        _mockUserService.Setup(x => x.ValidatePasswordAsync(1, request.Password, 1))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.ValidatePassword(1, request);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        var response = okResult.Value as ValidatePasswordResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsValid, Is.True);
    }

    [Test]
    public async Task ValidatePassword_InvalidPassword_ReturnsOkWithInvalidResult()
    {
        var request = new ValidatePasswordRequest
        {
            Password = "WrongPassword"
        };

        var expectedResponse = new ValidatePasswordResponse
        {
            IsValid = false
        };

        _mockUserService.Setup(x => x.ValidatePasswordAsync(1, request.Password, 1))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.ValidatePassword(1, request);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        var response = okResult.Value as ValidatePasswordResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsValid, Is.False);
    }

    [Test]
    public async Task ValidatePassword_NonExistingUser_ReturnsNotFound()
    {
        var request = new ValidatePasswordRequest
        {
            Password = "Password123!"
        };

        _mockUserService.Setup(x => x.ValidatePasswordAsync(-1, request.Password, 1))
            .ThrowsAsync(new UserNotFoundException("User not found"));

        var result = await _controller.ValidatePassword(-1, request);

        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
        Assert.That(notFoundResult.Value, Is.EqualTo("User not found"));
    }

    [Test]
    public async Task ValidatePassword_ServiceThrowsException_ReturnsInternalServerError()
    {
        var request = new ValidatePasswordRequest
        {
            Password = "Password123!"
        };

        _mockUserService.Setup(x => x.ValidatePasswordAsync(1, request.Password, 1))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.ValidatePassword(1, request);

        var statusCodeResult = result.Result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(500));
        Assert.That(statusCodeResult.Value, Is.EqualTo("An error occurred while validating the password"));
    }
}
