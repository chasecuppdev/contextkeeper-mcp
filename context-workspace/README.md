# ContextKeeper Workspace

This directory provides organized spaces for both user documentation and project history.

## Directory Structure

```
context-workspace/
├── workspace/          # Your active working files (accessible via @ in Claude)
│   ├── requirements/   # Project requirements and specifications
│   ├── design/        # Design decisions and architecture notes
│   ├── instructions/  # Custom AI instructions and prompts
│   └── README.md      # Detailed workspace usage guide
└── project-history/   # ContextKeeper project development history
    ├── roadmap/       # Project roadmaps and planning docs
    ├── development/   # Development phase summaries
    └── archive/       # Archived documentation
```

## Purpose

This structure serves two key purposes:

1. **User Workspace**: A flexible area for your custom documentation that:
   - Is visible to Claude Code via the @ symbol
   - Gets automatically included in context snapshots
   - Provides organized subdirectories for different content types

2. **Project History**: Keeps ContextKeeper's own development docs organized and out of the root directory

## Quick Start

1. Add your custom documentation to the `workspace/` subdirectories
2. Reference files using @ in Claude (e.g., `@context-workspace/workspace/requirements/api-spec.md`)
3. Files are automatically included in ContextKeeper snapshots

See `workspace/README.md` for detailed usage instructions.