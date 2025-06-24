# CLAUDE.md Historical Snapshot
**Date**: 2025-06-23
**Milestone**: test-suite-progress
**Previous State**: Initial test implementation
**Context**: Snapshot taken before cleaning up documentation to reflect actual test suite progress

---

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

## Next Steps: Comprehensive Regression Test Suite

### Overview
Before the C# code search MCP integration is considered stable, we need a comprehensive regression test suite that covers all existing functionality plus the new features.

### Test Structure Plan

#### 1. **Core Functionality Tests** (Update existing)
- StorageTests.cs - Verify configuration and directory structure
- SnapshotTests.cs - Test snapshot creation, validation, and edge cases
- SearchTests.cs - Test search functionality across snapshots
- EvolutionTests.cs - Test component tracking over time
- IntegrationTests.cs - End-to-end workflow scenarios

#### 2. **New C# Code Search Tests** (Create new)
- **CodeAnalysis/WorkspaceManagerTests.cs**
  - Solution loading and caching
  - Project loading
  - MSBuild registration
  - Error handling for invalid solutions/projects
  
- **CodeAnalysis/SymbolSearchServiceTests.cs**
  - FindSymbolsAsync with various filters
  - FindReferencesAsync
  - FindSymbolsByPatternAsync
  - FindDerivedClassesAsync
  - FindImplementationsAsync
  - Cache functionality
  
- **CodeAnalysis/CodeSearchToolsTests.cs**
  - FindSymbolDefinitions tool
  - FindSymbolReferences tool
  - NavigateInheritanceHierarchy tool
  - SearchSymbolsByPattern tool
  - GetSymbolDocumentation tool
  - JSON serialization/AOT compatibility

#### 3. **MCP Protocol Tests** (Create new)
- **Protocol/McpServerTests.cs**
  - Server initialization
  - Tool registration
  - Request/response handling
  - Error handling
  
- **Protocol/ContextKeeperMcpToolsTests.cs**
  - All ContextKeeper MCP tools
  - Integration with MCP SDK

#### 4. **Performance Tests** (Create new)
- **Performance/PerformanceTests.cs**
  - Large solution loading times
  - Symbol search performance
  - Cache effectiveness
  - Memory usage

#### 5. **Regression Test Suite** (Create new)
- **RegressionTests.cs**
  - Backward compatibility tests
  - Feature interaction tests
  - Edge case scenarios
  - Error recovery tests

### Test Data Requirements

#### New C# Test Data (To create)
- **TestSolution/** - A test C# solution with:
  - Multiple projects
  - Interfaces and implementations
  - Inheritance hierarchies
  - XML documentation
  - Various symbol types (classes, methods, properties, etc.)
  - Cross-project references

### Implementation Phases

1. **Update Existing Tests**
   - Update TestBase.cs to include new services
   - Add test categories/traits for organization
   - Ensure existing tests pass with new dependencies

2. **Create C# Code Search Tests**
   - Create test solution structure
   - Implement tests for all code analysis components
   - Test AOT compatibility

3. **Create MCP Protocol Tests**
   - Test server initialization
   - Test tool registration and discovery
   - Test request/response handling

4. **Create Performance Tests**
   - Define performance benchmarks
   - Create large test solutions
   - Implement performance measurement

5. **Create Regression Suite**
   - Document critical scenarios
   - Create comprehensive test cases
   - Test feature interactions

### Test Execution Strategy

```bash
# Run all tests
dotnet test

# Run specific category
dotnet test --filter "Category=Core"
dotnet test --filter "Category=CodeSearch"
dotnet test --filter "Category=Performance"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Success Criteria
- ✅ 100% of existing functionality covered
- ✅ 90%+ code coverage for new C# features
- ✅ All tests pass consistently
- ✅ Performance benchmarks established
- ✅ Clear documentation for maintainers
- ✅ Easy to run locally and in CI/CD

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

## Test Suite Implementation - Completed Phase 1 (2025-06-23)

### Successfully Completed Tasks

#### 1. **Interface Extraction for Dependency Injection** ✅
- Created interfaces for all core services:
  - `ISnapshotManager` - For snapshot creation and comparison
  - `ISearchEngine` - For full-text search functionality
  - `IEvolutionTracker` - For tracking component evolution
  - `ICompactionEngine` - For LSM-tree inspired compaction
  - `IContextKeeperService` - For the main service orchestration
- Updated all classes to implement their respective interfaces
- Updated DI registration in both production (`Program.cs`) and test code (`TestBase.cs`)
- Updated all dependent classes to use interfaces instead of concrete types

#### 2. **Test Infrastructure Improvements** ✅
- Fixed `TestBase.cs` initialization issues:
  - Changed from `Host.CreateDefaultBuilder()` to `new HostBuilder()` to avoid file system issues
  - Added safety checks for TestDataPath existence
  - Updated service registration to use interfaces
- Fixed test environment setup in multiple test classes:
  - Added proper directory management in SearchTests, EvolutionTests, IntegrationTests
  - Implemented IDisposable pattern to restore original directories after tests

#### 3. **Error Handling Design Improvement** ✅
- Changed `WorkspaceManager` from returning null to throwing exceptions:
  - `FileNotFoundException` for missing files
  - `InvalidOperationException` for loading failures
- Updated tests to expect exceptions instead of null returns
- Improved debugging experience with explicit error messages

#### 4. **Test Data Setup** ✅
- Created comprehensive C# test solution structure:
  - TestSolution with TestLibrary and TestApp projects
  - Multiple classes, interfaces, enums, and inheritance relationships
  - XML documentation comments
  - Cross-project references
- Added README.md for readme-workflow tests
- Fixed project file to exclude TestData from compilation

### Current Status

- **Build**: 0 warnings, 0 errors ✅
- **Tests**: 55 passing, 42 failing (out of 97 total)
- **Progress**: Increased from 48 to 55 passing tests (+14.6% improvement)

### Remaining Test Failures by Category

```
16 CodeAnalysis tests (WorkspaceManager, SymbolSearchService, CodeSearchTools)
7  EvolutionTests
7  SearchTests  
5  IntegrationTests
4  StorageTests
3  SnapshotTests
```

## Next Session Implementation Plan

### Phase 2: Fix Remaining Test Failures (High Priority)

#### 1. **CodeAnalysis Tests (16 failures)**
- Update tests to use TestLibrary/TestApp instead of TestProject
- Fix test data paths and ensure proper solution structure
- Handle MSBuild registration in test environment
- Update expected symbol names and types to match test solution

#### 2. **Search and Evolution Tests (14 failures)**
- Ensure test data files contain expected search terms
- Fix path resolution for snapshot directories
- Update expected counts based on actual test data

#### 3. **Integration and Storage Tests (9 failures)**
- Fix profile detection for both claude-workflow and readme-workflow
- Ensure proper test data isolation
- Update expected file counts and paths

#### 4. **Snapshot Tests (3 failures)**
- Fix validation error message expectations
- Ensure proper test data structure for snapshot creation

### Phase 3: Complete Test Coverage

Once all tests pass:
1. Create Performance/PerformanceTests.cs
2. Create RegressionTests.cs
3. Add test categories for selective execution
4. Generate code coverage report
5. Document any remaining gaps

### Key Learnings Applied

1. **Interfaces over Concrete Classes**: All services now use interfaces for better testability
2. **Explicit Error Handling**: Exceptions provide better debugging than null returns
3. **Test Isolation**: Each test class manages its own environment
4. **Shared Test Data**: Using a common TestSolution for all CodeAnalysis tests

### Commands for Next Session

```bash
# Check current test status
dotnet test --no-build --verbosity minimal

# Run specific failing test categories
dotnet test --filter "FullyQualifiedName~CodeAnalysis"
dotnet test --filter "FullyQualifiedName~SearchTests"

# Debug specific test
dotnet test --filter "FullyQualifiedName~SpecificTestName" --verbosity detailed
```

---

*This document serves as the primary context for AI assistants working on ContextKeeper development.*