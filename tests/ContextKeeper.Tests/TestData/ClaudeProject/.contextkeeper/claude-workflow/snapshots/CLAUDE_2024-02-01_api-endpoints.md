# CLAUDE.md Historical Snapshot
**Date**: 2024-02-01
**Milestone**: api-endpoints
**Previous State**: CLAUDE_2024-01-25_database-integration.md
**Compaction Status**: Active

## Changes in This Version
- Implemented all CRUD endpoints for Tasks and Projects
- Added pagination, filtering, and sorting
- Implemented request validation
- Added Swagger/OpenAPI documentation

## Context for Future Reference
- Using MediatR for CQRS pattern
- FluentValidation for request validation
- AutoMapper for DTO mapping
- Swagger UI available at /swagger

---
# TaskManager API - Development Guide

## Project Overview
TaskManager API is a complete RESTful service with full CRUD operations, pagination, and comprehensive documentation. This document tracks architectural decisions and implementation details for AI-assisted development.

## Technology Stack
- **Framework**: .NET 8 Web API
- **Database**: PostgreSQL 15 ✓
- **ORM**: Entity Framework Core 8 ✓
- **Authentication**: JWT Bearer ✓
- **Architecture**: Clean Architecture + CQRS
- **Documentation**: Swagger/OpenAPI ✓

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `POST /api/auth/refresh` - Refresh token
- `POST /api/auth/logout` - Logout user

### Projects
- `GET /api/projects` - Get all projects (paginated)
- `GET /api/projects/{id}` - Get project by ID
- `POST /api/projects` - Create new project
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project
- `GET /api/projects/{id}/tasks` - Get project tasks

### Tasks
- `GET /api/tasks` - Get all tasks (paginated)
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task
- `PATCH /api/tasks/{id}/status` - Update task status
- `PATCH /api/tasks/{id}/assign` - Assign task to user

## Request/Response Examples

### Create Project Request
```json
POST /api/projects
{
  "name": "Q1 Marketing Campaign",
  "description": "Digital marketing initiatives for Q1 2024",
  "startDate": "2024-01-01",
  "endDate": "2024-03-31"
}
```

### Get Tasks Response (Paginated)
```json
GET /api/tasks?page=1&pageSize=10&status=InProgress&sortBy=dueDate
{
  "items": [
    {
      "id": 1,
      "title": "Design landing page",
      "description": "Create responsive landing page design",
      "status": "InProgress",
      "dueDate": "2024-02-15",
      "project": {
        "id": 5,
        "name": "Q1 Marketing Campaign"
      },
      "assignedTo": {
        "id": "user123",
        "name": "John Doe"
      }
    }
  ],
  "totalCount": 45,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

## CQRS Implementation

### Commands
- `CreateProjectCommand`
- `UpdateProjectCommand`
- `DeleteProjectCommand`
- `CreateTaskCommand`
- `UpdateTaskCommand`
- `AssignTaskCommand`

### Queries
- `GetProjectsQuery` (with pagination)
- `GetProjectByIdQuery`
- `GetTasksQuery` (with filtering)
- `GetTaskByIdQuery`

## Validation Rules

### Project Validation
- Name: Required, 3-100 characters
- Description: Optional, max 500 characters
- EndDate: Must be after StartDate

### Task Validation
- Title: Required, 3-200 characters
- Description: Optional, max 1000 characters
- DueDate: Must be future date
- ProjectId: Must exist
- AssignedToId: Must be valid user

## Current Status
- [x] Initial project setup
- [x] Database configuration
- [x] Authentication system
- [x] Core API endpoints
- [x] Repository pattern
- [x] Database migrations
- [x] Request validation
- [x] API documentation
- [x] Pagination & filtering

## Performance Features
1. **Response Caching**: 5-minute cache for GET requests
2. **Rate Limiting**: 100 requests per minute per IP
3. **Compression**: Gzip compression enabled
4. **Async Operations**: All endpoints are async
5. **Query Optimization**: Includes for related data

## Security Features
- JWT Bearer authentication required
- Role-based authorization (Admin, User)
- CORS configured for specific origins
- SQL injection protection via EF Core
- XSS protection headers

## Testing
- Unit tests: 85% code coverage
- Integration tests for all endpoints
- Load testing: 1000 concurrent users
- Postman collection available

## Deployment Ready
- Docker support with multi-stage build
- Health checks endpoint
- Structured logging with Serilog
- Application Insights integration
- Environment-specific configurations

## Next Steps
1. Add real-time notifications (SignalR)
2. Implement file attachments for tasks
3. Add reporting endpoints
4. Create admin dashboard