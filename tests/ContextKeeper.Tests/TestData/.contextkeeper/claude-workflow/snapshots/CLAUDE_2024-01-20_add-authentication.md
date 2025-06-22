# CLAUDE.md Historical Snapshot
**Date**: 2024-01-20
**Milestone**: add-authentication
**Previous State**: CLAUDE_2024-01-15_initial-setup.md
**Compaction Status**: Active

## Changes in This Version
- Implemented JWT authentication system
- Added user registration and login endpoints
- Created authentication middleware
- Set up authorization policies

## Context for Future Reference
- Used Microsoft.AspNetCore.Authentication.JwtBearer
- Storing refresh tokens in database
- 15-minute access token lifetime, 7-day refresh token

---
# TaskManager API - Development Guide

## Project Overview
TaskManager API is a RESTful service for managing tasks and projects with secure authentication. This document tracks architectural decisions and implementation details for AI-assisted development.

## Technology Stack
- **Framework**: .NET 8 Web API
- **Database**: PostgreSQL (planned)
- **Authentication**: JWT Bearer ✓
- **Architecture**: Clean Architecture

## Project Structure
```
TaskManager/
├── src/
│   ├── TaskManager.API/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs    # NEW
│   │   │   └── WeatherController.cs
│   │   └── Middleware/
│   │       └── JwtMiddleware.cs     # NEW
│   ├── TaskManager.Application/
│   │   ├── Services/
│   │   │   ├── IAuthService.cs      # NEW
│   │   │   └── AuthService.cs       # NEW
│   │   └── DTOs/
│   │       ├── LoginRequest.cs      # NEW
│   │       └── RegisterRequest.cs   # NEW
│   ├── TaskManager.Domain/
│   │   └── Entities/
│   │       ├── User.cs              # NEW
│   │       └── RefreshToken.cs      # NEW
│   └── TaskManager.Infrastructure/
│       └── Security/
│           └── JwtTokenGenerator.cs  # NEW
└── tests/
    └── TaskManager.Tests/
        └── AuthServiceTests.cs       # NEW
```

## Authentication Implementation

### JWT Configuration
```csharp
// appsettings.json
"JwtSettings": {
  "Secret": "[Generated in production]",
  "Issuer": "TaskManagerAPI",
  "Audience": "TaskManagerClient",
  "AccessTokenExpiration": 15,  // minutes
  "RefreshTokenExpiration": 10080  // minutes (7 days)
}
```

### Key Components
1. **AuthController**: Handles login, register, refresh token
2. **JwtMiddleware**: Validates tokens on each request
3. **AuthService**: Business logic for authentication
4. **JwtTokenGenerator**: Creates and validates JWT tokens

## Current Status
- [x] Initial project setup
- [ ] Database configuration
- [x] Authentication system
- [ ] Core API endpoints

## API Endpoints
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout (invalidate refresh token)

## Security Considerations
- Passwords hashed with BCrypt
- Refresh tokens stored with expiration
- Rate limiting on auth endpoints (planned)
- HTTPS required in production

## Next Steps
1. Set up PostgreSQL connection
2. Create database migrations
3. Implement task/project models
4. Add role-based authorization