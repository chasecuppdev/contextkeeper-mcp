# README.md Documentation Snapshot
**Date**: 2024-02-15
**Milestone**: documentation-update
**Previous State**: None (Initial)

## Summary of Changes
- Added comprehensive API documentation
- Updated installation instructions
- Added Docker deployment guide
- Included troubleshooting section

---
# TaskManager API

A modern RESTful API for task and project management built with .NET 8.

## Features

- 🔐 JWT Authentication with refresh tokens
- 📋 Full CRUD operations for tasks and projects
- 🔍 Advanced filtering and pagination
- 📚 Swagger/OpenAPI documentation
- 🏗️ Clean Architecture pattern
- 🗄️ PostgreSQL database with EF Core
- 🚀 Docker ready

## Quick Start

### Prerequisites
- .NET 8 SDK
- PostgreSQL 15+
- Docker (optional)

### Installation

1. Clone the repository
```bash
git clone https://github.com/example/taskmanager-api.git
cd taskmanager-api
```

2. Set up the database
```bash
# Update connection string in appsettings.json
# Run migrations
dotnet ef database update -p src/TaskManager.Infrastructure -s src/TaskManager.API
```

3. Run the application
```bash
dotnet run --project src/TaskManager.API
```

The API will be available at `https://localhost:5001`

### Docker Deployment

```bash
# Build the image
docker build -t taskmanager-api .

# Run the container
docker run -d -p 5001:80 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Database=TaskManagerDb;Username=postgres;Password=yourpassword" \
  taskmanager-api
```

## API Documentation

Swagger UI is available at `/swagger` when running in Development mode.

### Authentication

All endpoints except `/api/auth/register` and `/api/auth/login` require authentication.

Include the JWT token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

### Example Requests

#### Register User
```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!",
    "name": "John Doe"
  }'
```

#### Create Task
```bash
curl -X POST https://localhost:5001/api/tasks \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Complete documentation",
    "description": "Write comprehensive API docs",
    "projectId": 1,
    "dueDate": "2024-03-01"
  }'
```

## Architecture

The project follows Clean Architecture principles:

```
├── TaskManager.API         # Presentation Layer
├── TaskManager.Application # Business Logic
├── TaskManager.Domain      # Domain Models
└── TaskManager.Infrastructure # Data Access
```

## Configuration

### Environment Variables

- `ASPNETCORE_ENVIRONMENT` - Development/Staging/Production
- `ConnectionStrings__DefaultConnection` - Database connection
- `JwtSettings__Secret` - JWT signing key

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=TaskManagerDb;Username=postgres;Password=password"
  },
  "JwtSettings": {
    "Secret": "your-256-bit-secret",
    "Issuer": "TaskManagerAPI",
    "Audience": "TaskManagerClient",
    "AccessTokenExpiration": 15,
    "RefreshTokenExpiration": 10080
  }
}
```

## Testing

```bash
# Run unit tests
dotnet test tests/TaskManager.Tests

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## Troubleshooting

### Database Connection Issues
1. Ensure PostgreSQL is running
2. Check connection string format
3. Verify database exists
4. Check firewall settings

### Authentication Failures
1. Ensure token hasn't expired
2. Check token format in header
3. Verify JWT secret matches

### Migration Errors
1. Ensure database user has CREATE permissions
2. Check for pending migrations
3. Verify EF Core tools are installed

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.