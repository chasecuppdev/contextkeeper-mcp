# ContextKeeper Configuration Guide

## Configuration Overview

ContextKeeper supports multiple configuration methods, from zero-config auto-detection to fully customized workflows.

## Configuration Hierarchy

Configuration is resolved in this order (highest priority first):

1. **Command-line options** - `--profile` flag
2. **Environment variables** - `CONTEXTKEEPER_PROFILE`
3. **Local config file** - `contextkeeper.config.json`
4. **Auto-detection** - Based on project structure
5. **Built-in profiles** - Default workflows

## Auto-Detection

ContextKeeper automatically detects your project type:

### CLAUDE.md Projects
- **Detection**: Presence of `CLAUDE.md` file
- **Profile**: `claude-workflow`
- **History**: `FeatureData/DataHistory/`

### README Projects
- **Detection**: `README.md` with history sections
- **Profile**: `readme-workflow`
- **History**: `.history/`

### Documentation Projects
- **Detection**: `docs/` directory
- **Profile**: `docs-workflow`
- **History**: `docs/.history/`

## Configuration File

Create `contextkeeper.config.json` in your project root:

```json
{
  "version": "1.0",
  "defaultProfile": "my-custom-profile",
  "profiles": {
    "my-custom-profile": {
      "name": "my-custom-profile",
      "description": "Custom workflow for my project",
      "detection": {
        "files": ["ARCHITECTURE.md"],
        "paths": ["docs"]
      },
      "paths": {
        "history": "docs/history",
        "snapshots": "docs/history/snapshots",
        "compacted": "docs/history/compacted"
      },
      "snapshot": {
        "prefix": "ARCH_",
        "dateFormat": "yyyy-MM-dd",
        "filenamePattern": "{prefix}{date}_{milestone}.md",
        "validation": "^[a-z0-9-]+$",
        "maxLength": 50
      },
      "compaction": {
        "threshold": 15,
        "strategy": "quarterly",
        "archivePath": "Archive_{quarter}"
      },
      "header": {
        "template": "# Architecture Snapshot\\n**Date**: {date}\\n**Milestone**: {milestone}\\n\\n---\\n{content}"
      }
    }
  }
}
```

## Profile Structure

### Detection Configuration
```json
"detection": {
  "files": ["README.md", "ARCHITECTURE.md"],  // Files that must exist
  "paths": ["docs", "documentation"]           // Directories that must exist
}
```

### Path Configuration
```json
"paths": {
  "history": "path/to/history",          // Root history directory
  "snapshots": "path/to/snapshots",      // Where snapshots are stored
  "compacted": "path/to/compacted"       // Where compacted files go (optional)
}
```

### Snapshot Configuration
```json
"snapshot": {
  "prefix": "SNAPSHOT_",                 // Filename prefix
  "dateFormat": "yyyy-MM-dd",            // Date format pattern
  "filenamePattern": "{prefix}{date}_{milestone}.md",  // Full pattern
  "validation": "^[a-z0-9-]+$",          // Regex for milestone validation
  "maxLength": 50                        // Max milestone length
}
```

### Compaction Configuration
```json
"compaction": {
  "threshold": 10,                       // Number of snapshots before compaction
  "strategy": "quarterly",               // Compaction strategy
  "archivePath": "Archive_{quarter}"     // Archive directory pattern
}
```

### Header Template Configuration
```json
"header": {
  "template": "# {document} Snapshot\\n**Date**: {date}\\n..."
}
```

Template variables:
- `{document}` - Main document name
- `{date}` - Current date
- `{milestone}` - Milestone description
- `{previous}` - Previous snapshot filename
- `{status}` - Compaction status
- `{content}` - Original document content

## Environment Variables

### Profile Selection
```bash
export CONTEXTKEEPER_PROFILE=my-custom-profile
```

### Installation Directory
```bash
export CONTEXTKEEPER_INSTALL_DIR=/opt/contextkeeper
```

## Built-in Profiles

### claude-workflow
The default profile for CLAUDE.md based projects:
- LSM-tree inspired history management
- 10-snapshot compaction threshold
- Quarterly archival strategy

### readme-workflow
For traditional README-based projects:
- Hidden `.history` directory
- 20-snapshot compaction threshold
- Monthly archival strategy

### custom-template
A template for creating your own profiles:
- Example configuration
- All options documented
- Ready to customize

## Date Format Patterns

Common patterns for `dateFormat`:
- `yyyy-MM-dd` - 2024-01-15 (default)
- `yyyyMMdd` - 20240115
- `yyyy-MM-dd-HH-mm` - 2024-01-15-14-30
- `dd-MMM-yyyy` - 15-Jan-2024

## Validation Patterns

Common patterns for milestone `validation`:
- `^[a-z0-9-]+$` - Lowercase alphanumeric with hyphens (default)
- `^[a-zA-Z0-9-_]+$` - Alphanumeric with hyphens and underscores
- `^[a-z0-9-]{3,30}$` - Length-limited kebab-case
- `.*` - Allow anything (not recommended)

## Compaction Strategies

Available strategies:
- `lsm-quarterly` - LSM-tree inspired quarterly compaction
- `monthly` - Compact every month
- `quarterly` - Compact every quarter
- `yearly` - Compact every year
- `custom` - Define your own in code

## Examples

### Minimal Configuration
```json
{
  "defaultProfile": "readme-workflow"
}
```

### Multi-Project Configuration
```json
{
  "version": "1.0",
  "defaultProfile": "frontend",
  "profiles": {
    "frontend": {
      "name": "frontend",
      "paths": {
        "history": "frontend/.history",
        "snapshots": "frontend/.history/snapshots"
      }
    },
    "backend": {
      "name": "backend",
      "paths": {
        "history": "backend/.history",
        "snapshots": "backend/.history/snapshots"
      }
    }
  }
}
```

### CI/CD Friendly Configuration
```json
{
  "version": "1.0",
  "defaultProfile": "ci-workflow",
  "profiles": {
    "ci-workflow": {
      "name": "ci-workflow",
      "detection": {
        "files": [".github/workflows/build.yml"]
      },
      "paths": {
        "history": ".ci/history",
        "snapshots": ".ci/history/snapshots"
      },
      "snapshot": {
        "prefix": "CI_",
        "validation": "^[a-z0-9-]+$",
        "maxLength": 30
      }
    }
  }
}
```

## Best Practices

1. **Use Auto-Detection** - Let ContextKeeper detect your project type
2. **Customize Gradually** - Start with defaults, customize as needed
3. **Version Control Config** - Include `contextkeeper.config.json` in git
4. **Consistent Naming** - Use kebab-case for milestones
5. **Regular Compaction** - Monitor snapshot count with `contextkeeper check`

## Troubleshooting

### Profile Not Found
```bash
# List available profiles
contextkeeper init --list-profiles

# Force a specific profile
contextkeeper init --profile claude-workflow
```

### Invalid Configuration
```bash
# Validate configuration
contextkeeper validate-config

# Use default if config is broken
CONTEXTKEEPER_PROFILE=claude-workflow contextkeeper snapshot test
```

### Path Issues
- Use forward slashes even on Windows
- Paths are relative to project root
- Create directories automatically with `init`