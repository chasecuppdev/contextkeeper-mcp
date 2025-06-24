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
    private readonly string _originalDirectory;
    
    public IntegrationTests()
    {
        _service = GetService<IContextKeeperService>();
        
        // Save original directory
        _originalDirectory = Environment.CurrentDirectory;
        
        // Create isolated environment for integration tests
        _tempDirectory = CreateTempDirectory();
        CopyTestData(_tempDirectory);
        Environment.CurrentDirectory = _tempDirectory;
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
        var searchResult = await _service.SearchHistory("integration-test", 5);
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
        Assert.Equal("claude-workflow", result["profile"]?.GetValue<string>());
        
        // Verify directory structure was created
        Assert.True(Directory.Exists(".contextkeeper/claude-workflow"));
        Assert.True(Directory.Exists(".contextkeeper/claude-workflow/snapshots"));
        Assert.True(Directory.Exists(".contextkeeper/claude-workflow/compacted"));
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
            "CLAUDE_2024-01-15_initial-setup.md",
            "CLAUDE_2024-02-01_api-endpoints.md"
        );
        
        // Assert
        Assert.True(result["success"]?.GetValue<bool>());
        
        var added = result["addedSections"] as JsonArray;
        var modified = result["modifiedSections"] as JsonArray;
        
        Assert.NotNull(added);
        Assert.NotNull(modified);
        Assert.NotEmpty(added);
        Assert.NotEmpty(modified);
    }
    
    [Fact]
    public async Task CompactionCheck_WithMultipleSnapshots_ShouldDetectNeed()
    {
        // Our test data has 4 snapshots with threshold of 10
        
        // Act
        var result = await _service.CheckCompactionNeeded();
        
        // Assert
        Assert.Equal(4, result["snapshotCount"]?.GetValue<int>());
        Assert.False(result["compactionNeeded"]?.GetValue<bool>());
        Assert.Contains("No compaction needed", result["recommendedAction"]?.GetValue<string>());
    }
    
    [Fact]
    public async Task ProfileDetection_ShouldWorkCorrectly()
    {
        // Test with README project
        var readmeDir = Path.Combine(_tempDirectory, "ReadmeProject");
        Directory.CreateDirectory(readmeDir);
        Environment.CurrentDirectory = readmeDir;
        await File.WriteAllTextAsync("README.md", "# Readme Project");
        
        // Act
        var result = await _service.InitializeProject();
        
        // Assert
        Assert.True(result["success"]?.GetValue<bool>());
        Assert.Equal("readme-workflow", result["profile"]?.GetValue<string>());
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
        // Restore original directory
        Environment.CurrentDirectory = _originalDirectory;
        
        // Clean up temporary directory
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
        base.Dispose();
    }
}