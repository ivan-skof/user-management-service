using Microsoft.EntityFrameworkCore;
using UserManagementService.Data.Context;

namespace UserManagementService.Api.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string API_KEY_HEADER = "X-API-Key";

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        // Skip authentication for Swagger endpoints
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "API Key is missing" });
            return;
        }


        var keys = await dbContext.ApiClients.Select(x => x.ApiKey).ToListAsync();
        

        var apiClient = await dbContext.ApiClients
            .FirstOrDefaultAsync(c => c.ApiKey == extractedApiKey.ToString() && c.IsActive);

        if (apiClient == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API Key" });
            return;
        }

        // Store client information in HttpContext for logging
        context.Items["ClientName"] = apiClient.ClientName;
        context.Items["ClientId"] = apiClient.Id;

        await _next(context);
    }
}