# ContextKeeper - AI Development Assistant Context

## Project Overview

ContextKeeper is an AI-powered development context management tool that implements LSM-tree inspired history tracking. It's designed to make development history accessible to AI assistants through the Model Context Protocol (MCP).

## Architecture

### Core Components

1. **SnapshotManager** (`Core/SnapshotManager.cs`)
   - Creates timestamped backups of project documentation
   - Validates milestone names using regex patterns
   - Manages snapshot file naming and organization

2. **SearchEngine** (`Core/SearchEngine.cs`)
   - Full-text search across all snapshots
   - Case-insensitive matching with context extraction
   - Pattern-based file filtering

3. **EvolutionTracker** (`Core/EvolutionTracker.cs`)
   - Tracks component mentions across snapshots
   - Identifies architectural evolution patterns
   - Generates timeline views of project history

4. **CompactionEngine** (`Core/CompactionEngine.cs`)
   - Monitors snapshot count against thresholds
   - Implements LSM-tree inspired compaction strategy
   - Recommends when to consolidate history

5. **ProfileDetector** (`Config/ProfileDetector.cs`)
   - Auto-detects project type based on files/structure
   - Supports multiple workflow profiles
   - Enables zero-configuration usage

### Storage Structure

As of the latest update, ContextKeeper uses a standardized `.contextkeeper/` directory:

```
.contextkeeper/
├── claude-workflow/
│   ├── snapshots/      # Individual timestamped snapshots
│   └── compacted/      # Quarterly/yearly archives
└── readme-workflow/
    ├── snapshots/
    └── compacted/
```

### Key Design Patterns

1. **Dependency Injection**: All services are injected via Microsoft.Extensions.DependencyInjection
2. **Async/Await**: Consistent async patterns throughout (though some methods use Task.FromResult for sync operations)
3. **Configuration as Code**: Workflow profiles defined in code with built-in defaults
4. **Immutable History**: Snapshots are never modified after creation
5. **Native AOT Compatible**: Uses source-generated JSON serialization

## Development Workflow

### Building
```bash
dotnet build
```

### Testing
```bash
dotnet test
```

### Running as MCP Server
```bash
dotnet run --project src/ContextKeeper
```

### Running CLI Commands
```bash
dotnet run --project src/ContextKeeper -- snapshot feature-implementation
dotnet run --project src/ContextKeeper -- search "authentication"
dotnet run --project src/ContextKeeper -- check
```

## Recent Changes

### C# Code Search Integration (Latest)
- Added Microsoft C# MCP SDK (ModelContextProtocol 0.3.0-preview.1)
- Integrated Roslyn for powerful C# code analysis
- Created WorkspaceManager for solution/project loading
- Implemented SymbolSearchService with caching
- Added CodeSearchTools with 5 MCP tools:
  - FindSymbolDefinitions
  - FindSymbolReferences
  - NavigateInheritanceHierarchy
  - SearchSymbolsByPattern
  - GetSymbolDocumentation
- Fixed all AOT compatibility issues with source-generated JSON
- Updated Program.cs to use MCP SDK server capabilities

### Build Warning Fixes
- Fixed all CS1998 warnings by removing unnecessary async/await
- Implemented JsonSerializerContext for Native AOT compatibility
- Added pragma suppressions for safe JsonArray operations
- Achieved 0 warnings, 0 errors build status

### Storage Location Update
- Migrated from `FeatureData/DataHistory/` to `.contextkeeper/` directory
- Updated all workflow profiles to use consistent storage location
- Added comprehensive test suite with realistic example data

## Testing Strategy

The project includes comprehensive tests organized by functionality:

1. **StorageTests**: Verify configuration and directory structure
2. **SnapshotTests**: Test snapshot creation and validation
3. **SearchTests**: Verify search functionality across snapshots
4. **EvolutionTests**: Test component tracking over time
5. **IntegrationTests**: End-to-end workflow scenarios

Test data includes a fictional "TaskManager API" project showing realistic evolution.

## MCP Protocol Implementation

ContextKeeper implements a simplified JSON-RPC server (`Protocol/SimpleJsonRpcServer.cs`) that:
- Handles stdio communication
- Exposes all core functions as MCP tools
- Returns structured JSON responses
- Supports the standard MCP initialization flow

## Configuration

### Built-in Profiles

1. **claude-workflow**: For CLAUDE.md based projects
   - 10 snapshot threshold
   - Quarterly compaction
   - LSM-tree pattern

2. **readme-workflow**: For README.md based projects
   - 20 snapshot threshold
   - Yearly compaction
   - Standard documentation pattern

### Environment Variables
- `CONTEXTKEEPER_PROFILE`: Override default profile selection

## Performance Considerations

1. **File I/O**: All operations are file-based, no database required
2. **Memory Usage**: Minimal - processes files streaming where possible
3. **Startup Time**: ~50ms with Native AOT compilation
4. **Binary Size**: ~5.6MB standalone executable

## Future Enhancements

1. **Migration Tool**: Automated migration from old storage locations
2. **Fuzzy Search**: Implement fuzzy matching for better search results
3. **Diff Visualization**: Better comparison output formatting
4. **Cloud Storage**: Optional S3/Azure blob storage backends
5. **Real-time Monitoring**: File system watcher for automatic snapshots

## Contributing Guidelines

1. Maintain the existing architecture patterns
2. Keep nullable reference types enabled
3. Ensure Native AOT compatibility
4. Write tests for new features
5. Update this document with significant changes

## Dependencies

- .NET 9.0
- System.CommandLine (CLI parsing)
- Microsoft.Extensions.Hosting (DI container)
- System.Text.Json (JSON serialization)
- No external databases or services required

## Security Considerations

1. All operations are local file system based
2. No network communication except stdio for MCP
3. No credentials or secrets stored
4. Respects file system permissions

---

*This document serves as the primary context for AI assistants working on ContextKeeper development.*