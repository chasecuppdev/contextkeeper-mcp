# ContextKeeper User Workspace

Welcome to your ContextKeeper workspace! This is your flexible area for maintaining custom documentation that enhances AI assistance.

## Why Use This Workspace?

- **Visibility**: Unlike dot-prefixed folders, this shows up in Claude's @ symbol file browser
- **Organization**: Pre-structured directories for common documentation types
- **Integration**: All files here are automatically included in ContextKeeper snapshots
- **Flexibility**: Add any files or create new subdirectories as needed

## Directory Purpose

### 📋 requirements/
Store project requirements, specifications, and acceptance criteria:
- API specifications
- User stories
- Feature requirements
- Business rules
- Compliance requirements

### 🎨 design/
Document design decisions and architectural choices:
- Architecture Decision Records (ADRs)
- Design patterns being used
- System diagrams (as .md with ASCII art or mermaid)
- Component relationships
- Technology choices and rationale

### 🤖 instructions/
Custom instructions for AI assistants:
- Coding style preferences
- Project-specific guidelines
- Common patterns to follow
- Things to avoid
- Context about the current task

## Best Practices

### 1. Use Descriptive Filenames
```
✅ api-v2-authentication-spec.md
❌ spec.md
```

### 2. Include Dates for Temporal Context
```
✅ 2025-06-25-performance-requirements.md
✅ design-decisions-2025-Q2.md
```

### 3. Use Markdown Headers for Structure
```markdown
# Authentication API Requirements
## Overview
## Endpoints
### POST /auth/login
```

### 4. Link Between Documents
```markdown
See [API Design](../design/api-architecture.md) for implementation details.
```

## Example Files

### requirements/example-api-spec.md
```markdown
# User Management API v2.0

## Overview
RESTful API for user management with JWT authentication.

## Endpoints

### GET /api/users
Returns paginated list of users...
```

### design/example-adr.md
```markdown
# ADR-001: Use JWT for Authentication

## Status
Accepted

## Context
We need a stateless authentication mechanism...

## Decision
We will use JWT tokens with 15-minute expiry...
```

### instructions/example-coding-style.md
```markdown
# Project Coding Guidelines

## For this session:
- Use async/await consistently
- Prefer LINQ over loops where readable
- Add XML documentation to public methods
- Follow existing patterns in UserService.cs
```

## Git Integration

By default, this workspace is:
- ✅ Tracked by git (for team sharing)
- ✅ Included in ContextKeeper snapshots

To ignore specific files, add them to `.gitignore`:
```
context-workspace/workspace/temp-notes.md
context-workspace/workspace/personal/
```

## Tips for AI Assistance

1. **Reference files directly**: Use `@context-workspace/workspace/requirements/feature-x.md` in Claude
2. **Update frequently**: Keep instructions current with project changes
3. **Be specific**: More detail helps AI provide better assistance
4. **Clean up**: Remove outdated files to avoid confusion

## Questions?

This workspace is designed to enhance your AI-assisted development workflow. Feel free to organize it however works best for your project!