# CLAUDE.md Historical Snapshot
**Date**: 2024-01-25
**Milestone**: database-integration
**Previous State**: CLAUDE_2024-01-20_add-authentication.md
**Compaction Status**: Active

## Changes in This Version
- Integrated PostgreSQL with Entity Framework Core
- Created database migrations
- Implemented repository pattern
- Added connection pooling and retry logic

## Context for Future Reference
- Using Code-First approach with EF Core
- Implemented generic repository pattern
- Added database health checks
- Connection string in user secrets for dev

---
# TaskManager API - Development Guide

## Project Overview
TaskManager API is a fully functional RESTful service with PostgreSQL integration for managing tasks and projects. This document tracks architectural decisions and implementation details for AI-assisted development.

## Technology Stack
- **Framework**: .NET 8 Web API
- **Database**: PostgreSQL 15 ✓
- **ORM**: Entity Framework Core 8
- **Authentication**: JWT Bearer ✓
- **Architecture**: Clean Architecture

## Project Structure
```
TaskManager/
├── src/
│   ├── TaskManager.API/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   └── Program.cs              # Added EF Core setup
│   ├── TaskManager.Application/
│   │   ├── Services/
│   │   └── Interfaces/
│   │       └── IRepository.cs       # NEW
│   ├── TaskManager.Domain/
│   │   └── Entities/
│   │       ├── User.cs
│   │       ├── Task.cs              # NEW
│   │       ├── Project.cs           # NEW
│   │       └── BaseEntity.cs        # NEW
│   └── TaskManager.Infrastructure/
│       ├── Data/
│       │   ├── TaskManagerContext.cs # NEW
│       │   ├── Migrations/          # NEW
│       │   └── Configurations/      # NEW
│       └── Repositories/
│           ├── Repository.cs        # NEW
│           └── UserRepository.cs    # NEW
└── tests/
    └── TaskManager.Tests/
        └── RepositoryTests.cs       # NEW
```

## Database Schema

### Core Entities
```csharp
public class Task : BaseEntity
{
    public string Title { get; set; }
    public string Description { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime DueDate { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; }
    public string AssignedToId { get; set; }
    public User AssignedTo { get; set; }
}

public class Project : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string OwnerId { get; set; }
    public User Owner { get; set; }
    public ICollection<Task> Tasks { get; set; }
}
```

### Database Configuration
```json
// appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=TaskManagerDb;Username=postgres;Password=****"
},
"DatabaseSettings": {
  "EnableSensitiveDataLogging": false,
  "CommandTimeout": 30,
  "EnableRetryOnFailure": true,
  "MaxRetryCount": 3
}
```

## Repository Pattern Implementation

### Generic Repository
- `IRepository<T>` interface for CRUD operations
- `Repository<T>` base implementation with EF Core
- Specific repositories inherit from base

### Unit of Work
- Manages database transactions
- Ensures data consistency
- Implements dispose pattern

## Current Status
- [x] Initial project setup
- [x] Database configuration
- [x] Authentication system
- [ ] Core API endpoints
- [x] Repository pattern
- [x] Database migrations

## Database Features
- **Connection Pooling**: Min 5, Max 100 connections
- **Retry Logic**: 3 retries with exponential backoff
- **Health Checks**: Database connectivity monitoring
- **Migrations**: Automated with EF Core
- **Audit Fields**: CreatedAt, UpdatedAt, CreatedBy

## Performance Optimizations
1. Async/await throughout data access layer
2. Projection with Select for read operations
3. Compiled queries for frequent operations
4. Proper indexing on foreign keys

## Next Steps
1. Implement task management endpoints
2. Add pagination support
3. Create API documentation
4. Set up integration tests