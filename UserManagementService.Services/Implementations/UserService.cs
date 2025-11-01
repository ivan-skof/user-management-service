using Microsoft.EntityFrameworkCore;
using UserManagementService.Core.DTOs;
using UserManagementService.Data.Context;
using UserManagementService.Data.Entities;
using UserManagementService.Services.Exceptions;
using UserManagementService.Services.Interfaces;

namespace UserManagementService.Services.Implementations;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    public UserService(AppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponse> GetUserAsync(int id, int clientId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.ApiClientId == clientId);

        if (user == null)
            throw new UserNotFoundException($"User with ID {id} not found");

        return MapToDTO(user);
    }

    public async Task<List<UserResponse>> GetAllUsersAsync(int clientId)
    {
        var users = await _context.Users
            .Where(u => u.ApiClientId == clientId)
            .ToListAsync();

        return users.Select(MapToDTO).ToList();
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, int clientId)
    {            
        if (await _context.Users.AnyAsync(u => u.UserName == request.UserName && u.ApiClientId == clientId))
            throw new DuplicateUserException("Username already exists");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.ApiClientId == clientId))
            throw new DuplicateUserException("Email already exists");

        var (hash, salt) = _passwordHasher.HashPassword(request.Password);


        var user = new User
        {
            UserName = request.UserName,
            FullName = request.FullName,
            Email = request.Email,
            MobileNumber = request.MobileNumber,
            Language = request.Language,
            Culture = request.Culture,
            ApiClientId = clientId,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return MapToDTO(user);
    }

    public async Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request, int clientId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.ApiClientId == clientId);

        if (user == null)
            throw new UserNotFoundException($"User with ID {id} not found");

        //Update only provided fields
        if (!string.IsNullOrEmpty(request.FullName))
            user.FullName = request.FullName;

        if (!string.IsNullOrEmpty(request.Email))
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != id && u.ApiClientId == clientId))
                throw new DuplicateUserException("Email already exists");

            user.Email = request.Email;
        }

        if (!string.IsNullOrEmpty(request.MobileNumber))
            user.MobileNumber = request.MobileNumber;

        if (!string.IsNullOrEmpty(request.Language))
            user.Language = request.Language;

        if (!string.IsNullOrEmpty(request.Culture))
            user.Culture = request.Culture;

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDTO(user);
    }

    public async Task DeleteUserAsync(int id, int clientId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.ApiClientId == clientId);

        if (user == null)
            throw new UserNotFoundException($"User with ID {id} not found");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task<ValidatePasswordResponse> ValidatePasswordAsync(int id, string password, int clientId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.ApiClientId == clientId);

        if (user == null)
            throw new UserNotFoundException($"User with ID {id} not found");

        var isValid = _passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);

        return new ValidatePasswordResponse { IsValid = isValid };
    }

    private UserResponse MapToDTO(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            FullName = user.FullName,
            Email = user.Email,
            MobileNumber = user.MobileNumber,
            Language = user.Language,
            Culture = user.Culture,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}