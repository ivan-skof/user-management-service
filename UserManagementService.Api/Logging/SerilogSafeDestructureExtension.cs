using Serilog;
using UserManagementService.Core.DTOs;

namespace UserManagementService.Api.Logging;

public static class SerilogSafeDestructureExtensions
{
    public static LoggerConfiguration DestructureSensitiveDtos(this LoggerConfiguration config)
    {
        return config
            .Destructure.ByTransforming<CreateUserRequest>(r => new
            {
                r.UserName,
                r.FullName,
                r.Email,
                r.MobileNumber,
                r.Language,
                r.Culture,
                Password = "***"
            })
            .Destructure.ByTransforming<ValidatePasswordRequest>(r => new
            {
                Password = "***"
            });
    }
}
