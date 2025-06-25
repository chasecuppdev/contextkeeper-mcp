# ContextKeeper Quick Start Guide

Get ContextKeeper running with your AI assistant in under 5 minutes!

## 1. Install and Build

```bash
# Clone and build
git clone https://github.com/yourusername/contextkeeper-mcp.git
cd contextkeeper-mcp
dotnet build
```

## 2. Configure Claude Desktop

Add to your Claude Desktop config file:
- **macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`
- **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`

```json
{
  "mcpServers": {
    "contextkeeper": {
      "command": "dotnet",
      "args": ["run", "--project", "/full/path/to/contextkeeper-mcp/src/ContextKeeper"],
      "cwd": "/path/to/your/project"
    }
  }
}
```

## 3. Initialize Your Project

In your project directory:

```bash
# Basic initialization
dotnet run --project /path/to/contextkeeper-mcp/src/ContextKeeper -- init

# With git hooks for automatic capture
dotnet run --project /path/to/contextkeeper-mcp/src/ContextKeeper -- init --git-hooks
```

## 4. Start Using with Claude

Restart Claude Desktop and you'll have access to these MCP tools:

### Create a Snapshot
```
Use the snapshot tool to create a milestone "feature-complete"
```

### Search Your History
```
Use the search_evolution tool to find "when was authentication added?"
```

### Track Component Evolution
```
Use the track_component tool to see how "UserService" evolved
```

### Check System Status
```
Use the get_status tool to check compaction status
```

## 5. CLI Commands

For manual operations:

```bash
# Create snapshot
dotnet run --project /path/to/contextkeeper -- snapshot "milestone-name"

# Search history
dotnet run --project /path/to/contextkeeper -- search "search term"

# Check status
dotnet run --project /path/to/contextkeeper -- check
```

## What's Next?

- **Automatic Captures**: Git hooks create snapshots on commits and branch switches
- **Natural Language**: Ask questions like "what changed in the last week?"
- **Perfect Memory**: Your AI assistant remembers everything between sessions

## Troubleshooting

### "Command not found"
Ensure you're using the full path to the ContextKeeper project in your Claude config.

### "No snapshots found"
Create your first snapshot: `dotnet run -- snapshot "initial"`

### Git hooks not working
Ensure `.git/hooks/pre-commit` and `.git/hooks/post-checkout` are executable:
```bash
chmod +x .git/hooks/pre-commit .git/hooks/post-checkout
```

## Example Session with Claude

```
You: Create a snapshot for the authentication feature I just completed

Claude: I'll create a snapshot for your authentication feature completion.
[Uses snapshot tool with milestone "authentication-complete"]
✓ Snapshot created successfully

You: When did we first start working on user authentication?

Claude: Let me search through the project history for authentication work.
[Uses search_evolution tool with query "authentication"]
Based on the history, authentication work started on January 20, 2024...
```

---

**Need help?** Check the full [README](README.md) or open an [issue](https://github.com/yourusername/contextkeeper-mcp/issues).