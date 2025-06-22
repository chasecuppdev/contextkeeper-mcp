# ContextKeeper 🚀

AI-powered development context management with LSM-tree inspired history tracking. Make your development history accessible to AI assistants through the Model Context Protocol (MCP).

## What is ContextKeeper?

ContextKeeper is a tool that helps AI assistants understand your project's evolution by maintaining intelligent snapshots of your documentation. It was born from the need to preserve development context across AI sessions, implementing a proven workflow that has been battle-tested on real projects.

### Key Features

- 📸 **Smart Snapshots** - Create timestamped backups with milestone tracking
- 🔍 **Intelligent Search** - Find when and why changes were made
- 📊 **Evolution Tracking** - See how components developed over time
- 🗜️ **Automatic Compaction** - LSM-tree inspired history management
- 🤖 **AI-Native** - Built for Model Context Protocol (MCP) integration
- 🎯 **Multi-Profile** - Adapts to different project structures

## Quick Start

### 30-Second Installation

```bash
# Install ContextKeeper
curl -sSL https://raw.githubusercontent.com/chasecupp43/contextkeeper-mcp/main/scripts/install.sh | bash

# Initialize in your project
contextkeeper init

# Create your first snapshot
contextkeeper snapshot initial-setup
```

### For Windows Users

```powershell
# Download and run the installer
iwr -useb https://raw.githubusercontent.com/chasecupp43/contextkeeper-mcp/main/scripts/install.ps1 | iex
```

## Usage Examples

### Basic Workflow

```bash
# Create a snapshot after implementing a feature
contextkeeper snapshot feature-user-authentication

# Search for when you added PostgreSQL
contextkeeper search postgresql

# Track how your API evolved
contextkeeper evolution "API"

# Check if compaction is needed
contextkeeper check
```

### AI Integration (Claude)

```bash
# Add ContextKeeper to Claude
claude mcp add contextkeeper -- ~/.contextkeeper/contextkeeper

# Then in Claude, you can use:
# "Create a snapshot for the testing implementation"
# "Show me the evolution of the ArchitecturePatternDetector"
# "Search history for when we added fuzzy matching"
```

## How It Works

ContextKeeper uses an LSM-tree inspired approach to manage your development history:

1. **Write-Ahead Log Pattern** - Every significant change is backed up before modification
2. **Immutable History** - Previous states are preserved as timestamped snapshots
3. **Automatic Compaction** - When 10+ snapshots accumulate, they're intelligently compacted
4. **Smart Detection** - Automatically adapts to your project structure

## Supported Workflows

### CLAUDE.md Projects (Default)
Perfect for AI-assisted development with comprehensive project documentation:
- Detects `CLAUDE.md` files
- Uses `FeatureData/DataHistory` structure
- Implements proven snapshot patterns

### README-based Projects
For traditional projects with README documentation:
- Detects `README.md` files
- Creates hidden `.history` directory
- Higher compaction threshold (20 snapshots)

### Custom Workflows
Create your own workflow profiles for specific needs:
- Define custom detection rules
- Set your own directory structure
- Configure compaction strategies

## Configuration

ContextKeeper can be configured through:

1. **Auto-detection** - Automatically detects project type
2. **Environment variables** - `CONTEXTKEEPER_PROFILE=custom`
3. **Config file** - `contextkeeper.config.json` in project root
4. **Command line** - `--profile` option

### Example Configuration

```json
{
  "version": "1.0",
  "defaultProfile": "claude-workflow",
  "profiles": {
    "my-custom-workflow": {
      "name": "my-custom-workflow",
      "detection": {
        "files": ["ARCHITECTURE.md"],
        "paths": ["docs/history"]
      },
      "paths": {
        "history": "docs/history",
        "snapshots": "docs/history/snapshots"
      }
    }
  }
}
```

## Advanced Features

### Compaction

When your snapshot count reaches the threshold, ContextKeeper will recommend compaction:

```bash
$ contextkeeper check
{
  "snapshotCount": 12,
  "compactionNeeded": true,
  "recommendedAction": "Compaction recommended - 12/10 snapshots exist"
}
```

### Evolution Tracking

Track how specific components evolved:

```bash
$ contextkeeper evolution "DatabaseService"
{
  "componentName": "DatabaseService",
  "evolutionSteps": [
    {
      "date": "2024-01-15",
      "milestone": "initial implementation",
      "status": "Planned"
    },
    {
      "date": "2024-01-20",
      "milestone": "postgresql integration",
      "status": "In Progress"
    },
    {
      "date": "2024-01-25",
      "milestone": "connection pooling",
      "status": "Completed"
    }
  ]
}
```

## Why ContextKeeper?

### The Problem
AI assistants lose context between sessions. Your project's history, architectural decisions, and evolution are locked away in git commits or scattered documentation.

### The Solution
ContextKeeper makes your development history accessible to AI, enabling:
- Better architectural decisions based on past patterns
- Quick recovery of "why did we do it this way?" answers
- Consistent documentation across AI sessions
- Preservation of institutional knowledge

## Architecture

ContextKeeper is built with:
- **.NET 9** - Latest framework with Native AOT support
- **5.6MB binary** - Fast startup, minimal footprint
- **Model Context Protocol** - Native MCP server implementation
- **Extensible design** - Easy to add new features and workflows

## Contributing

We welcome contributions! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Development Setup

```bash
# Clone the repository
git clone https://github.com/chasecupp43/contextkeeper-mcp.git
cd contextkeeper-mcp

# Build the project
dotnet build

# Run tests
dotnet test

# Run locally
dotnet run --project src/ContextKeeper
```

## License

MIT License - see [LICENSE](LICENSE) for details.

## Acknowledgments

- Inspired by LSM-tree data structures and immutable history patterns
- Built for the Claude MCP ecosystem
- Battle-tested on the CodeCartographerAI project

---

**Created by Chase Cupp** | [GitHub](https://github.com/chasecupp43) | [LinkedIn](https://linkedin.com/in/chasecupp)

*If you find ContextKeeper useful, please ⭐ the repository!*