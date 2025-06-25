using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using ContextKeeper.Core;
using ContextKeeper.Core.Interfaces;
using ContextKeeper.Json;

namespace ContextKeeper.Protocol;

[McpServerToolType]
public class ContextKeeperMcpTools
{
    private readonly IContextKeeperService _service;
    private readonly ILogger<ContextKeeperMcpTools> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ContextKeeperMcpTools(IContextKeeperService service, ILogger<ContextKeeperMcpTools> logger)
    {
        _service = service;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    [McpServerTool]
    [Description("Create a comprehensive development context snapshot capturing git state, workspace, and documentation")]
    public async Task<string> Snapshot(
        [Description("Milestone description in kebab-case (e.g., 'feature-implementation')")] string milestone,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _service.CreateSnapshot(milestone);
            return result.ToJsonString(_jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating snapshot");
            #pragma warning disable IL2026, IL3050
            var jsonNode = JsonSerializer.SerializeToNode(new { success = false, error = ex.Message });
            #pragma warning restore IL2026, IL3050
            return jsonNode?.ToJsonString(_jsonOptions) ?? "{}";
        }
    }

    [McpServerTool]
    [Description("Search through your codebase evolution history with natural language")]
    public async Task<string> SearchEvolution(
        [Description("Natural language query about your project's evolution (e.g., 'when was authentication added?')")] string query,
        [Description("Maximum number of results to return")] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, use keyword search. In future, this could use AI to interpret the query
            var keywords = ExtractKeywords(query);
            var result = await _service.SearchHistory(string.Join(" ", keywords), maxResults);
            
            // Enhance result with AI-style summary
            var jsonResult = JsonNode.Parse(result.ToJsonString()) as JsonObject;
            if (jsonResult != null)
            {
                jsonResult["query"] = query;
                jsonResult["interpretation"] = $"Searching for evolution of: {string.Join(", ", keywords)}";
                jsonResult["suggestion"] = "Use 'track_component' for detailed component evolution or 'compare_periods' for time-based analysis";
            }
            
            return jsonResult?.ToJsonString(_jsonOptions) ?? result.ToJsonString(_jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching evolution");
            #pragma warning disable IL2026, IL3050
            var jsonNode = JsonSerializer.SerializeToNode(new { success = false, error = ex.Message });
            #pragma warning restore IL2026, IL3050
            return jsonNode?.ToJsonString(_jsonOptions) ?? "{}";
        }
    }

    [McpServerTool]
    [Description("Track how a specific component or feature evolved through your codebase history")]
    public async Task<string> TrackComponent(
        [Description("Name of the component, feature, or architectural pattern to track")] string componentName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _service.GetArchitecturalEvolution(componentName);
            
            // Enhance with AI-style insights
            var jsonResult = JsonNode.Parse(result.ToJsonString()) as JsonObject;
            if (jsonResult != null)
            {
                var steps = jsonResult["steps"] as JsonArray;
                if (steps != null && steps.Count > 0)
                {
                    var insights = new JsonArray();
                    
                    // Add evolution insights
                    if (steps.Count == 0)
                    {
                        insights.Add(JsonValue.Create($"{componentName} not found in history - it may be a new component or use different naming"));
                    }
                    else if (steps.Count == 1)
                    {
                        insights.Add(JsonValue.Create($"{componentName} was introduced once and hasn't changed significantly"));
                    }
                    else
                    {
                        insights.Add(JsonValue.Create($"{componentName} evolved through {steps.Count} documented changes"));
                        insights.Add(JsonValue.Create($"First appearance: {steps[0]?["date"]}"));
                        insights.Add(JsonValue.Create($"Latest update: {steps[steps.Count - 1]?["date"]}"));
                    }
                    
                    jsonResult["insights"] = insights;
                    jsonResult["recommendation"] = "Use 'compare_snapshots' to see detailed changes between any two evolution steps";
                }
            }
            
            return jsonResult?.ToJsonString(_jsonOptions) ?? result.ToJsonString(_jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking component");
            #pragma warning disable IL2026, IL3050
            var jsonNode = JsonSerializer.SerializeToNode(new { success = false, error = ex.Message });
            #pragma warning restore IL2026, IL3050
            return jsonNode?.ToJsonString(_jsonOptions) ?? "{}";
        }
    }

    [McpServerTool]
    [Description("Compare two snapshots to understand what changed between them")]
    public async Task<string> CompareSnapshots(
        [Description("First snapshot filename or 'latest' for most recent")] string snapshot1,
        [Description("Second snapshot filename or 'latest' for most recent")] string snapshot2,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Handle 'latest' keyword
            if (snapshot1.ToLower() == "latest" || snapshot2.ToLower() == "latest")
            {
                var checkResult = await _service.CheckCompactionNeeded();
                var newestSnapshot = checkResult["newestSnapshot"]?.GetValue<string>();
                if (newestSnapshot != null)
                {
                    var filename = Path.GetFileName(newestSnapshot);
                    if (snapshot1.ToLower() == "latest") snapshot1 = filename;
                    if (snapshot2.ToLower() == "latest") snapshot2 = filename;
                }
            }
            
            var result = await _service.CompareSnapshots(snapshot1, snapshot2);
            return result.ToJsonString(_jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing snapshots");
            #pragma warning disable IL2026, IL3050
            var jsonNode = JsonSerializer.SerializeToNode(new { success = false, error = ex.Message });
            #pragma warning restore IL2026, IL3050
            return jsonNode?.ToJsonString(_jsonOptions) ?? "{}";
        }
    }

    [McpServerTool]
    [Description("Get the status of your development context including snapshots, compaction, and timeline")]
    public async Task<string> GetStatus(CancellationToken cancellationToken = default)
    {
        try
        {
            var compactionStatus = await _service.CheckCompactionNeeded();
            
            // Build comprehensive status
            var status = new JsonObject
            {
                ["contextkeeper_version"] = "2.0",
                ["snapshot_status"] = compactionStatus,
                ["auto_features"] = new JsonObject
                {
                    ["git_hooks_available"] = true,
                    ["auto_compaction"] = compactionStatus["autoCompactEnabled"] ?? true,
                    ["context_tracking"] = new JsonObject
                    {
                        ["git_state"] = true,
                        ["workspace"] = true,
                        ["documentation"] = true
                    }
                },
                ["quick_actions"] = new JsonArray
                {
                    "Use 'snapshot' to capture current state",
                    "Use 'search_evolution' to explore history",
                    "Use 'track_component' to follow feature evolution",
                    "Use 'timeline' to see chronological development"
                }
            };
            
            return status.ToJsonString(_jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status");
            #pragma warning disable IL2026, IL3050
            var jsonNode = JsonSerializer.SerializeToNode(new { success = false, error = ex.Message });
            #pragma warning restore IL2026, IL3050
            return jsonNode?.ToJsonString(_jsonOptions) ?? "{}";
        }
    }

    [McpServerTool]
    [Description("Get a chronological timeline of your project's development")]
    public async Task<string> GetTimeline(
        [Description("Maximum number of events to return")] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, return snapshot list as timeline
            // In future, this could include git commits, major changes, etc.
            var searchResult = await _service.SearchHistory("", limit);
            
            var timeline = new JsonObject
            {
                ["timeline_type"] = "development_evolution",
                ["events"] = new JsonArray()
            };
            
            var events = timeline["events"] as JsonArray;
            var matches = searchResult["matches"] as JsonArray;
            
            if (matches != null)
            {
                foreach (var match in matches)
                {
                    var filename = match?["fileName"]?.GetValue<string>() ?? "";
                    var parts = filename.Split('_');
                    
                    var eventObj = new JsonObject
                    {
                        ["date"] = parts.Length > 1 ? parts[1] : "unknown",
                        ["type"] = parts.Length > 2 ? parts[2] : "snapshot",
                        ["milestone"] = parts.Length > 3 ? parts[3]?.Replace(".md", "") : "unknown",
                        ["file"] = filename
                    };
                    
                    events?.Add(eventObj);
                }
            }
            
            timeline["total_events"] = events?.Count ?? 0;
            timeline["insight"] = "Your project has evolved through multiple documented milestones. Use 'track_component' to follow specific features.";
            
            return timeline.ToJsonString(_jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting timeline");
            #pragma warning disable IL2026, IL3050
            var jsonNode = JsonSerializer.SerializeToNode(new { success = false, error = ex.Message });
            #pragma warning restore IL2026, IL3050
            return jsonNode?.ToJsonString(_jsonOptions) ?? "{}";
        }
    }
    
    private List<string> ExtractKeywords(string query)
    {
        // Simple keyword extraction - in future could use NLP
        var stopWords = new HashSet<string> { "the", "was", "is", "are", "when", "how", "what", "where", "did", "does", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for" };
        
        return query.ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(word => !stopWords.Contains(word) && word.Length > 2)
            .ToList();
    }
}