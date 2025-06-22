# CLAUDE.md Workflow Guide

## Overview

The CLAUDE.md workflow is ContextKeeper's flagship profile, implementing an LSM-tree inspired history management system specifically designed for AI-assisted development.

## What is CLAUDE.md?

CLAUDE.md is a living project documentation file that serves as the primary context source for AI assistants. It contains:

- Complete project overview and architecture
- Current implementation status
- Technology stack and design patterns
- Development guidelines
- Recent updates and milestones

## Workflow Philosophy

### Write-Ahead Log Pattern
Every significant update to CLAUDE.md is backed up before modification, ensuring you can always recover previous states.

### Immutable History
Snapshots are never modified after creation. This provides a reliable audit trail of your project's evolution.

### Intelligent Compaction
When 10+ snapshots accumulate, they're compacted into quarterly archives while preserving the latest state and key milestones.

## Directory Structure

```
project-root/
├── CLAUDE.md                         # Main project documentation
├── FeatureData/
│   └── DataHistory/
│       ├── CLAUDE/                   # Individual snapshots
│       │   ├── CLAUDE_2024-01-15_initial-setup.md
│       │   ├── CLAUDE_2024-01-20_database-integration.md
│       │   └── CLAUDE_2024-01-25_testing-complete.md
│       ├── Compacted/                # Compacted archives
│       │   └── Archived_2024-Q1/
│       └── HISTORY_MANAGEMENT_WORKFLOW.md
```

## Step-by-Step Workflow

### 1. Initial Setup

```bash
# Initialize ContextKeeper (auto-detects CLAUDE.md)
contextkeeper init

# Creates the directory structure:
# FeatureData/DataHistory/CLAUDE/
```

### 2. Creating Snapshots

Before making significant changes to CLAUDE.md:

```bash
# Create a snapshot with descriptive milestone
contextkeeper snapshot feature-authentication-complete

# Milestone naming conventions:
# - Use kebab-case (lowercase with hyphens)
# - Be descriptive but concise
# - Examples:
#   - initial-setup
#   - database-integration
#   - api-endpoints-complete
#   - testing-framework-added
#   - performance-optimization
```

### 3. Snapshot Format

Each snapshot includes a structured header:

```markdown
# CLAUDE.md Historical Snapshot
**Date**: 2024-01-25
**Milestone**: testing framework added
**Previous State**: CLAUDE_2024-01-20_api-endpoints-complete.md
**Compaction Status**: Individual Snapshot

## Changes in This Version
- [To be filled by developer]

## Context for Future Reference
- [To be filled by developer]

---
[Original CLAUDE.md content follows]
```

### 4. Filling Change Information

After creating a snapshot, edit it to document:

**Changes in This Version:**
- Specific features added
- Refactoring performed
- Dependencies updated
- Configuration changes

**Context for Future Reference:**
- Why these changes were made
- Design decisions
- Trade-offs considered
- Links to issues/PRs

### 5. Monitoring Compaction

```bash
# Check if compaction is needed
contextkeeper check

# Output:
{
  "snapshotCount": 11,
  "compactionNeeded": true,
  "recommendedAction": "Compaction recommended - 11/10 snapshots exist"
}
```

### 6. Compaction Process

When 10+ snapshots exist, compaction preserves history efficiently:

1. **Quarterly Archives** - Snapshots grouped by quarter
2. **Summary Generation** - Key changes extracted
3. **Latest State** - Most recent snapshot preserved in full
4. **Cleanup** - Original snapshots moved to archive

## Best Practices

### 1. Snapshot Timing

Create snapshots:
- Before major feature implementations
- After completing significant milestones
- Before refactoring or breaking changes
- After resolving critical bugs
- Before extended breaks in development

### 2. Milestone Naming

Good milestone names:
- ✅ `user-authentication-complete`
- ✅ `database-migration-postgres`
- ✅ `api-v2-endpoints-added`
- ✅ `performance-caching-layer`

Poor milestone names:
- ❌ `update` (too vague)
- ❌ `fixed stuff` (not kebab-case)
- ❌ `MAJOR_CHANGES` (wrong format)
- ❌ `added-new-feature-for-user-management-with-roles` (too long)

### 3. Documentation Quality

In the "Changes" section:
```markdown
## Changes in This Version
- Added user authentication with JWT tokens
- Implemented role-based access control (Admin, User, Guest)
- Created middleware for request validation
- Added comprehensive test suite for auth endpoints
```

In the "Context" section:
```markdown
## Context for Future Reference
- Chose JWT over sessions for stateless architecture
- RBAC implementation allows future permission granularity
- Middleware pattern enables easy extension
- Test coverage at 95% for critical paths
```

### 4. AI Collaboration

When working with AI assistants:

```bash
# Before starting work
contextkeeper search "authentication"
contextkeeper evolution "UserService"

# AI can then understand:
# - Previous implementation attempts
# - Design decisions made
# - Current state of components
```

## Advanced Usage

### Searching History

```bash
# Find when PostgreSQL was added
contextkeeper search "postgresql"

# Search for specific patterns
contextkeeper search "CREATE TABLE"
```

### Tracking Evolution

```bash
# See how the API evolved
contextkeeper evolution "API"

# Track specific service development
contextkeeper evolution "AuthenticationService"
```

### Comparing Versions

```bash
# Compare two specific snapshots
contextkeeper compare \
  CLAUDE_2024-01-15_initial-setup.md \
  CLAUDE_2024-01-25_testing-complete.md
```

## Integration with AI Assistants

### Setup for Claude

```bash
# Add to Claude MCP
claude mcp add contextkeeper -- ~/.contextkeeper/contextkeeper
```

### AI Prompts

Effective prompts for AI assistants:

- "Check the history for how we implemented caching"
- "Create a snapshot for the completed OAuth integration"
- "Show me the evolution of the database schema"
- "When did we add Redis to the stack?"

## Troubleshooting

### Missing CLAUDE.md

If CLAUDE.md doesn't exist:
1. Create it with project overview
2. Run `contextkeeper init`
3. Create initial snapshot

### Large CLAUDE.md Files

For files over 100KB:
- Consider splitting into sections
- Use more frequent compaction
- Archive old implementation details

### Recovery from Errors

```bash
# List all snapshots
ls FeatureData/DataHistory/CLAUDE/

# Manually restore if needed
cp FeatureData/DataHistory/CLAUDE/CLAUDE_2024-01-20_*.md CLAUDE.md
```

## Migration from Manual Process

If you've been manually managing snapshots:

1. Move existing snapshots to `FeatureData/DataHistory/CLAUDE/`
2. Rename to match pattern: `CLAUDE_YYYY-MM-DD_description.md`
3. Run `contextkeeper check` to verify
4. Continue with ContextKeeper commands

## Benefits

### For Developers
- Never lose important context
- Quick recovery of previous states
- Clear progression of features
- Reduced onboarding time

### For AI Assistants
- Complete project history access
- Understanding of design evolution
- Context for better suggestions
- Awareness of past decisions

### For Teams
- Shared understanding of changes
- Audit trail of development
- Knowledge preservation
- Reduced bus factor

## Future Enhancements

Planned improvements for CLAUDE.md workflow:
- Automatic change detection
- Git commit correlation
- Visual timeline generation
- AI-powered summary creation