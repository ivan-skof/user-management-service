namespace UserManagementService.Services.Interfaces
{
    public interface ILogService
    {
        Task LogAsync(string level, string methodName, string requestParams, string message, string clientIp, string clientName);
    }
}