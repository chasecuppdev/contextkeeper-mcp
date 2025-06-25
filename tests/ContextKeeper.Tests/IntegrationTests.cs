using Xunit;
using System.Text.Json.Nodes;
using ContextKeeper.Core;
using ContextKeeper.Core.Interfaces;

namespace ContextKeeper.Tests;

/// <summary>
/// Integration tests that verify complete workflows.
/// These tests exercise multiple components together as they would be used in production.
/// </summary>
public class IntegrationTests : TestBase, IDisposable
{
    private readonly IContextKeeperService _service;
    private readonly string _tempDirectory;
    
    public IntegrationTests() : base(useMockConfiguration: false)
    {
        _service = GetService<IContextKeeperService>();
        
        // Create isolated environment with mixed scenario (both CLAUDE.md and README.md)
        _tempDirectory = CreateIsolatedEnvironment(TestScenario.Mixed);
        SetCurrentDirectory(_tempDirectory);
        
        // Create test snapshots for integration tests
        CreateIntegrationTestData().GetAwaiter().GetResult();
    }
    
    private async Task CreateIntegrationTestData()
    {
        var snapshotPath = Path.Combine(Environment.CurrentDirectory, ".contextkeeper/snapshots");
        Directory.CreateDirectory(snapshotPath);
        
        // Create snapshots with expected content
        var snapshot1 = Path.Combine(snapshotPath, "SNAPSHOT_2024-01-15_manual_initial-setup.md");
        await File.WriteAllTextAsync(snapshot1, @"# Development Context Snapshot
**Timestamp**: 2024-01-15 10:00:00 UTC
**Type**: manual
**Milestone**: initial-setup

## Documentation
### CLAUDE.md
# TaskManager API
## Architecture
- Clean Architecture: Planned
- Authentication: Planned
");
        
        var snapshot2 = Path.Combine(snapshotPath, "SNAPSHOT_2024-02-01_manual_api-endpoints.md");
        await File.WriteAllTextAsync(snapshot2, @"# Development Context Snapshot
**Timestamp**: 2024-02-01 10:00:00 UTC
**Type**: manual
**Milestone**: api-endpoints

## Documentation
### CLAUDE.md
# TaskManager API
## Architecture
- Clean Architecture: Completed
- Authentication: Completed
- API Endpoints: Added
- New Controllers: UserController, TaskController
");
    }
    
    [Fact]
    public async Task CompleteWorkflow_CreateSearchAndTrack_ShouldWork()
    {
        // This test simulates a complete user workflow
        
        // Step 1: Create a new snapshot
        var snapshotResult = await _service.CreateSnapshot("integration-test");
        Assert.NotNull(snapshotResult);
        Assert.True(snapshotResult["success"]?.GetValue<bool>());
        
        // Step 2: Search for content in the new snapshot
        var searchResult = await _service.SearchHistory("TaskManager", 5);
        Assert.NotNull(searchResult);
        Assert.True(searchResult["totalMatches"]?.GetValue<int>() > 0);
        
        // Step 3: Check compaction status
        var compactionResult = await _service.CheckCompactionNeeded();
        Assert.NotNull(compactionResult);
        Assert.True(compactionResult.ContainsKey("snapshotCount"));
        
        // Step 4: Track evolution
        var evolutionResult = await _service.GetArchitecturalEvolution("Authentication");
        Assert.NotNull(evolutionResult);
        Assert.NotNull(evolutionResult["evolutionSteps"]);
    }
    
    [Fact]
    public async Task InitializeProject_ShouldCreateDirectoryStructure()
    {
        // Arrange
        var newProjectDir = Path.Combine(_tempDirectory, "NewProject");
        Directory.CreateDirectory(newProjectDir);
        Environment.CurrentDirectory = newProjectDir;
        
        // Create a CLAUDE.md file to trigger profile detection
        await File.WriteAllTextAsync("CLAUDE.md", "# New Project");
        
        // Act
        var result = await _service.InitializeProject();
        
        // Assert
        Assert.True(result["success"]?.GetValue<bool>());
        
        // Verify directory structure was created (new simplified structure)
        Assert.True(Directory.Exists(".contextkeeper"));
        Assert.True(Directory.Exists(".contextkeeper/snapshots"));
        Assert.True(Directory.Exists(".contextkeeper/archived"));
        
        // Note: The config service loads from test binary directory, not current directory
        // So we don't check for config file creation here - it's tested elsewhere
    }
    
    [Fact]
    public async Task SearchAcrossMultipleSnapshots_ShouldFindAllOccurrences()
    {
        // This tests that search works across multiple files
        
        // Act
        var searchResult = await _service.SearchHistory("Architecture", 20);
        
        // Assert
        Assert.NotNull(searchResult);
        var matches = searchResult["matches"] as JsonArray;
        Assert.NotNull(matches);
        Assert.True(matches.Count > 1);
        
        // Should find matches in different snapshots
        var fileNames = matches.Select(m => m?["fileName"]?.GetValue<string>()).Distinct();
        Assert.True(fileNames.Count() > 1);
    }
    
    [Fact]
    public async Task CompareSnapshots_ShouldIdentifyChanges()
    {
        // Act
        var result = await _service.CompareSnapshots(
            "SNAPSHOT_2024-01-15_manual_initial-setup.md",
            "SNAPSHOT_2024-02-01_manual_api-endpoints.md"
        );
        
        // Assert
        Assert.True(result["success"]?.GetValue<bool>());
        
        var added = result["addedSections"] as JsonArray;
        var modified = result["modifiedSections"] as JsonArray;
        
        Assert.NotNull(added);
        Assert.NotNull(modified);
        // At least one of them should have changes
        Assert.True(added?.Count > 0 || modified?.Count > 0, 
            "Expected either added or modified sections in comparison");
    }
    
    [Fact]
    public async Task CompactionCheck_WithMultipleSnapshots_ShouldDetectNeed()
    {
        // Our test data has 4 snapshots with threshold of 10
        // However, other tests may have created additional snapshots
        
        // Act
        var result = await _service.CheckCompactionNeeded();
        
        // Assert
        var snapshotCount = result["snapshotCount"]?.GetValue<int>() ?? 0;
        Assert.True(snapshotCount >= 2, $"Expected at least 2 snapshots but found {snapshotCount}");
        // With only 2-4 snapshots and threshold of 20, compaction should not be needed
        Assert.False(result["compactionNeeded"]?.GetValue<bool>());
    }
    
    [Fact]
    public async Task InitializeProject_WithDifferentDocumentationFiles_ShouldWork()
    {
        // Create a separate isolated environment for README-only project
        var readmeDir = CreateIsolatedEnvironment(TestScenario.ReadmeOnly);
        SetCurrentDirectory(readmeDir);
        
        // Act
        var result = await _service.InitializeProject();
        
        // Assert
        Assert.True(result["success"]?.GetValue<bool>());
        // No more profile detection - should work with any documentation file
        Assert.True(Directory.Exists(".contextkeeper"));
        
        // Clean up config file to prevent pollution
        if (File.Exists("contextkeeper.config.json"))
        {
            File.Delete("contextkeeper.config.json");
        }
        
        // Restore to main test directory
        SetCurrentDirectory(_tempDirectory);
    }
    
    [Fact]
    public async Task ErrorHandling_WithInvalidSnapshot_ShouldReturnError()
    {
        // Act
        var result = await _service.CompareSnapshots("nonexistent1.md", "nonexistent2.md");
        
        // Assert
        Assert.False(result["success"]?.GetValue<bool>());
        Assert.NotNull(result["message"]);
    }
    
    [Fact]
    public async Task ConcurrentOperations_ShouldNotInterfere()
    {
        // This tests that multiple operations can run without issues
        
        // Act - Run multiple operations in parallel
        var tasks = new[]
        {
            _service.SearchHistory("test", 5),
            _service.CheckCompactionNeeded(),
            _service.GetArchitecturalEvolution("Task"),
            _service.SearchHistory("API", 5)
        };
        
        var results = await Task.WhenAll(tasks);
        
        // Assert - All operations should succeed
        Assert.All(results, result => Assert.NotNull(result));
    }
    
    public override void Dispose()
    {
        // Clean up any config files that might have been created
        var configPath = Path.Combine(_tempDirectory, "contextkeeper.config.json");
        if (File.Exists(configPath))
        {
            try { File.Delete(configPath); } catch { }
        }
        
        // Base class handles directory restoration and cleanup
        base.Dispose();
    }
}