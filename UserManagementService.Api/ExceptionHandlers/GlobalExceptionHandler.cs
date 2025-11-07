using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UserManagementService.Services.Exceptions;

namespace UserManagementService.Api.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {

        _logger.LogError(exception,
            "Exception occurred while processing {RequestPath}: {Message}",
            httpContext.Request.Path, exception.Message);

        var problem = exception switch
        {
            UserNotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "User not found",
                Detail = exception.Message
            },
            DuplicateUserException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Duplicate user",
                Detail = exception.Message
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server error",
                Detail = "An unexpected error occurred."
            }
        };

        // 🔹 Write response
        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";



        await httpContext.Response
            .WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
