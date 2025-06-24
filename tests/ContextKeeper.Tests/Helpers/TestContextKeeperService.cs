using System.Text.Json.Nodes;
using ContextKeeper.Core;
using ContextKeeper.Core.Interfaces;
using ContextKeeper.Config;
using Microsoft.Extensions.Logging;

namespace ContextKeeper.Tests.Helpers;

/// <summary>
/// A wrapper around ContextKeeperService that prevents config file creation during tests.
/// </summary>
public class TestContextKeeperService : IContextKeeperService
{
    private readonly ContextKeeperService _innerService;
    private readonly ILogger<TestContextKeeperService> _logger;
    private readonly IConfigurationService _configService;
    private readonly bool _preventConfigWrites;

    public TestContextKeeperService(
        ILogger<TestContextKeeperService> logger,
        IConfigurationService configService,
        ISnapshotManager snapshotManager,
        ISearchEngine searchEngine,
        IEvolutionTracker evolutionTracker,
        ICompactionEngine compactionEngine,
        bool preventConfigWrites = true)
    {
        _logger = logger;
        _configService = configService;
        _preventConfigWrites = preventConfigWrites;
        
        // Create the real service with a logger factory
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var innerLogger = loggerFactory.CreateLogger<ContextKeeperService>();
        
        _innerService = new ContextKeeperService(
            innerLogger,
            configService,
            snapshotManager,
            searchEngine,
            evolutionTracker,
            compactionEngine);
    }

    public Task<JsonObject> CreateSnapshot(string milestoneDescription)
    {
        return _innerService.CreateSnapshot(milestoneDescription);
    }

    public Task<JsonObject> CheckCompactionNeeded()
    {
        return _innerService.CheckCompactionNeeded();
    }

    public Task<JsonObject> SearchHistory(string searchTerm, int maxResults = 5)
    {
        return _innerService.SearchHistory(searchTerm, maxResults);
    }

    public Task<JsonObject> GetArchitecturalEvolution(string componentName)
    {
        return _innerService.GetArchitecturalEvolution(componentName);
    }

    public Task<JsonObject> CompareSnapshots(string snapshot1, string snapshot2)
    {
        return _innerService.CompareSnapshots(snapshot1, snapshot2);
    }

    public async Task<JsonObject> InitializeProject(string? profileName = null)
    {
        if (_preventConfigWrites)
        {
            _logger.LogInformation("Test mode: Skipping config file creation");
            
            // Simulate the initialization without creating config file
            var profile = profileName != null 
                ? await _configService.GetProfileAsync(profileName)
                : await _configService.DetectProfileAsync();
                
            if (profile == null)
            {
                return new JsonObject
                {
                    ["success"] = false,
                    ["message"] = "No suitable profile found. Please specify a profile name."
                };
            }
            
            // Create necessary directories
            var paths = profile.Paths;
            Directory.CreateDirectory(paths.History);
            Directory.CreateDirectory(paths.Snapshots);
            if (!string.IsNullOrEmpty(paths.Compacted))
            {
                Directory.CreateDirectory(paths.Compacted);
            }
            
            // Return success without creating config file
            return new JsonObject
            {
                ["success"] = true,
                ["profile"] = profile.Name,
                ["message"] = $"Initialized ContextKeeper with '{profile.Name}' profile (test mode - no config file created)",
                ["directories"] = new JsonObject
                {
                    ["history"] = paths.History,
                    ["snapshots"] = paths.Snapshots
                }
            };
        }
        
        return await _innerService.InitializeProject(profileName);
    }
}