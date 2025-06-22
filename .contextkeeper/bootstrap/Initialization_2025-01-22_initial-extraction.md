# ContextKeeper Project Initialization Snapshot

**Date**: 2025-01-22
**Purpose**: Bootstrap snapshot for ContextKeeper development until MCP is stable
**Status**: Project extracted from CodeCartographerAI.HistoryMCP

## Project Context

This is the initial snapshot of ContextKeeper, a newly extracted MCP server from the CodeCartographerAI project. This document serves as our development context until ContextKeeper itself is stable enough to manage its own history.

## Current Architecture State

### Project Structure
```
contextkeeper-mcp/
├── src/
│   └── ContextKeeper/
│       ├── Config/
│       │   ├── Models/
│       │   │   └── ContextKeeperConfig.cs
│       │   ├── ConfigurationService.cs
│       │   └── ProfileDetector.cs
│       ├── Core/
│       │   ├── CompactionEngine.cs
│       │   ├── ContextKeeperService.cs
│       │   ├── EvolutionTracker.cs
│       │   ├── SearchEngine.cs
│       │   └── SnapshotManager.cs
│       ├── Json/
│       │   └── ContextKeeperJsonContext.cs
│       ├── Protocol/
│       │   ├── JsonRpcModels.cs
│       │   └── SimpleJsonRpcServer.cs
│       ├── Utils/
│       │   └── FileSystemHelpers.cs
│       └── Program.cs
├── tests/
│   └── ContextKeeper.Tests/
│       ├── TestData/
│       │   └── .contextkeeper/
│       ├── EvolutionTests.cs
│       ├── FunctionalityTest.cs
│       ├── IntegrationTests.cs
│       ├── SearchTests.cs
│       ├── SnapshotTests.cs
│       ├── StorageTests.cs
│       └── TestBase.cs
├── CLAUDE.md
├── README.md
└── Initialization.md (this file)
```

### Technology Stack
- **.NET 9.0** with Native AOT compilation
- **C# 12** with nullable reference types
- **System.CommandLine** for CLI
- **Microsoft.Extensions.Hosting** for DI
- **System.Text.Json** with source generation

### Key Design Decisions

1. **Storage Location**: Standardized on `.contextkeeper/` directory
   - Previously: `FeatureData/DataHistory/`
   - Now: `.contextkeeper/{workflow-name}/`
   - Rationale: Hidden directory, consistent across all workflows

2. **Profile System**: Multi-workflow support
   - claude-workflow: For CLAUDE.md projects
   - readme-workflow: For README.md projects
   - Auto-detection based on file presence

3. **Native AOT**: Full compatibility
   - JsonSerializerContext for all JSON operations
   - No reflection-based serialization
   - 5.6MB standalone binary

4. **Testing Strategy**: Comprehensive test suite
   - Realistic test data (TaskManager API example)
   - Isolated test environments
   - Tests double as usage documentation

## Recent Accomplishments

### Build Warning Resolution
- Fixed 7 CS1998 warnings (async without await)
- Fixed 40+ IL2026/IL3050 warnings (Native AOT)
- Implemented source-generated JSON serialization
- Achieved zero-warning build status

### Test Infrastructure
- Created 40+ unit and integration tests
- Built fictional TaskManager API as test data
- Implemented proper test isolation
- Added test data copying to project file

### Documentation
- Created comprehensive CLAUDE.md
- Updated README.md with new structure
- Added visual directory diagrams
- Documented recent changes

## Current Capabilities

### Working Features
- ✅ Snapshot creation with validation
- ✅ Full-text search across history
- ✅ Component evolution tracking
- ✅ Compaction status checking
- ✅ Multi-profile support
- ✅ MCP server mode (stdio)
- ✅ CLI interface

### MCP Tools Exposed
1. `create_snapshot` - Create timestamped backup
2. `check_compaction` - Check if compaction needed
3. `search_history` - Search across all snapshots
4. `get_evolution` - Track component changes
5. `compare_snapshots` - Diff two snapshots

## Known Issues and Limitations

1. **Test Stability**: Some tests fail due to directory resolution issues when changing current directory
2. **Migration Tool**: No automated migration from old structure yet
3. **Compaction Implementation**: Detection works but actual compaction not implemented
4. **MCP Initialization**: Basic implementation, may need enhancement

## Development Workflow

### Building
```bash
dotnet build
# Should show: 0 Warning(s), 0 Error(s)
```

### Testing
```bash
# Run specific test suites
dotnet test --filter "FullyQualifiedName~StorageTests"

# Run all tests
dotnet test
```

### Running as MCP Server
```bash
# For development
dotnet run --project src/ContextKeeper

# For MCP integration (once stable)
~/.contextkeeper/contextkeeper
```

### Creating Snapshots (CLI)
```bash
# Initialize project
dotnet run --project src/ContextKeeper -- init

# Create snapshot
dotnet run --project src/ContextKeeper -- snapshot feature-name

# Search history
dotnet run --project src/ContextKeeper -- search "search term"
```

## Next Development Steps

### High Priority
1. Fix remaining test failures (directory resolution)
2. Implement actual compaction logic
3. Add file watcher for auto-snapshots
4. Create migration tool for old structure

### Medium Priority
1. Enhance MCP protocol compliance
2. Add diff visualization
3. Implement fuzzy search
4. Create interactive CLI mode

### Future Enhancements
1. Cloud storage backends
2. Collaborative features
3. IDE integrations
4. Web UI for history browsing

## Configuration Reference

### Environment Variables
- `CONTEXTKEEPER_PROFILE` - Override profile detection

### Default Profiles
```csharp
// Claude Workflow (CLAUDE.md projects)
Paths = new PathConfig
{
    History = ".contextkeeper/claude-workflow",
    Snapshots = ".contextkeeper/claude-workflow/snapshots",
    Compacted = ".contextkeeper/claude-workflow/compacted"
}

// README Workflow (README.md projects)
Paths = new PathConfig
{
    History = ".contextkeeper/readme-workflow",
    Snapshots = ".contextkeeper/readme-workflow/snapshots",
    Compacted = ".contextkeeper/readme-workflow/compacted"
}
```

## Bootstrap Process

Until ContextKeeper is stable enough to track its own history:

1. **Manual Snapshots**: Copy this file when making significant changes
2. **Naming Convention**: `Initialization_YYYY-MM-DD_milestone.md`
3. **Storage Location**: `.contextkeeper/bootstrap/`
4. **Transition Plan**: First self-snapshot once MCP is stable

## Development Guidelines

1. **Maintain Native AOT compatibility** - No dynamic JSON serialization
2. **Keep services testable** - Use dependency injection
3. **Document breaking changes** - Update this file
4. **Test before committing** - Ensure zero warnings
5. **Update CLAUDE.md** - Keep AI context current

## Contact and Resources

- **Repository**: https://github.com/chasecupp43/contextkeeper-mcp
- **Author**: Chase Cupp
- **Extracted From**: CodeCartographerAI.HistoryMCP
- **License**: MIT

---

*This initialization snapshot created on 2025-01-22 after completing storage migration and build warning fixes.*