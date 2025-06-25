using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ContextKeeper.Config;
using ContextKeeper.Config.Models;
using ContextKeeper.Json;
using ContextKeeper.Core.Interfaces;

namespace ContextKeeper.Core;

public class ContextKeeperService : IContextKeeperService
{
    private readonly ILogger<ContextKeeperService> _logger;
    private readonly IConfigurationService _configService;
    private readonly ISnapshotManager _snapshotManager;
    private readonly ISearchEngine _searchEngine;
    private readonly IEvolutionTracker _evolutionTracker;
    private readonly ICompactionEngine _compactionEngine;
    
    public ContextKeeperService(
        ILogger<ContextKeeperService> logger,
        IConfigurationService configService,
        ISnapshotManager snapshotManager,
        ISearchEngine searchEngine,
        IEvolutionTracker evolutionTracker,
        ICompactionEngine compactionEngine)
    {
        _logger = logger;
        _configService = configService;
        _snapshotManager = snapshotManager;
        _searchEngine = searchEngine;
        _evolutionTracker = evolutionTracker;
        _compactionEngine = compactionEngine;
    }
    
    public async Task<JsonObject> CreateSnapshot(string milestoneDescription)
    {
        try
        {
            var config = await _configService.GetConfigAsync();
            var result = await _snapshotManager.CreateSnapshotAsync(milestoneDescription, config);
            
            return new JsonObject
            {
                ["success"] = result.Success,
                ["snapshotPath"] = result.SnapshotPath,
                ["message"] = result.Message,
                ["version"] = config.Version
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create snapshot");
            return new JsonObject
            {
                ["success"] = false,
                ["message"] = $"Error: {ex.Message}"
            };
        }
    }
    
    public async Task<JsonObject> CheckCompactionNeeded()
    {
        try
        {
            var config = await _configService.GetConfigAsync();
            var result = await _compactionEngine.CheckCompactionNeededAsync(config);
            
            return new JsonObject
            {
                ["snapshotCount"] = result.SnapshotCount,
                ["compactionNeeded"] = result.CompactionNeeded,
                ["oldestSnapshot"] = result.OldestSnapshot ?? "",
                ["newestSnapshot"] = result.NewestSnapshot ?? "",
                ["recommendedAction"] = result.RecommendedAction,
                ["version"] = config.Version
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check compaction status");
            return new JsonObject
            {
                ["error"] = ex.Message
            };
        }
    }
    
    public async Task<JsonObject> SearchHistory(string searchTerm, int maxResults = 5)
    {
        try
        {
            var config = await _configService.GetConfigAsync();
            var results = await _searchEngine.SearchAsync(searchTerm, maxResults, config);
            
            var matches = new JsonArray();
            foreach (var result in results.Matches)
            {
#pragma warning disable IL2026, IL3050 // Suppress AOT warnings for JsonObject in JsonArray
                matches.Add(new JsonObject
                {
                    ["fileName"] = result.FileName,
                    ["lineNumber"] = result.LineNumber,
                    ["context"] = result.Context,
                    ["matchedLine"] = result.MatchedLine
                });
#pragma warning restore IL2026, IL3050
            }
            
            return new JsonObject
            {
                ["searchTerm"] = searchTerm,
                ["totalMatches"] = results.TotalMatches,
                ["matches"] = matches,
                ["version"] = config.Version
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search history");
            return new JsonObject
            {
                ["error"] = ex.Message
            };
        }
    }
    
    public async Task<JsonObject> GetArchitecturalEvolution(string componentName)
    {
        try
        {
            var config = await _configService.GetConfigAsync();
            var evolution = await _evolutionTracker.GetEvolutionAsync(componentName, config);
            
            var steps = new JsonArray();
            foreach (var step in evolution.Steps)
            {
#pragma warning disable IL2026, IL3050 // Suppress AOT warnings for JsonObject in JsonArray
                steps.Add(new JsonObject
                {
                    ["date"] = step.Date.ToString("yyyy-MM-dd"),
                    ["milestone"] = step.Milestone,
                    ["status"] = step.Status,
                    ["fileName"] = step.FileName
                });
#pragma warning restore IL2026, IL3050
            }
            
            return new JsonObject
            {
                ["componentName"] = componentName,
                ["evolutionSteps"] = steps,
                ["summary"] = evolution.Summary,
                ["version"] = config.Version
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get architectural evolution");
            return new JsonObject
            {
                ["error"] = ex.Message
            };
        }
    }
    
    public async Task<JsonObject> CompareSnapshots(string snapshot1, string snapshot2)
    {
        try
        {
            var config = await _configService.GetConfigAsync();
            var comparison = await _snapshotManager.CompareSnapshotsAsync(snapshot1, snapshot2, config);
            
            if (!comparison.Success)
            {
                return new JsonObject
                {
                    ["success"] = false,
                    ["message"] = comparison.Message
                };
            }
            
            var addedSections = new JsonArray(comparison.AddedSections.Select(s => JsonValue.Create(s)).ToArray());
            var removedSections = new JsonArray(comparison.RemovedSections.Select(s => JsonValue.Create(s)).ToArray());
            var modifiedSections = new JsonArray(comparison.ModifiedSections.Select(s => JsonValue.Create(s)).ToArray());
            
            return new JsonObject
            {
                ["success"] = true,
                ["snapshot1"] = snapshot1,
                ["snapshot2"] = snapshot2,
                ["addedSections"] = addedSections,
                ["removedSections"] = removedSections,
                ["modifiedSections"] = modifiedSections,
                ["summary"] = comparison.Message,
                ["version"] = config.Version
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compare snapshots");
            return new JsonObject
            {
                ["error"] = ex.Message
            };
        }
    }
    
    // Additional tools for enhanced functionality
    
    public async Task<JsonObject> InitializeProject(string? profileName = null)
    {
        try
        {
            var config = await _configService.GetConfigAsync();
                
            // Configuration is always available with defaults
            
            // Create necessary directories
            var paths = config.Paths;
            Directory.CreateDirectory(paths.History);
            Directory.CreateDirectory(paths.Snapshots);
            Directory.CreateDirectory(paths.Archived);
            
            // Save configuration if needed
            await _configService.InitializeProjectAsync();
            
            return new JsonObject
            {
                ["success"] = true,
                ["version"] = config.Version,
                ["message"] = "Initialized ContextKeeper successfully",
                ["directories"] = new JsonObject
                {
                    ["history"] = paths.History,
                    ["snapshots"] = paths.Snapshots,
                    ["archived"] = paths.Archived
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize project");
            return new JsonObject
            {
                ["success"] = false,
                ["message"] = $"Error: {ex.Message}"
            };
        }
    }
}