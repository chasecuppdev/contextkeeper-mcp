# ContextKeeper MCP Demo Script

## Setup Complete! ✅

The ContextKeeper MCP server is now configured with Claude Code. Here's how to demo it:

## 1. Start Claude Code with MCP Support

Claude Code should now see the ContextKeeper MCP server. When you start a new conversation, you'll have access to these tools:

### Available MCP Tools:

#### 📸 **snapshot** - Create development context snapshots
```
Example: "Create a snapshot called 'demo-ready'"
```

#### 🔍 **search_evolution** - Natural language search through history
```
Example: "When did we add the MCP integration?"
Example: "Search for authentication implementation"
```

#### 📊 **track_component** - Track feature evolution
```
Example: "Track the evolution of the SnapshotManager"
Example: "Show me how the compaction engine evolved"
```

#### 🔄 **compare_snapshots** - Compare two snapshots
```
Example: "Compare the latest snapshot with yesterday's"
```

#### 📈 **get_status** - System status
```
Example: "Show me the current ContextKeeper status"
```

#### 📅 **get_timeline** - Project timeline
```
Example: "Show me the project timeline"
```

### C# Code Analysis Tools:

#### 🔎 **FindSymbolDefinitions** - Find symbol declarations
```
Example: "Find the definition of ISnapshotManager"
```

#### 📍 **FindSymbolReferences** - Find all usages
```
Example: "Find all references to ContextKeeperService"
```

#### 🌳 **NavigateInheritanceHierarchy** - Explore type relationships
```
Example: "Show the inheritance hierarchy for McpServerToolType"
```

#### 🔤 **SearchSymbolsByPattern** - Wildcard pattern matching
```
Example: "Find all classes ending with *Service"
```

#### 📚 **GetSymbolDocumentation** - Extract XML documentation
```
Example: "Get documentation for the Snapshot method"
```

## 2. Quick Demo Flow (10 minutes)

### Part 1: Context Capture (2 min)
1. "Show me the current ContextKeeper status"
2. "Create a snapshot called 'interview-demo'"
3. "Show me the project timeline"

### Part 2: History Search (3 min)
1. "When was the MCP integration added to this project?"
2. "Search for compaction engine implementation"
3. "Track how the SnapshotManager evolved"

### Part 3: Code Intelligence (3 min)
1. "Find the definition of IContextKeeperService"
2. "Find all classes that implement MCP tools"
3. "Show me all *Manager classes in the codebase"

### Part 4: Real-World Use Case (2 min)
1. "Compare the latest snapshot with the previous one"
2. "When did we switch from manual JSON to source-generated serialization?"
3. "Track the evolution of the storage architecture"

## 3. Key Talking Points

### The Problem It Solves
- AI assistants lose context between sessions
- Developers waste time re-explaining project history
- Difficult to track when/why architectural decisions were made

### ContextKeeper Benefits
- **Perfect Memory**: AI never forgets your project's evolution
- **Natural Language**: Ask "when did we add X?" and get instant answers
- **Zero Config**: Auto-detects project type (CLAUDE.md vs README.md)
- **Git Integration**: Automatic snapshots on commits/branch switches
- **LSM-tree Storage**: Efficient compaction keeps history manageable

### Technical Highlights
- **.NET 9 Native AOT**: 12ms startup, 41MB binary
- **MCP Protocol**: Standard integration with any AI assistant
- **Roslyn Integration**: Deep C# code analysis
- **98 Tests Passing**: Comprehensive test coverage
- **Production Ready**: Extracted from CodeCartographerAI

## 4. Troubleshooting

If Claude Code doesn't see the MCP server:
1. Restart Claude Code
2. Check the config file: `~/.config/claude-code/mcp-servers.json`
3. Verify the server runs: `dotnet run --project src/ContextKeeper`

## 5. Quick Commands Reference

```bash
# Manual snapshot
dotnet run --project src/ContextKeeper -- snapshot "milestone-name"

# Search history
dotnet run --project src/ContextKeeper -- search "keyword"

# Check status
dotnet run --project src/ContextKeeper -- check

# Initialize in a new project
dotnet run --project src/ContextKeeper -- init --git-hooks
```

## Good luck with your interview! 🚀

Remember: ContextKeeper is about giving AI assistants perfect memory of your project's evolution. Focus on how it solves real development pain points!