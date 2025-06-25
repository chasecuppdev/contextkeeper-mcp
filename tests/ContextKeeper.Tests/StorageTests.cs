using Xunit;
using ContextKeeper.Config;
using ContextKeeper.Config.Models;

namespace ContextKeeper.Tests;

/// <summary>
/// Tests for storage configuration and directory structure.
/// These tests verify that our new .contextkeeper directory structure works correctly.
/// </summary>
public class StorageTests : TestBase, IDisposable
{
    private readonly IConfigurationService _configService;
    
    public StorageTests() : base(useMockConfiguration: true)
    {
        _configService = GetService<IConfigurationService>();
        
        // Create isolated test environment
        var testDir = CreateIsolatedEnvironment(TestScenario.Mixed);
        SetCurrentDirectory(testDir);
    }
    
    [Fact]
    public async Task GetConfig_ShouldUseContextKeeperDirectory()
    {
        // Act
        var config = await _configService.GetConfigAsync();
        
        // Assert
        Assert.NotNull(config);
        Assert.Equal("2.0", config.Version);
        Assert.StartsWith(".contextkeeper", config.Paths.History);
        Assert.Equal(".contextkeeper", config.Paths.History);
        Assert.Equal(".contextkeeper/snapshots", config.Paths.Snapshots);
        Assert.Equal(".contextkeeper/archived", config.Paths.Archived);
    }
    
    [Fact]
    public async Task GetConfig_ShouldHaveDefaultSettings()
    {
        // Act
        var config = await _configService.GetConfigAsync();
        
        // Assert
        Assert.NotNull(config);
        Assert.Equal(20, config.Compaction.Threshold);
        Assert.Equal(90, config.Compaction.MaxAgeInDays);
        Assert.True(config.Compaction.AutoCompact);
        Assert.True(config.Snapshot.AutoCapture);
        Assert.Equal(30, config.Snapshot.AutoCaptureIntervalMinutes);
    }
    
    [Fact]
    public async Task GetConfig_ShouldIncludeContextTrackingSettings()
    {
        // Act
        var config = await _configService.GetConfigAsync();
        
        // Assert
        Assert.NotNull(config);
        Assert.True(config.ContextTracking.TrackOpenFiles);
        Assert.True(config.ContextTracking.TrackGitState);
        Assert.True(config.ContextTracking.TrackRecentCommands);
        Assert.Contains("*.md", config.ContextTracking.DocumentationFiles);
    }
    
    [Theory]
    [InlineData("yyyy-MM-dd", "SNAPSHOT_{date}_{type}_{milestone}.md")]
    public async Task Config_ShouldHaveCorrectSnapshotSettings(string expectedDateFormat, string expectedPattern)
    {
        // This demonstrates parameterized testing with xUnit Theory
        
        // Act
        var config = await _configService.GetConfigAsync();
        
        // Assert
        Assert.NotNull(config);
        Assert.Equal(expectedDateFormat, config.Snapshot.DateFormat);
        Assert.Equal(expectedPattern, config.Snapshot.FilenamePattern);
    }
    
    [Fact]
    public async Task TestDataStructure_ShouldExist()
    {
        // This test verifies our test data setup is correct
        var config = await _configService.GetConfigAsync();
        
        // Initialize the project structure
        var service = GetService<ContextKeeper.Core.Interfaces.IContextKeeperService>();
        await service.InitializeProject();
        
        // Assert - Check that our test data directories exist
        Assert.True(Directory.Exists(".contextkeeper"));
        Assert.True(Directory.Exists(".contextkeeper/snapshots"));
        Assert.True(Directory.Exists(".contextkeeper/archived"));
        
        // Create a test snapshot to verify structure works
        await service.CreateSnapshot("test-structure");
        
        // Check that we have at least one snapshot
        var snapshots = Directory.GetFiles(
            ".contextkeeper/snapshots", 
            "*.md");
        Assert.True(snapshots.Length >= 1, $"Expected at least 1 snapshot, but found {snapshots.Length}");
    }
    
    public override void Dispose()
    {
        base.Dispose();
    }
}