using Microsoft.AspNetCore.Mvc;
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
    private string GetClientIp()
    {
        return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetClientName()
    {
        return _httpContextAccessor.HttpContext?.Items["ClientName"]?.ToString() ?? "Unknown";
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        var methodName = nameof(CreateUser);
        var requestParams = JsonSerializer.Serialize(new
        {
            request.UserName,
            request.FullName,
            request.Email,
            request.MobileNumber,
            request.Language,
            request.Culture
            // Password excluded
        });

        var logMetadata = $"ClientIp: {GetClientIp()} | ClientName: {GetClientName()} | Method: {methodName} | RequestParams: {requestParams}";
        
        try
        {
            var user = await _userService.CreateUserAsync(request, GetClientId());

            _logger.LogInformation("{logMetadata} | User created successfully with ID: {UserId}", logMetadata, user.Id);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (DuplicateUserException ex)
        {
            _logger.LogError("{logMetadata} | {exMessage}", logMetadata, ex.Message);

            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("{logMetadata} | Error creating user: {exMessage}", logMetadata, ex.Message);

            return StatusCode(500, "An error occurred while creating the user");
        }
    }

    // GET: api/users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUser(int id)
    {
        var methodName = nameof(GetUser);
        var requestParams = JsonSerializer.Serialize(new { id });
        var logMetadata = $"ClientIp: {GetClientIp()} | ClientName: {GetClientName()} | Method: {methodName} | RequestParams: {requestParams}";

        try
        {
            var user = await _userService.GetUserAsync(id, GetClientId());

            _logger.LogInformation("{logMetadata} | User retrieved successfully: {userName}", logMetadata, user.UserName);

            return Ok(user);
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogInformation("{logMetadata} | User with ID:{id} not found.", logMetadata, id);

            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("{logMetadata} | Error retrieving user: {exMessage}", logMetadata, ex.Message);

            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }

    // GET: api/users/
    [HttpGet]
    public async Task<ActionResult<UserResponse>> GetAllUsers()
    {
        var methodName = nameof(GetAllUsers);
        var logMetadata = $"ClientIp: {GetClientIp()} | ClientName: {GetClientName()} | Method: {methodName} | RequestParams: ";

        try
        {
            var users = await _userService.GetAllUsersAsync(GetClientId());

            _logger.LogInformation("{logMetadata} | Retrieved {usersCount} users.", logMetadata, users.Count);

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError("{logMetadata} | Error retrieving users: {exMessage}", logMetadata, ex.Message);

            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    // PUT: api/users/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var methodName = nameof(UpdateUser);
        var requestParams = JsonSerializer.Serialize(new { id, request });
        var logMetadata = $"ClientIp: {GetClientIp()} | ClientName: {GetClientName()} | Method: {methodName} | RequestParams: {requestParams}";

        try
        {
            var user = await _userService.UpdateUserAsync(id, request, GetClientId());

            _logger.LogInformation("{logMetadata} | User updated successfully", logMetadata);

            return Ok(user);
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogInformation("{logMetadata} | User with ID:{id} not found.", logMetadata, id);

            return NotFound(ex.Message);
        }
        catch (DuplicateUserException ex)
        {
            _logger.LogError("{logMetadata} | {exMessage}", logMetadata, ex.Message);

            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("{logMetadata} | Error updating user: {exMessage}", logMetadata, ex.Message);

            return StatusCode(500, "An error occurred while updating the user");
        }
    }

    // DELETE: api/users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var methodName = nameof(DeleteUser);
        var requestParams = JsonSerializer.Serialize(new { id });
        var logMetadata = $"ClientIp: {GetClientIp()} | ClientName: {GetClientName()} | Method: {methodName} | RequestParams: {requestParams}";

        try
        {
            await _userService.DeleteUserAsync(id, GetClientId());

            _logger.LogInformation("{logMetadata} | User deleted successfully", logMetadata);

            return NoContent();
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogInformation("{logMetadata} | User with ID:{id} not found.", logMetadata, id);

            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("{logMetadata} | Error deleting user: {exMessage}", logMetadata, ex.Message);

            return StatusCode(500, "An error occurred while deleting the user");
        }
    }


    // POST: api/users/{id}/validate-password
    [HttpPost("{id}/validate-password")]
    public async Task<ActionResult<ValidatePasswordResponse>> ValidatePassword(int id, [FromBody] ValidatePasswordRequest request)
    {
        var methodName = nameof(ValidatePassword);
        var requestParams = JsonSerializer.Serialize(new { id });
        var logMetadata = $"ClientIp: {GetClientIp()} | ClientName: {GetClientName()} | Method: {methodName} | RequestParams: {requestParams}";

        try
        {
            var result = await _userService.ValidatePasswordAsync(id, request.Password, GetClientId());

            _logger.LogInformation("{logMetadata} | Password validation completed: {isValid}", logMetadata, (result.IsValid ? "Valid" : "Invalid"));

            return Ok(result);
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogInformation("{logMetadata} | User with ID:{id} not found.", logMetadata, id);

            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("{logMetadata} | Error validating password: {exMessage}", logMetadata, ex.Message);

            return StatusCode(500,"An error occurred while validating the password");
        }
    }
}