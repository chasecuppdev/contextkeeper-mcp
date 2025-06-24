# CLAUDE.md Historical Snapshot
**Date**: 2025-06-23
**Milestone**: post compaction
**Previous State**: CLAUDE_2025-06-23_pre-compaction.md
**Compaction Status**: Major Compaction Completed

## Changes in This Version
- Performed major compaction following Anthropic's best practices
- Reduced from 548 lines to 154 lines (72% reduction)
- Removed historical implementation details while preserving in snapshots
- Restructured for clarity and immediate utility

## Context for Future Reference
- All historical context preserved in pre-compaction snapshot
- New structure focuses on essential commands, critical warnings, and current state
- Follows "concise and human-readable" principle from Anthropic guidelines
- Test implementation phases and detailed progress archived but accessible

---
# ContextKeeper

AI-powered development context management with LSM-tree inspired history tracking. Implements Model Context Protocol (MCP) for AI assistants.

## Essential Commands

```bash
# Build and test
dotnet build
dotnet test

# Run as MCP server
dotnet run --project src/ContextKeeper

# CLI operations
dotnet run --project src/ContextKeeper -- snapshot <milestone-name>
dotnet run --project src/ContextKeeper -- search "search term"
dotnet run --project src/ContextKeeper -- check
```

## Critical Information

**IMPORTANT**: 
- Tests MUST use proper isolation - see test helpers in `tests/ContextKeeper.Tests/Helpers/`
- Path resolution MUST use `Path.Combine(Directory.GetCurrentDirectory(), relativePath)`
- Roslyn pattern matching uses wildcards (*), not regex
- All projects target .NET 9.0 with Native AOT compatibility

## Architecture

### Core Services
- **SnapshotManager** - Creates/manages timestamped documentation backups
- **SearchEngine** - Full-text search across all snapshots
- **EvolutionTracker** - Tracks component mentions over time
- **CompactionEngine** - LSM-tree inspired consolidation strategy
- **ProfileDetector** - Auto-detects project type for zero-config usage

### Storage Layout
```
.contextkeeper/
├── claude-workflow/
│   ├── snapshots/      # Individual timestamped snapshots
│   └── compacted/      # Quarterly archives
└── readme-workflow/
    ├── snapshots/
    └── compacted/      # Yearly archives
```

### Key Patterns
- Dependency injection via Microsoft.Extensions.DependencyInjection
- All services implement interfaces (ISnapshotManager, ISearchEngine, etc.)
- Source-generated JSON serialization for AOT compatibility
- Immutable history - snapshots never modified after creation

## Testing

### Current Status
- 82 tests passing, 15 failing
- Build: 0 warnings, 0 errors

### Test Organization
- **StorageTests** - Configuration and directory structure
- **SnapshotTests** - Snapshot creation and validation
- **SearchTests** - Search functionality
- **EvolutionTests** - Component tracking over time
- **IntegrationTests** - End-to-end workflows
- **CodeAnalysis/** - Roslyn integration tests

### Test Best Practices
```csharp
// Use mocked configuration to prevent file pollution
public TestClass() : base(useMockConfiguration: true)

// Always isolate test environments
_tempDirectory = CreateTempDirectory();
CopyTestData(_tempDirectory);
Environment.CurrentDirectory = _tempDirectory;

// Clean up in Dispose
Environment.CurrentDirectory = _originalDirectory;
Directory.Delete(_tempDirectory, true);
```

## Configuration

### Workflow Profiles
1. **claude-workflow** - CLAUDE.md projects, 10 snapshot threshold, quarterly compaction
2. **readme-workflow** - README.md projects, 20 snapshot threshold, yearly compaction

### Environment Variables
- `CONTEXTKEEPER_PROFILE` - Override auto-detected profile

## MCP Integration

### Available Tools
- `snapshot` - Create documentation snapshot
- `search` - Search across history
- `check` - Check compaction status
- `evolution` - Track component evolution
- `compare` - Compare two snapshots

### C# Code Search Tools (via Roslyn)
- `FindSymbolDefinitions` - Find symbol declarations
- `FindSymbolReferences` - Find all references
- `NavigateInheritanceHierarchy` - Explore type hierarchies
- `SearchSymbolsByPattern` - Wildcard pattern search
- `GetSymbolDocumentation` - Extract XML docs

## Current Issues

### Test Failures (15 remaining)
- **CodeAnalysis (6)** - Pattern matching expectations, generic type handling
- **Snapshots (4)** - Validation message format
- **Integration (3)** - Profile detection edge cases
- **Protocol (1)** - MCP server registration
- **Evolution (1)** - Component tracking logic

### Known Limitations
- Roslyn symbol search doesn't handle all generic type scenarios
- MCP server registration test timing issues
- Some validation error messages don't match expected format

## Dependencies

- .NET 9.0
- System.CommandLine
- Microsoft.Extensions.Hosting
- Microsoft.CodeAnalysis.* (Roslyn)
- ModelContextProtocol 0.3.0-preview.1
- System.Text.Json (source-generated)

## Quick Reference

### Creating Snapshots
```bash
# Manual snapshot creation
cp CLAUDE.md .contextkeeper/claude-workflow/snapshots/CLAUDE_$(date +%Y-%m-%d)_<milestone>.md

# Via CLI
dotnet run --project src/ContextKeeper -- snapshot feature-complete
```

### Debugging Tests
```bash
# Run specific test category
dotnet test --filter "FullyQualifiedName~StorageTests"

# Debug with detailed output
dotnet test --filter "TestName" --verbosity detailed
```

---

*For historical context and detailed implementation notes, see `.contextkeeper/claude-workflow/snapshots/`*