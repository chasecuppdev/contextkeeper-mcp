# C# Code Search with ContextKeeper MCP

ContextKeeper now includes powerful C# code search capabilities powered by Roslyn, the .NET compiler platform. This allows you to search for symbols, find references, navigate inheritance hierarchies, and more across your C# solutions and projects.

## Features

### Code Search Tools

1. **FindSymbolDefinitions** - Search for symbol definitions by name
   - Supports classes, methods, properties, fields, interfaces, and namespaces
   - Case-insensitive search
   - Symbol type filtering

2. **FindSymbolReferences** - Find all references to a specific symbol
   - Tracks usage across the entire codebase
   - Shows line numbers and preview of usage

3. **NavigateInheritanceHierarchy** - Explore type inheritance
   - View base types and derived types
   - Find interface implementations
   - Navigate complete inheritance chains

4. **SearchSymbolsByPattern** - Pattern-based symbol search
   - Supports wildcards (* and ?)
   - Filter by symbol kinds
   - Namespace filtering

5. **GetSymbolDocumentation** - Extract XML documentation
   - Summary and remarks
   - Parameter documentation
   - Return value information

## Setup with Claude Code

### 1. Install ContextKeeper

First, ensure ContextKeeper is built:

```bash
cd contextkeeper-mcp
dotnet build
```

### 2. Configure Claude Code

Add ContextKeeper to your Claude Code configuration:

```bash
# Add with project scope
claude mcp add contextkeeper -s project dotnet run --project ./src/ContextKeeper/ContextKeeper.csproj

# Or add with user scope
claude mcp add contextkeeper -s user dotnet run --project /absolute/path/to/contextkeeper-mcp/src/ContextKeeper/ContextKeeper.csproj
```

### 3. Manual Configuration (Alternative)

Edit `~/.claude/config/claude.json`:

```json
{
  "mcpServers": {
    "contextkeeper": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/contextkeeper-mcp/src/ContextKeeper/ContextKeeper.csproj"],
      "env": {
        "DOTNET_CLI_TELEMETRY_OPTOUT": "1"
      }
    }
  }
}
```

## Usage Examples

Once configured, you can use these queries in Claude:

### Finding Symbol Definitions

"Find all classes that implement IDisposable in my solution"
```
FindSymbolDefinitions /path/to/solution.sln "IDisposable" symbolKind="Interface"
```

"Show me all methods named ProcessData"
```
FindSymbolDefinitions /path/to/project.csproj "ProcessData" symbolKind="Method"
```

### Finding References

"Show me all references to the UserService class"
```
FindSymbolReferences /path/to/solution.sln "UserService"
```

"Find where the Initialize method is called"
```
FindSymbolReferences /path/to/solution.sln "Initialize" containingType="ApplicationManager"
```

### Exploring Inheritance

"What classes inherit from BaseController?"
```
NavigateInheritanceHierarchy /path/to/solution.sln "BaseController" includeDerivedTypes=true
```

"Show me the inheritance hierarchy for IUserService"
```
NavigateInheritanceHierarchy /path/to/solution.sln "IUserService" includeImplementations=true
```

### Pattern-Based Search

"Find all services in the application"
```
SearchSymbolsByPattern /path/to/solution.sln "*Service" symbolKinds="Class,Interface"
```

"Search for all Get methods in the Controllers namespace"
```
SearchSymbolsByPattern /path/to/solution.sln "Get*" symbolKinds="Method" namespaceFilter="Controllers"
```

### Getting Documentation

"Show me the documentation for ILogger"
```
GetSymbolDocumentation /path/to/solution.sln "ILogger"
```

## Supported File Types

- Solution files (`.sln`)
- C# project files (`.csproj`)
- Both are supported for all search operations

## Performance Considerations

1. **First Load**: The initial loading of a solution may take time as Roslyn builds the semantic model
2. **Caching**: Solutions are cached in memory for faster subsequent searches
3. **Large Solutions**: For very large solutions, consider searching at the project level when possible

## Troubleshooting

### MSBuild Registration

If you encounter MSBuild-related errors, ensure:
1. You have the .NET SDK installed
2. MSBuildLocator can find your MSBuild installation

### Symbol Not Found

If symbols aren't found:
1. Ensure the solution/project builds successfully
2. Check that all project references are resolved
3. Verify the symbol name and casing (though search is case-insensitive by default)

### Performance Issues

For better performance:
1. Use project files instead of solution files when searching within a specific project
2. Use specific symbol filters to narrow search scope
3. Limit the number of results with maxResults parameter

## Integration with ContextKeeper Features

The C# code search tools integrate seamlessly with ContextKeeper's existing features:

- **Snapshots**: Document your code exploration findings
- **Search History**: Search through previous code analysis sessions
- **Evolution Tracking**: Track how specific components evolved over time
- **Comparison**: Compare code structure between different snapshots

## Future Enhancements

Planned improvements include:
- Semantic code search (find similar code patterns)
- Call graph visualization
- Dependency analysis
- Refactoring suggestions
- Code metrics and complexity analysis

---

For more information about ContextKeeper, see the main [README](../README.md).