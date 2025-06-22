using System.Text.Json;
using System.Text.Json.Nodes;
using ContextKeeper.Core;

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
    
    private async Task<JsonObject> HandleInitialize(JsonNode? parameters)
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
        
        return capabilities;
    }
    
    private JsonObject GetToolsList()
    {
        var tools = new JsonArray
        {
            new JsonObject
            {
                ["name"] = "create_snapshot",
                ["description"] = "Create a timestamped backup of your main document",
                ["inputSchema"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["milestone"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "Milestone description in kebab-case (e.g., 'feature-implementation')",
                            ["pattern"] = "^[a-z0-9-]+$",
                            ["maxLength"] = 50
                        }
                    },
                    ["required"] = new JsonArray { "milestone" }
                }
            },
            new JsonObject
            {
                ["name"] = "check_compaction",
                ["description"] = "Check if history compaction is needed based on snapshot count",
                ["inputSchema"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject { }
                }
            },
            new JsonObject
            {
                ["name"] = "search_history",
                ["description"] = "Search through all historical snapshots for specific content",
                ["inputSchema"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["searchTerm"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "Term to search for in history"
                        },
                        ["maxResults"] = new JsonObject
                        {
                            ["type"] = "integer",
                            ["description"] = "Maximum number of results to return",
                            ["default"] = 5
                        }
                    },
                    ["required"] = new JsonArray { "searchTerm" }
                }
            },
            new JsonObject
            {
                ["name"] = "get_evolution",
                ["description"] = "Track how a specific component evolved over time",
                ["inputSchema"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["componentName"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "Name of the component to track"
                        }
                    },
                    ["required"] = new JsonArray { "componentName" }
                }
            },
            new JsonObject
            {
                ["name"] = "compare_snapshots",
                ["description"] = "Compare two snapshots to see what changed",
                ["inputSchema"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["snapshot1"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "First snapshot filename"
                        },
                        ["snapshot2"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "Second snapshot filename"
                        }
                    },
                    ["required"] = new JsonArray { "snapshot1", "snapshot2" }
                }
            }
        };
        
        return new JsonObject
        {
            ["tools"] = tools
        };
    }
}