# ContextKeeper MCP Testing Guide

## Quick Start

1. **Run the test script**:
   ```bash
   ./test-mcp.sh
   ```

2. **Start Claude Code with MCP**:
   ```bash
   claude-code --mcp-config ./claude-code-config.json
   ```

3. **Verify MCP is working**:
   ```
   /mcp list
   ```

## Available MCP Tools

### 1. get_status
Get system status and configuration info.
```
/mcp call contextkeeper get_status
```

### 2. snapshot
Create a development context snapshot.
```
/mcp call contextkeeper snapshot {"milestone": "feature-complete"}
```

### 3. search_evolution
Natural language search through project history.
```
/mcp call contextkeeper search_evolution {"query": "when was authentication added", "maxResults": 10}
```

### 4. track_component
Track how a component evolved over time.
```
/mcp call contextkeeper track_component {"componentName": "SnapshotManager"}
```

### 5. compare_snapshots
Compare two snapshots to see changes.
```
/mcp call contextkeeper compare_snapshots {"snapshot1": "latest", "snapshot2": "phase3-complete"}
```

### 6. get_timeline
View chronological project timeline.
```
/mcp call contextkeeper get_timeline {"limit": 20}
```

## Natural Language Usage

Instead of direct tool calls, you can ask Claude naturally:

- "Use contextkeeper to create a snapshot called 'new-feature-start'"
- "Search the project history for information about refactoring"
- "Show me how the SearchEngine component evolved"
- "Compare the latest snapshot with yesterday's snapshot"
- "Display the project timeline"

## Testing Scenarios

### Basic Workflow Test
1. Create initial snapshot: `snapshot {"milestone": "test-start"}`
2. Make some code changes
3. Create another snapshot: `snapshot {"milestone": "test-changes"}`
4. Compare them: `compare_snapshots {"snapshot1": "test-start", "snapshot2": "test-changes"}`
5. Search for your changes: `search_evolution {"query": "test changes"}`

### Auto-Compaction Test
Create 21 snapshots rapidly to trigger auto-compaction:
```bash
for i in {1..21}; do
  echo "Creating snapshot $i"
  # Use MCP or CLI to create snapshot
done
```

### Context Capture Verification
1. Make uncommitted changes to files
2. Create a snapshot
3. Check that the snapshot includes:
   - Git uncommitted files list
   - Current branch and commit
   - Workspace documentation
   - User workspace contents

## Troubleshooting

### MCP Tools Not Appearing
1. Check Claude Code logs: `~/.claude-code/logs/`
2. Verify config path is absolute
3. Enable debug mode: `export MCP_DEBUG=true`
4. Restart Claude Code

### Tool Calls Failing
1. Check if ContextKeeper builds: `dotnet build`
2. Verify .contextkeeper directory exists
3. Test CLI directly: `dotnet run --project src/ContextKeeper -- check`
4. Look for error messages in terminal

### Common Error Messages

| Error | Solution |
|-------|----------|
| "Server not found" | Config path incorrect or not absolute |
| "Tool not recognized" | MCP server not properly registered |
| "Permission denied" | Check write permissions on .contextkeeper/ |
| "Snapshot failed" | Ensure git repository is initialized |

## Debug Commands

```bash
# Test MCP server directly
dotnet run --project src/ContextKeeper

# Check configuration
cat claude-code-config.json

# Verify directories
ls -la .contextkeeper/
ls -la context-workspace/

# Check for snapshots
ls -la .contextkeeper/snapshots/

# View latest snapshot
ls -t .contextkeeper/snapshots/ | head -1
```

## Expected Results

✅ **Successful MCP Registration**:
- `/mcp list` shows "contextkeeper" with 6 tools
- No errors in Claude Code startup

✅ **Successful Tool Calls**:
- Each tool returns JSON response
- No error messages
- Results match expected format

✅ **Successful Snapshot Creation**:
- New file in .contextkeeper/snapshots/
- Filename format: SNAPSHOT_YYYY-MM-DD_manual_milestone.md
- Contains git state, workspace, and documentation

✅ **Successful Search**:
- Returns relevant matches
- Includes AI interpretation
- Shows file snippets

## Next Steps

After successful testing:
1. Use ContextKeeper in your actual development workflow
2. Create snapshots at key milestones
3. Search history when you need context
4. Let auto-compaction manage storage

For issues or questions, check:
- This guide
- CLAUDE.md for architecture details
- README.md for general usage
- Run tests: `dotnet test`