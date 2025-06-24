# CLAUDE.md Historical Snapshot
**Date**: 2024-01-15
**Milestone**: initial-setup
**Previous State**: None (Initial)
**Compaction Status**: Active

## Changes in This Version
- Created initial project structure
- Set up .NET 8 Web API project
- Added basic folder structure
- Configured initial dependencies

## Context for Future Reference
- Decided to use Clean Architecture pattern
- PostgreSQL chosen for data persistence
- Planning JWT authentication

---
# TaskManager API - Development Guide

## Project Overview
TaskManager API is a RESTful service for managing tasks and projects. This document tracks architectural decisions and implementation details for AI-assisted development.

## Technology Stack
- **Framework**: .NET 8 Web API
- **Database**: PostgreSQL (planned)
- **Authentication**: JWT Bearer (planned)
- **Architecture**: Clean Architecture

## Project Structure
```
TaskManager/
├── src/
│   ├── TaskManager.API/          # Web API Layer
│   ├── TaskManager.Application/  # Business Logic
│   ├── TaskManager.Domain/       # Domain Models
│   └── TaskManager.Infrastructure/ # Data Access
└── tests/
    └── TaskManager.Tests/        # Unit Tests
```

## Current Status
- [x] Initial project setup
- [ ] Database configuration
- [ ] Authentication system
- [ ] Core API endpoints

## Development Principles
1. **Test-Driven Development**: Write tests first
2. **Clean Architecture**: Maintain separation of concerns
3. **SOLID Principles**: Keep code maintainable
4. **Documentation**: Keep this file updated

## Next Steps
1. Set up PostgreSQL connection
2. Create domain models (Task, Project, User)
3. Implement repository pattern
4. Add authentication middleware