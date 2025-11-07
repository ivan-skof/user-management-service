using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using System.Text.Json;
using UserManagementService.Core.DTOs;
using UserManagementService.Services.Exceptions;
using UserManagementService.Services.Interfaces;

namespace UserManagementService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsersController(IUserService userService, ILogger<UsersController> logger, IHttpContextAccessor httpContextAccessor)
    {
        _userService = userService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    private int GetClientId()
    {
        return int.Parse(_httpContextAccessor.HttpContext?.Items["ClientId"]?.ToString() ?? "0");
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request, GetClientId());

        _logger.LogInformation("User created successfully with ID: {UserId}. Parameters: {@Parameters}", user.Id, request);

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    // GET: api/users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUser(int id)
    {
        var user = await _userService.GetUserAsync(id, GetClientId());

        _logger.LogInformation("User with ID: {UserId} retrieved successfully: {@User}", id,  user);

        return Ok(user);
    }

    // GET: api/users/
    [HttpGet]
    public async Task<ActionResult<UserResponse>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync(GetClientId());

        _logger.LogInformation("Retrieved {UsersCount} users.", users.Count);

        return Ok(users);
    }

    // PUT: api/users/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userService.UpdateUserAsync(id, request, GetClientId());

        _logger.LogInformation("User with ID: {UserId} updated successfully. Parameters: {@Parameters}", id, request);

        return Ok(user);
    }

    // DELETE: api/users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteUserAsync(id, GetClientId());

        _logger.LogInformation("User with ID: {UserId} deleted successfully", id);

        return NoContent();
    }


    // POST: api/users/{id}/validate-password
    [HttpPost("{id}/validate-password")]
    public async Task<ActionResult<ValidatePasswordResponse>> ValidatePassword(int id, [FromBody] ValidatePasswordRequest request)
    {
        var result = await _userService.ValidatePasswordAsync(id, request.Password, GetClientId());
        _logger.LogInformation("Password validation for user with ID: {UserId} completed: {IsValid}", id, (result.IsValid ? "Valid" : "Invalid"));

        return Ok(result);
    }
}