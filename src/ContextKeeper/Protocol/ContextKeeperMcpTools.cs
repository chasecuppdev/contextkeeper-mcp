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
    [Description("Create a timestamped backup of your main document")]
    public async Task<string> CreateSnapshot(
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
    [Description("Check if history compaction is needed based on snapshot count")]
    public async Task<string> CheckCompaction(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _service.CheckCompactionNeeded();
            return result.ToJsonString(_jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking compaction");
            #pragma warning disable IL2026, IL3050
            var jsonNode = JsonSerializer.SerializeToNode(new { success = false, error = ex.Message });
            #pragma warning restore IL2026, IL3050
            return jsonNode?.ToJsonString(_jsonOptions) ?? "{}";
        }
    }

    [McpServerTool]
    [Description("Search through all historical snapshots for specific content")]
    public async Task<string> SearchHistory(
        [Description("Term to search for in history")] string searchTerm,
        [Description("Maximum number of results to return")] int maxResults = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _service.SearchHistory(searchTerm, maxResults);
            return result.ToJsonString(_jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching history");
            #pragma warning disable IL2026, IL3050
            var jsonNode = JsonSerializer.SerializeToNode(new { success = false, error = ex.Message });
            #pragma warning restore IL2026, IL3050
            return jsonNode?.ToJsonString(_jsonOptions) ?? "{}";
        }
    }

    [McpServerTool]
    [Description("Track how a specific component evolved over time")]
    public async Task<string> GetEvolution(
        [Description("Name of the component to track")] string componentName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _service.GetArchitecturalEvolution(componentName);
            return result.ToJsonString(_jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting evolution");
            #pragma warning disable IL2026, IL3050
            var jsonNode = JsonSerializer.SerializeToNode(new { success = false, error = ex.Message });
            #pragma warning restore IL2026, IL3050
            return jsonNode?.ToJsonString(_jsonOptions) ?? "{}";
        }
    }

    [McpServerTool]
    [Description("Compare two snapshots to see what changed")]
    public async Task<string> CompareSnapshots(
        [Description("First snapshot filename")] string snapshot1,
        [Description("Second snapshot filename")] string snapshot2,
        CancellationToken cancellationToken = default)
    {
        try
        {
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
}