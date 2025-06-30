# Changelog

All notable changes to ContextKeeper will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Technical deep dive section in README with LSM-tree architecture details
- Concrete usage examples and quick demo section
- Performance benchmarks and Native AOT metrics
- Comprehensive testing documentation (98 tests passing)

### Fixed
- Updated all GitHub URLs to use correct username
- Cleaned up duplicate nested directories in project structure

### Changed
- Enhanced README with interview-ready content
- Improved performance metrics with actual benchmarks

## [2.0.0] - 2025-06-24

### Changed - BREAKING
- Complete architectural refactoring from documentation-focused to comprehensive development context management
- Removed ProfileDetector and multi-profile system in favor of single unified workflow
- Changed snapshot naming from `PREFIX_date_milestone` to `SNAPSHOT_date_type_milestone`
- Simplified configuration from workflow profiles to single `ContextKeeperConfig`
- Moved from `.contextkeeper/{workflow}/` to `.contextkeeper/` directory structure
- MCP tool methods renamed for clarity:
  - `CreateSnapshot` → `Snapshot`
  - `CheckCompaction` → `GetStatus`
  - `SearchHistory` → `SearchEvolution`
  - `GetEvolution` → `TrackComponent`
  - Added `GetTimeline` as new tool

### Added
- **Comprehensive Context Capture**
  - Git context (branch, commit, uncommitted files)
  - Workspace information (working directory, recent commands)
  - Full documentation capture (all markdown files)
  - Metadata tracking with full JSON context

- **Automatic Features**
  - Auto-compaction based on count (20) and age (90 days)
  - Git hooks integration (pre-commit, post-checkout)
  - Non-blocking background compaction
  - Automatic context capture on git operations

- **Enhanced MCP Tools**
  - `snapshot` - Create comprehensive context snapshots
  - `search_evolution` - Natural language search through history
  - `track_component` - Follow feature evolution over time
  - `compare_snapshots` - Compare any two snapshots (supports 'latest')
  - `get_status` - Complete system status with insights
  - `get_timeline` - Chronological project evolution view

- **Development Improvements**
  - GitHelper utility for git operations
  - ContextCaptureService for comprehensive context gathering
  - Enhanced validation with detailed error messages
  - Improved test isolation with xUnit collection behaviors

### Fixed
- All 15 original test failures from v1.1.0
- Test isolation issues causing ~11 additional failures
- Build warnings for Native AOT compatibility
- Accumulated test artifacts (cleaned 4,152 directories)

### Removed
- ProfileDetector class and profile detection logic
- WorkflowProfile concept
- Multiple workflow configurations (claude-workflow, readme-workflow)
- Manual compaction commands (now automatic)

## [1.1.0] - 2025-06-23

### Added
- **C# Code Search Capabilities** via Microsoft MCP SDK integration
  - `FindSymbolDefinitions` - Find where symbols are declared
  - `FindSymbolReferences` - Find all references to a symbol
  - `NavigateInheritanceHierarchy` - Explore type hierarchies
  - `SearchSymbolsByPattern` - Pattern-based symbol search
  - `GetSymbolDocumentation` - Extract XML documentation
- Roslyn integration for deep C# code analysis
- WorkspaceManager for solution/project loading
- SymbolSearchService for code navigation
- Native AOT compatibility throughout

### Fixed
- Build warnings for AOT compatibility
- JSON serialization updated to use source generators

### Technical
- Integrated Microsoft.CodeAnalysis (Roslyn) 4.12.0
- Added ModelContextProtocol 0.3.0-preview.1
- Maintained zero-warning build status

## [1.0.0] - 2025-06-22

### Initial Release
Extracted from CodeCartographerAI with battle-tested functionality:

### Core Features
- **Snapshot Management** - Create timestamped documentation backups
- **Search Engine** - Full-text search across all snapshots
- **Evolution Tracking** - Track component mentions over time
- **Compaction Engine** - LSM-tree inspired consolidation
- **Profile Detection** - Auto-detect project type (CLAUDE.md vs README.md)

### MCP Integration
- Native Model Context Protocol server
- Five foundational MCP tools:
  - `CreateSnapshot` - Create documentation snapshots
  - `CheckCompaction` - Check if compaction needed
  - `SearchHistory` - Search across snapshots
  - `GetEvolution` - Track component evolution
  - `CompareSnapshots` - Compare two snapshots

### Technical Foundation
- .NET 9.0 with Native AOT support
- 5.6MB standalone binary
- ~50ms startup time
- Comprehensive test suite (82 tests)
- Clean architecture with DI
- Zero-warning build

### Storage Architecture
- `.contextkeeper/` directory structure
- Workflow-based organization
- Immutable snapshot history
- Quarterly/yearly compaction strategies

---

## Version History Summary

- **v2.0.0** (2025-06-24) - Major refactoring: Comprehensive context capture, auto-compaction, enhanced MCP tools
- **v1.1.0** (2025-06-23) - Added C# code analysis with Roslyn integration
- **v1.0.0** (2025-06-22) - Initial release extracted from CodeCartographerAI

## Development History

### Origin Story
ContextKeeper was extracted from the CodeCartographerAI project where it proved invaluable for maintaining development context across AI sessions. The CLAUDE.md workflow pattern emerged from real-world needs and has been battle-tested through months of development.

### Key Milestones
- **2024-12-01** - Initial concept in CodeCartographerAI
- **2025-06-22** - Extraction as standalone tool (v1.0.0)
- **2025-06-23** - C# code analysis added (v1.1.0)
- **2025-06-24** - Major architectural refactoring (v2.0.0)

### Acknowledgments
- Inspired by LSM-tree data structures
- Built for the Model Context Protocol ecosystem
- Thanks to the CodeCartographerAI project for proving the concept
- Community feedback that led to the v2.0 simplification