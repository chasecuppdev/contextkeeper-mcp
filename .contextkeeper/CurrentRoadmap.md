ContextKeeper Product Evolution Plan

  Phase 1: Simplification & Focus

  Remove complexity to create a lean, focused product

  1.1 Remove Multiple Workflow Profiles

  - What: Eliminate the claude-workflow vs readme-workflow distinction
  - Why: Users don't care about this complexity; they want it to "just work"
  - Action:
    - Remove ProfileDetector service
    - Single workflow that auto-adapts to project type
    - Store all snapshots in .contextkeeper/snapshots/
    - Use intelligent naming: SNAPSHOT_2025-06-24_<type>_<milestone>.md

  1.2 Simplify Compaction Strategy

  - What: Hide LSM-tree complexity from users
  - Why: Technical implementation details shouldn't be user-facing
  - Action:
    - Remove manual compaction commands
    - Auto-compact based on snapshot count/age
    - No quarterly/yearly distinction - just "archived" folder
    - Keep the LSM efficiency but make it invisible

  1.3 Remove Manual Snapshot Commands

  - What: Eliminate snapshot <milestone-name> command
  - Why: DevContext saves 2.1 hours/day through automatic capture
  - Action:
    - Auto-snapshot on git events (commits, branch switches)
    - Keep snapshots lightweight and frequent
    - Add snapshot metadata (branch, commit hash, timestamp)

  1.4 Deprecate File-Type Specific Logic

  - What: Remove hard-coded CLAUDE.md/README.md focus
  - Why: Too limiting for broader market appeal
  - Action:
    - Implement generic "context file" detection
    - Support multiple documentation formats
    - Let users configure what to track

  Phase 2: High-Impact Features (Ordered by Market Fit)

  2.1 🎯 Expand Context Capture (Highest Impact)

  - Why: DevContext proves capturing full development state has massive value ($174/day saved)
  - What: Track more than just documentation
  - Implementation:
  {
    "snapshot": {
      "timestamp": "2025-06-24T10:30:00Z",
      "context": {
        "open_files": ["src/main.cs", "tests/test.cs"],
        "cursor_positions": {...},
        "active_file": "src/main.cs:142",
        "git_state": {
          "branch": "feature/mcp-integration",
          "commit": "abc123",
          "uncommitted_changes": []
        },
        "recent_commands": ["dotnet test", "git status"],
        "documentation": {
          "CLAUDE.md": "...",
          "README.md": "..."
        }
      }
    }
  }

  2.2 🎯 Git Integration & Auto-Capture

  - Why: Seamless integration = adoption. Manual steps = abandonment
  - What: Automatic context capture on git events
  - Implementation:
    - Git hooks for pre-commit/post-checkout snapshots
    - Track which files changed between snapshots
    - Link snapshots to commits/PRs
    - Command: contextkeeper init sets up git hooks

  2.3 🎯 AI-Powered Evolution Insights

  - Why: Unique value prop - no other MCP server offers codebase time-travel with AI analysis
  - What: Natural language queries about project evolution
  - Implementation:
    - "What changed in authentication since v2.0?"
    - "Show me how the API evolved this quarter"
    - "When did we introduce the caching layer?"
    - Leverage existing SearchEngine with AI summarization

  2.4 🎯 Visual Timeline Interface

  - Why: DevContext's visual timeline is a key selling point
  - What: Web-based timeline visualization
  - Implementation:
    - Local web server: contextkeeper timeline
    - Interactive timeline showing all snapshots
    - Click to see state at any point
    - Diff view between any two points
    - Export timeline as markdown/HTML

  2.5 Context Recovery ("Time Travel")

  - Why: Unique feature leveraging your LSM-tree architecture
  - What: Restore complete development context from any snapshot
  - Implementation:
    - contextkeeper restore <snapshot-id>
    - Opens files in editor
    - Checks out git branch/commit
    - Restores terminal state
    - Shows documentation from that time

  2.6 IDE Extensions

  - Why: Meet developers where they work
  - What: VS Code extension (priority) + JetBrains
  - Implementation:
    - Sidebar showing snapshot timeline
    - Quick snapshot creation
    - Context restoration
    - Inline evolution hints ("This function changed 5 times")

  2.7 MCP Tool Enhancement

  - Why: Differentiate in the MCP marketplace
  - What: Unique MCP tools leveraging history
  - Implementation:
    - compare_evolution: Compare component across time
    - find_introduction: When was X introduced?
    - track_decisions: Show architectural decision history
    - context_at_time: Get full context from specific date

  2.8 Team Collaboration Features

  - Why: Enterprise adoption requires team features
  - What: Shared context across team
  - Implementation:
    - .contextkeeper/team/ for shared snapshots
    - Merge team members' contexts
    - "Who worked on what when" tracking
    - Context handoff for code reviews

  2.9 Export/Integration Capabilities

  - Why: Fit into existing workflows
  - What: Export to various formats
  - Implementation:
    - Export to Confluence/Notion
    - Generate architecture decision records (ADRs)
    - Create changelog from snapshot diffs
    - API for CI/CD integration

  2.10 TypeScript/Node Implementation

  - Why: Broader adoption in MCP ecosystem
  - What: Parallel implementation or full port
  - Implementation:
    - Start with TypeScript MCP server wrapper
    - Keep C# core for performance
    - Or full TypeScript rewrite
    - Market as "Enterprise MCP server"

  Phase 3: Market Positioning

  3.1 Update Core Messaging

  - From: "AI-powered development context management with LSM-tree inspired history tracking"
  - To: "The MCP server with perfect memory. Time-travel through your codebase evolution."

  3.2 Create Compelling Demos

  1. Time-travel demo: Show restoring exact development state from 6 months ago
  2. Evolution demo: "How did our auth system evolve?" with AI insights
  3. Context handoff: Developer A → Developer B seamless transition
  4. Bug archaeology: "When did this bug get introduced?"

  3.3 Target Use Cases

  1. Onboarding: New developers understand codebase evolution
  2. Debugging: "What changed that broke this?"
  3. Architecture: Track architectural decisions over time
  4. Compliance: Audit trail of all changes
  5. AI Training: Give AI assistants long-term memory

  Implementation Priority Summary

  Must Have (MVP):
  1. Simplify to single workflow
  2. Expand context capture beyond docs
  3. Git integration with auto-capture
  4. AI-powered evolution insights

  Should Have (v1.0):
  5. Visual timeline interface
  6. Context recovery/time travel
  7. Enhanced MCP tools

  Nice to Have (Future):
  8. IDE extensions
  9. Team features
  10. Export capabilities
  11. TypeScript version

  This plan focuses on your unique value proposition (LSM-tree based evolution tracking) while expanding appeal
  through broader context capture and AI-powered insights. The phased approach ensures you ship value quickly
  while building toward a comprehensive solution.