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

### Test Data Structure

#### Existing C# Test Data ✅
- **TestData/TestSolution/** - A comprehensive test C# solution containing:
  - TestLibrary project (class library with models, services, repositories)
  - TestApp project (console app with controllers)
  - Complex inheritance hierarchies (IRepository<T>, Service<T>, BaseController)
  - XML documentation throughout
  - Generic types and interfaces
  - Cross-project references
  
- **TestData/.contextkeeper/** - Sample snapshot data:
  - claude-workflow/snapshots/ - Sample CLAUDE_*.md files
  - readme-workflow/snapshots/ - Sample README_*.md files
  - Contains search terms: PostgreSQL, JWT, Repository, Clean Architecture

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
- Microsoft.Build.Locator (MSBuild registration)
- Microsoft.CodeAnalysis.CSharp.Workspaces (Roslyn)
- Microsoft.CodeAnalysis.Workspaces.MSBuild (Solution/project loading)
- ModelContextProtocol 0.3.0-preview.1 (MCP SDK)
- No external databases or services required

## Security Considerations

1. All operations are local file system based
2. No network communication except stdio for MCP
3. No credentials or secrets stored
4. Respects file system permissions

## Test Suite Implementation Progress

### Phase 1 - Completed (2025-06-23 Morning)

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
- **Tests**: 65 passing, 32 failing (out of 97 total)
- **Progress**: Increased from 48 to 65 passing tests (+35.4% improvement)

### Remaining Test Failures by Category

```
7  CodeAnalysis tests (SymbolSearchService pattern matching, inheritance)
7  EvolutionTests
7  SearchTests (cannot find snapshot files despite correct paths)
4  IntegrationTests
4  StorageTests
3  SnapshotTests
1  Protocol test (MCP server registration)
```

### Phase 2 - In Progress (2025-06-23 Afternoon)

#### Successfully Completed in Phase 2

1. **Fixed CodeAnalysis Test Issues** ✅
   - Updated all tests to use TestLibrary/TestApp instead of non-existent TestProject
   - Changed CodeSearchTools tests to use solution files instead of project files
   - Fixed MSBuild registration and solution loading
   - Reduced failures from 16 to 7

2. **Updated Path Configuration** ✅
   - Fixed ProfileDetector to use new `.contextkeeper` paths instead of old `FeatureData/DataHistory`
   - Updated both claude-workflow and readme-workflow profiles
   - Ensured ConfigurationService has matching paths

3. **Fixed Test Data Issues** ✅
   - Ensured test data (including hidden .contextkeeper directories) copies to output
   - Updated project file to include all test data files
   - Verified snapshot files contain expected search terms

4. **Discovered Key Issues**
   - **Pattern Matching**: Roslyn's `FindSourceDeclarationsWithPatternAsync` uses wildcards (`*`), not regex
   - **Search Path Issue**: Despite correct paths and data, search tests still can't find snapshots
   - **Inheritance Tests**: Some derived class and interface implementation tests failing

### Current Blockers

1. **Search/Evolution Tests (14 failures)**
   - Test data exists in correct location
   - Paths are configured correctly
   - But SearchEngine still returns 0 results
   - Likely issue with profile detection or initialization

2. **CodeAnalysis Pattern Tests (7 failures)**
   - Need to adjust expectations for wildcard vs regex patterns
   - Inheritance hierarchy navigation needs fixing
   - Symbol documentation retrieval failing

3. **Integration/Storage Tests (8 failures)**
   - Profile detection not working correctly
   - May be related to the search issue

## Next Session Implementation Plan

### Phase 3: Resolve Remaining Blockers

#### 1. **Debug Search/Evolution Tests (14 failures) - HIGHEST PRIORITY**
- Investigate why SearchEngine returns 0 results despite correct setup
- Check if profile detection is working in test environment
- Verify Environment.CurrentDirectory handling in tests
- May need to debug step-by-step through SearchEngine.SearchAsync

#### 2. **Fix Remaining CodeAnalysis Tests (7 failures)**
- Update pattern tests to use wildcards instead of regex
- Fix FindDerivedClassesAsync for generic interfaces
- Fix inheritance hierarchy navigation
- Resolve symbol documentation retrieval

#### 3. **Resolve Integration/Storage Tests (8 failures)**
- Fix profile auto-detection logic
- Ensure test isolation between different workflow types
- Update expected values based on actual implementation

#### 4. **Complete Snapshot Tests (3 failures)**
- Fix validation error message format
- Ensure milestone validation regex is correct
- Test snapshot comparison logic

### Phase 4: Complete Test Coverage

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

## Using ContextKeeper's Snapshot Pattern

### Creating Development Snapshots

When making significant changes or reaching milestones:

1. **Before Major Changes**: Create a snapshot to preserve current state
   ```bash
   cp CLAUDE.md .contextkeeper/claude-workflow/snapshots/CLAUDE_$(date +%Y-%m-%d)_before-change.md
   ```

2. **After Progress**: Document what was accomplished
   ```bash
   # Edit CLAUDE.md to reflect current reality
   # Then create a progress snapshot
   cp CLAUDE.md .contextkeeper/claude-workflow/snapshots/CLAUDE_$(date +%Y-%m-%d)_progress-update.md
   ```

3. **Reference Previous Work**: Check snapshots to understand context
   ```bash
   ls -la .contextkeeper/claude-workflow/snapshots/
   grep -i "search" .contextkeeper/claude-workflow/snapshots/*.md
   ```

### Current Snapshots

- `CLAUDE_2025-06-23_test-suite-progress.md` - State before documentation cleanup
- `CLAUDE_2025-06-23_documentation-cleanup.md` - Will be created after this update

This approach maintains context across AI sessions by preserving history and progress markers.

---

*This document serves as the primary context for AI assistants working on ContextKeeper development.*