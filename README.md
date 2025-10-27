# UserManagementService

An ASP.NET Core web service for managing users with API key authentication and logging.

## Features

- User CRUD operations (Create, Read, Update, Delete)
- Password validation
- API key authentication with multi-tenant support
- Daily log files with detailed request tracking
- SQLite database with Entity Framework Core

## Prerequisites

- .NET 9.0 SDK or later
- SQLite (via NuGet package `Microsoft.EntityFrameworkCore.Sqlite`)


## Setup Instructions

### 1. Clone/Extract the Project

Extract or clone the project to your local machine.

### 2. Configure Database Connection

Edit `UserManagementService.Api/appsettings.json` and update the connection string if needed:

```json
"ConnectionStrings": {
    "DefaultConnection": "Data Source=usermanagement.db"
  }
```

### 3. Install Dependencies

```bash
dotnet restore
```

### 4. Create Database

Run Entity Framework migrations to create the database:

```bash
# Navigate to the root project directory
cd user-management-service

# Apply migrations from the solution root
dotnet ef database update   --project UserManagementService.Data   --startup-project UserManagementService.Api
```

If `dotnet ef` is not installed:
```bash
dotnet tool install --global dotnet-ef
```

### 5. API Keys

The service uses API key authentication. API keys are stored in the database in the `ApiKeys` table.

When you run the application for the first time, **two API keys dev1 and dev2 are seeded** into the database. You can use these keys immediately for testing.


## Running the Service

```bash
# From the solution root
dotnet run --project UserManagementService.Api
```

The service will start at `https://localhost:7013` (or `http://localhost:5238`)

## Testing the Service

### Run Unit Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal
```

### Test with Swagger UI

Once the service is running, open your browser and navigate to:
```
https://localhost:7013/swagger 
or 
https://localhost:5238/swagger 
```

Before running any requests click Authorize and enter api key.

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/users` | Get all users |
| GET | `/api/users/{id}` | Get user by ID |
| POST | `/api/users` | Create new user |
| PUT | `/api/users/{id}` | Update user |
| DELETE | `/api/users/{id}` | Delete user |
| POST | `/api/users/{id}/validate-password` | Validate user password |

## Logging

Logs are automatically written to the `Logs` folder in the API project directory.


