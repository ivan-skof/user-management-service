using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using UserManagementService.Api.ExceptionHandlers;
using UserManagementService.Api.Logging;
using UserManagementService.Api.Middleware;
using UserManagementService.Core.DTOs;
using UserManagementService.Data.Context;
using UserManagementService.Data.Entities;
using UserManagementService.Services.Implementations;
using UserManagementService.Services.Interfaces;

Log.Logger = new LoggerConfiguration()
    .Enrich.WithMachineName()
    .Enrich.FromLogContext()
    .DestructureSensitiveDtos()
    .MinimumLevel.Information()
    // Console: log everything
    .WriteTo.Console()
    // File: only app logs
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly("SourceContext like '%UserManagementService%'")
        .WriteTo.File(
            path: "Logs/serilog_.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} Level={Level} Host={MachineName} ClientIp={ClientIp} ClientName={ClientName} MethodName={MethodName}] {Message}{NewLine}{Exception}"
        ))
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.Filters.Add<LogEnrichmentActionFilter>();
});
builder.Services.AddEndpointsApiExplorer();


// Configure Swagger with API Key authentication
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "User Management API",
        Version = "v1",
        Description = "API for managing users with password validation"
    });

    // Define the API Key security scheme
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Description = "Enter your API Key in the text input below. (seeded api keys: dev1, dev2)"
    });

    // Make API Key required for all endpoints
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] { }
        }
    });
});
// Database configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register custom services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Custom middleware for API key authentication
app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseExceptionHandler();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); 

    // Seed two dev API clients
    if (!db.ApiClients.Any(c => c.ClientName == "devApiClient1"))
    {
        db.ApiClients.Add(new ApiClient
        {
            ClientName = "devApiClient1",
            ApiKey = "dev1",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        db.ApiClients.Add(new ApiClient
        {
            ClientName = "devApiClient2",
            ApiKey = "dev2",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
        Console.WriteLine("Seeded devApiClients");
    }
}

app.Run();