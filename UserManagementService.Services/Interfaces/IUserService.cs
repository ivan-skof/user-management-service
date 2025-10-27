using UserManagementService.Core.DTOs;

namespace UserManagementService.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> GetUserAsync(int id, int clientId);
        Task<List<UserResponse>> GetAllUsersAsync(int clientId); //for easier manual testing
        Task<UserResponse> CreateUserAsync(CreateUserRequest request, int clientId);
        Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request, int clientId);
        Task DeleteUserAsync(int id, int clientId);
        Task<ValidatePasswordResponse> ValidatePasswordAsync(int id, string password, int clientId);
    }
}