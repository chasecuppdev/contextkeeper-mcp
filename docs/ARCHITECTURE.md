# ContextKeeper Architecture

## Overview

ContextKeeper is built as a modular, extensible system for managing development context history. It follows clean architecture principles with clear separation of concerns.

## System Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   CLI / MCP Interface                    │
├─────────────────────────────────────────────────────────┤
│                  ContextKeeperService                    │
│                   (Orchestration Layer)                  │
├──────────┬──────────┬──────────┬──────────┬───────────┤
│ Snapshot │  Search  │Evolution │Compaction│  Config   │
│ Manager  │  Engine  │ Tracker  │  Engine  │ Service   │
├──────────┴──────────┴──────────┴──────────┴───────────┤
│                    File System Layer                     │
└─────────────────────────────────────────────────────────┘
```

## Core Components

### 1. ContextKeeperService
The main orchestration layer that coordinates all operations:
- Handles all tool requests from MCP or CLI
- Manages configuration and profile selection
- Provides a unified API for all operations

### 2. SnapshotManager
Responsible for creating and comparing snapshots:
- Validates milestone descriptions
- Creates timestamped backups
- Generates snapshot headers from templates
- Compares snapshots to identify changes

### 3. SearchEngine
Provides full-text search across history:
- Searches through all snapshots
- Returns contextual results
- Supports pattern-based file searches

### 4. EvolutionTracker
Tracks how components change over time:
- Identifies component mentions across snapshots
- Detects status changes (Planned → In Progress → Completed)
- Generates timeline views

### 5. CompactionEngine
Implements LSM-tree inspired compaction:
- Monitors snapshot count
- Performs intelligent compaction
- Archives old snapshots
- Maintains query performance

### 6. ConfigurationService
Manages configuration and profiles:
- Auto-detects project types
- Loads workflow profiles
- Handles environment variables
- Manages config file parsing

## Design Patterns

### Strategy Pattern
Different workflow profiles implement different strategies for:
- Directory structures
- Naming conventions
- Compaction thresholds
- Header templates

### Template Method
Snapshot creation follows a template method pattern:
1. Validate input
2. Find main document
3. Generate filename
4. Apply header template
5. Write snapshot

### Repository Pattern
All file system operations are abstracted through service interfaces, making the system testable and maintainable.

## Data Flow

### Snapshot Creation
```
User Request → CLI/MCP → ContextKeeperService → SnapshotManager
                                                        ↓
                                              Validate Milestone
                                                        ↓
                                               Read Main Document
                                                        ↓
                                              Generate Snapshot
                                                        ↓
                                               Write to Disk
```

### Search Operation
```
Search Term → SearchEngine → Load Snapshots → Search Content
                                                     ↓
                                             Extract Context
                                                     ↓
                                              Return Results
```

## File Structure

### Project Layout
```
project-root/
├── CLAUDE.md                    # Main document (example)
├── contextkeeper.config.json    # Optional config
└── FeatureData/
    └── DataHistory/
        ├── CLAUDE/              # Snapshots directory
        │   ├── CLAUDE_2024-01-15_initial-setup.md
        │   └── CLAUDE_2024-01-20_feature-complete.md
        └── Compacted/           # Compacted snapshots
            └── Archived_2024-Q1/
```

### Snapshot Format
Each snapshot contains:
1. **Header** - Metadata about the snapshot
2. **Changes Section** - What changed in this version
3. **Context Section** - Why changes were made
4. **Content** - Full document content at that point

## Performance Considerations

### Native AOT Compilation
- Compiled to native code for fast startup
- ~5.6MB binary size
- No JIT compilation overhead
- Instant availability for AI tools

### Efficient Search
- Line-by-line search with early termination
- Configurable result limits
- Context extraction for relevance

### Scalable Storage
- LSM-tree inspired organization
- Automatic compaction at thresholds
- Archived snapshots for long-term storage

## Extensibility

### Adding New Profiles
1. Create profile JSON in `profiles/` directory
2. Define detection rules
3. Configure paths and templates
4. Profile auto-loads on startup

### Adding New Tools
1. Add method to `ContextKeeperService`
2. Register in `SimpleJsonRpcServer`
3. Update tool list in protocol handler
4. Add CLI command if needed

### Custom Compaction Strategies
1. Extend `CompactionEngine`
2. Implement strategy selection logic
3. Configure in profile

## Security Considerations

- No network operations (local-only)
- Read/write only in configured directories
- No execution of external commands
- Sanitized file paths and names

## Future Architecture Considerations

### Planned Enhancements
- Plugin system for custom analyzers
- Git integration for correlation
- Web UI for visualization
- Cloud backup options

### Scalability Path
- Database backend for large histories
- Distributed search capabilities
- Multi-project management
- Team collaboration features