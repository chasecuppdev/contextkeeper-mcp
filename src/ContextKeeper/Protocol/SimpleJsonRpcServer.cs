using System.Text.Json;
using System.Text.Json.Nodes;
using ContextKeeper.Core;
using ContextKeeper.Json;

namespace ContextKeeper.Protocol;

public class SimpleJsonRpcServer
{
    private readonly ContextKeeperService _service;
    
    public SimpleJsonRpcServer(ContextKeeperService service)
    {
        _service = service;
    }
    
    public async Task RunAsync()
    {
        string? line;
        while ((line = await Console.In.ReadLineAsync()) != null)
        {
            try
            {
                var request = JsonNode.Parse(line);
                if (request == null) continue;
                
                var response = await ProcessRequestAsync(request);
                Console.WriteLine(response.ToJsonString());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error processing request: {ex.Message}");
                
                var errorResponse = new JsonObject
                {
                    ["jsonrpc"] = "2.0",
                    ["error"] = new JsonObject
                    {
                        ["code"] = -32603,
                        ["message"] = "Internal error",
                        ["data"] = ex.Message
                    }
                };
                
                Console.WriteLine(errorResponse.ToJsonString());
            }
        }
    }
    
    private async Task<JsonObject> ProcessRequestAsync(JsonNode request)
    {
        var method = request["method"]?.GetValue<string>();
        var parameters = request["params"];
        var id = request["id"];
        
        JsonObject result;
        
        switch (method)
        {
            case "initialize":
                result = await HandleInitialize(parameters);
                break;
                
            case "create_snapshot":
                var milestone = parameters?["milestone"]?.GetValue<string>() ?? "";
                result = await _service.CreateSnapshot(milestone);
                break;
                
            case "check_compaction":
                result = await _service.CheckCompactionNeeded();
                break;
                
            case "search_history":
                var searchTerm = parameters?["searchTerm"]?.GetValue<string>() ?? "";
                var maxResults = parameters?["maxResults"]?.GetValue<int>() ?? 5;
                result = await _service.SearchHistory(searchTerm, maxResults);
                break;
                
            case "get_evolution":
                var componentName = parameters?["componentName"]?.GetValue<string>() ?? "";
                result = await _service.GetArchitecturalEvolution(componentName);
                break;
                
            case "compare_snapshots":
                var snapshot1 = parameters?["snapshot1"]?.GetValue<string>() ?? "";
                var snapshot2 = parameters?["snapshot2"]?.GetValue<string>() ?? "";
                result = await _service.CompareSnapshots(snapshot1, snapshot2);
                break;
                
            case "tools/list":
                result = GetToolsList();
                break;
                
            default:
                return new JsonObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = id,
                    ["error"] = new JsonObject
                    {
                        ["code"] = -32601,
                        ["message"] = $"Method not found: {method}"
                    }
                };
        }
        
        return new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id,
            ["result"] = result
        };
    }
    
    private Task<JsonObject> HandleInitialize(JsonNode? parameters)
    {
        var capabilities = new JsonObject
        {
            ["name"] = "ContextKeeper",
            ["version"] = "1.0.0",
            ["vendor"] = "contextkeeper-mcp",
            ["capabilities"] = new JsonObject
            {
                ["tools"] = new JsonObject { }
            }
        };
        
        return Task.FromResult(capabilities);
    }
    
    private JsonObject GetToolsList()
    {
        var toolDefinitions = new object[]
        {
            new
            {
                name = "create_snapshot",
                description = "Create a timestamped backup of your main document",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        milestone = new
                        {
                            type = "string",
                            description = "Milestone description in kebab-case (e.g., 'feature-implementation')",
                            pattern = "^[a-z0-9-]+$",
                            maxLength = 50
                        }
                    },
                    required = new[] { "milestone" }
                }
            },
            new
            {
                name = "check_compaction",
                description = "Check if history compaction is needed based on snapshot count",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "search_history",
                description = "Search through all historical snapshots for specific content",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        searchTerm = new
                        {
                            type = "string",
                            description = "Term to search for in history"
                        },
                        maxResults = new
                        {
                            type = "integer",
                            description = "Maximum number of results to return",
                            @default = 5
                        }
                    },
                    required = new[] { "searchTerm" }
                }
            },
            new
            {
                name = "get_evolution",
                description = "Track how a specific component evolved over time",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        componentName = new
                        {
                            type = "string",
                            description = "Name of the component to track"
                        }
                    },
                    required = new[] { "componentName" }
                }
            },
            new
            {
                name = "compare_snapshots",
                description = "Compare two snapshots to see what changed",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        snapshot1 = new
                        {
                            type = "string",
                            description = "First snapshot filename"
                        },
                        snapshot2 = new
                        {
                            type = "string",
                            description = "Second snapshot filename"
                        }
                    },
                    required = new[] { "snapshot1", "snapshot2" }
                }
            }
        };
        
#pragma warning disable IL2026, IL3050 // Suppress AOT warnings for anonymous types
        var tools = JsonSerializer.SerializeToNode(toolDefinitions, ContextKeeperJsonContext.Default.Options) as JsonArray ?? new JsonArray();
#pragma warning restore IL2026, IL3050
        
        return new JsonObject
        {
            ["tools"] = tools
        };
    }
}