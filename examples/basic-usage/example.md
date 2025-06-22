# ContextKeeper Basic Usage Example

This example demonstrates basic ContextKeeper usage in a typical project.

## Setup

```bash
# Initialize ContextKeeper in your project
$ contextkeeper init

{
  "success": true,
  "profile": "claude-workflow",
  "message": "Initialized ContextKeeper with 'claude-workflow' profile",
  "directories": {
    "history": "FeatureData/DataHistory",
    "snapshots": "FeatureData/DataHistory/CLAUDE"
  }
}
```

## Creating Your First Snapshot

```bash
# After initial project setup
$ contextkeeper snapshot initial-project-setup

{
  "success": true,
  "snapshotPath": "FeatureData/DataHistory/CLAUDE/CLAUDE_2024-01-22_initial-project-setup.md",
  "message": "Snapshot created successfully: CLAUDE_2024-01-22_initial-project-setup.md",
  "profile": "claude-workflow"
}
```

## Making Changes and Creating Snapshots

```bash
# After adding authentication
$ contextkeeper snapshot user-authentication-added

# After database integration
$ contextkeeper snapshot postgresql-database-integrated

# After adding tests
$ contextkeeper snapshot comprehensive-testing-suite
```

## Searching History

```bash
# Find when authentication was added
$ contextkeeper search "authentication"

{
  "searchTerm": "authentication",
  "totalMatches": 3,
  "matches": [
    {
      "fileName": "CLAUDE_2024-01-23_user-authentication-added.md",
      "lineNumber": 125,
      "context": "    ### 2. AuthenticationService ✅ COMPLETED\n>>> - Implemented JWT-based authentication\n    - Added role-based access control",
      "matchedLine": "- Implemented JWT-based authentication"
    }
  ],
  "profile": "claude-workflow"
}
```

## Tracking Component Evolution

```bash
# See how the AuthService evolved
$ contextkeeper evolution "AuthService"

{
  "componentName": "AuthService",
  "evolutionSteps": [
    {
      "date": "2024-01-22",
      "milestone": "initial project setup",
      "status": "Planned",
      "fileName": "CLAUDE_2024-01-22_initial-project-setup.md"
    },
    {
      "date": "2024-01-23",
      "milestone": "user authentication added",
      "status": "In Progress",
      "fileName": "CLAUDE_2024-01-23_user-authentication-added.md"
    },
    {
      "date": "2024-01-25",
      "milestone": "comprehensive testing suite",
      "status": "Completed",
      "fileName": "CLAUDE_2024-01-25_comprehensive-testing-suite.md"
    }
  ],
  "summary": "Component found in 3 snapshots",
  "profile": "claude-workflow"
}
```

## Checking Compaction Status

```bash
# Check if compaction is needed
$ contextkeeper check

{
  "snapshotCount": 8,
  "compactionNeeded": false,
  "oldestSnapshot": "FeatureData/DataHistory/CLAUDE/CLAUDE_2024-01-22_initial-project-setup.md",
  "newestSnapshot": "FeatureData/DataHistory/CLAUDE/CLAUDE_2024-01-30_api-v2-complete.md",
  "recommendedAction": "No compaction needed - 8/10 snapshots",
  "profile": "claude-workflow"
}
```

## Comparing Snapshots

```bash
# Compare initial setup with current state
$ contextkeeper compare CLAUDE_2024-01-22_initial-project-setup.md CLAUDE_2024-01-30_api-v2-complete.md

{
  "success": true,
  "snapshot1": "CLAUDE_2024-01-22_initial-project-setup.md",
  "snapshot2": "CLAUDE_2024-01-30_api-v2-complete.md",
  "addedSections": [
    "Authentication",
    "Database Layer",
    "API Endpoints",
    "Testing Strategy"
  ],
  "removedSections": [
    "Planned Features"
  ],
  "modifiedSections": [
    "Project Overview",
    "Architecture",
    "Current Status"
  ],
  "summary": "Changes: 4 added, 1 removed, 3 modified",
  "profile": "claude-workflow"
}
```

## Integration with AI (Claude)

Once added to Claude MCP, you can use natural language:

**You**: "Create a snapshot for the API v2 completion"

**Claude**: "I'll create a snapshot for the API v2 completion."
```bash
$ contextkeeper snapshot api-v2-complete
```

**You**: "When did we add Redis caching?"

**Claude**: "Let me search the history for Redis caching."
```bash
$ contextkeeper search "redis caching"
```

## Tips for Effective Usage

1. **Descriptive Milestones** - Use clear, kebab-case descriptions
2. **Regular Snapshots** - Create snapshots at logical breakpoints
3. **Document Changes** - Fill in the "Changes" section after creating snapshots
4. **Search Effectively** - Use specific terms for better search results
5. **Track Key Components** - Monitor evolution of critical services

## Next Steps

- Explore advanced configuration options
- Set up custom workflow profiles
- Integrate with your CI/CD pipeline
- Create team guidelines for snapshot creation