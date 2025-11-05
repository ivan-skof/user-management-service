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
        using (LogContext.PushProperty("BodyParameters", request, true))
        {
            try
            {
                var user = await _userService.CreateUserAsync(request, GetClientId());

                _logger.LogInformation("User created successfully with ID: {@UserId}", user.Id);

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (DuplicateUserException ex)
            {
                _logger.LogError("{@ExMessage}", ex.Message);

                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating user: {@ExMessage}", ex.Message);

                return StatusCode(500, "An error occurred while creating the user");
            }
        }
    }

    // GET: api/users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUser(int id)
    {
        using (LogContext.PushProperty("Parameters", id))
        {
            try
            {
                var user = await _userService.GetUserAsync(id, GetClientId());

                _logger.LogInformation("User retrieved successfully: {@userName}", user.UserName);

                return Ok(user);
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogInformation("User with ID:{@id} not found.", id);

                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving user: {@exMessage}", ex.Message);

                return StatusCode(500, "An error occurred while retrieving the user");
            }
        }
    }

    // GET: api/users/
    [HttpGet]
    public async Task<ActionResult<UserResponse>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync(GetClientId());

            _logger.LogInformation("Retrieved {@usersCount} users.", users.Count);

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving users: {@exMessage}", ex.Message);

            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    // PUT: api/users/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        using (LogContext.PushProperty("Parameters", id))
        using (LogContext.PushProperty("BodyParameters", request, true))
        {
            try
            {
                var user = await _userService.UpdateUserAsync(id, request, GetClientId());

                _logger.LogInformation("User updated successfully");

                return Ok(user);
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogInformation("User with ID:{@Id} not found.", id);

                return NotFound(ex.Message);
            }
            catch (DuplicateUserException ex)
            {
                _logger.LogError("{@ExMessage}", ex.Message);

                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating user: {@ExMessage}", ex.Message);

                return StatusCode(500, "An error occurred while updating the user");
            }
        }
    }

    // DELETE: api/users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        using (LogContext.PushProperty("Parameters", id))
        {
            try
            {
                await _userService.DeleteUserAsync(id, GetClientId());

                _logger.LogInformation("User deleted successfully");

                return NoContent();
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogInformation("User with ID:{@Id} not found.", id);

                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting user: {@ExMessage}", ex.Message);

                return StatusCode(500, "An error occurred while deleting the user");
            }
        }
    }


    // POST: api/users/{id}/validate-password
    [HttpPost("{id}/validate-password")]
    public async Task<ActionResult<ValidatePasswordResponse>> ValidatePassword(int id, [FromBody] ValidatePasswordRequest request)
    {
        using (LogContext.PushProperty("Parameters", id))
        using (LogContext.PushProperty("BodyParameters", request, true))
        {
            try
            {
                var result = await _userService.ValidatePasswordAsync(id, request.Password, GetClientId());
                _logger.LogInformation("Password validation completed: {@IsValid}", (result.IsValid ? "Valid" : "Invalid"));

                return Ok(result);
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogInformation("User with ID:{@Id} not found.", id);

                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error validating password: {@ExMessage}", ex.Message);

                return StatusCode(500, "An error occurred while validating the password");
            }
        }
    }
}