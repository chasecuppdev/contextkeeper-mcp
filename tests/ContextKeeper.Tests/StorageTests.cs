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
    private readonly string _tempDirectory;
    private readonly string _originalDirectory;
    
    public StorageTests() : base(useMockConfiguration: true)
    {
        _configService = GetService<IConfigurationService>();
        
        // Save original directory
        _originalDirectory = Environment.CurrentDirectory;
        
        // Create isolated test environment
        _tempDirectory = CreateTempDirectory();
        CopyTestData(_tempDirectory);
        Environment.CurrentDirectory = _tempDirectory;
    }
    
    [Fact]
    public async Task GetActiveProfile_WithClaudeFile_ShouldUseContextKeeperDirectory()
    {
        // Act
        var profile = await _configService.GetActiveProfileAsync();
        
        // Assert
        Assert.NotNull(profile);
        Assert.Equal("claude-workflow", profile.Name);
        Assert.StartsWith(".contextkeeper/", profile.Paths.History);
        Assert.Equal(".contextkeeper/claude-workflow", profile.Paths.History);
        Assert.Equal(".contextkeeper/claude-workflow/snapshots", profile.Paths.Snapshots);
        Assert.Equal(".contextkeeper/claude-workflow/compacted", profile.Paths.Compacted);
    }
    
    [Fact]
    public async Task GetProfileAsync_ForReadmeWorkflow_ShouldUseContextKeeperDirectory()
    {
        // Act
        var profile = await _configService.GetProfileAsync("readme-workflow");
        
        // Assert
        Assert.NotNull(profile);
        Assert.Equal("readme-workflow", profile.Name);
        Assert.Equal(".contextkeeper/readme-workflow", profile.Paths.History);
        Assert.Equal(".contextkeeper/readme-workflow/snapshots", profile.Paths.Snapshots);
        Assert.Equal(".contextkeeper/readme-workflow/compacted", profile.Paths.Compacted);
    }
    
    [Fact]
    public async Task LoadConfigAsync_ShouldIncludeBothDefaultProfiles()
    {
        // Act
        var config = await _configService.LoadConfigAsync();
        
        // Assert
        Assert.NotNull(config);
        Assert.True(config.Profiles.ContainsKey("claude-workflow"));
        Assert.True(config.Profiles.ContainsKey("readme-workflow"));
        Assert.Equal(2, config.Profiles.Count);
    }
    
    [Theory]
    [InlineData("claude-workflow", "CLAUDE_", 10)]
    [InlineData("readme-workflow", "README_", 20)]
    public async Task Profiles_ShouldHaveCorrectSettings(string profileName, string expectedPrefix, int expectedThreshold)
    {
        // This demonstrates parameterized testing with xUnit Theory
        
        // Act
        var profile = await _configService.GetProfileAsync(profileName);
        
        // Assert
        Assert.NotNull(profile);
        Assert.Equal(expectedPrefix, profile.Snapshot.Prefix);
        Assert.Equal(expectedThreshold, profile.Compaction.Threshold);
    }
    
    [Fact]
    public void TestDataStructure_ShouldExist()
    {
        // This test verifies our test data setup is correct
        
        // Assert - Check that our test data directories exist
        Assert.True(Directory.Exists(".contextkeeper"));
        Assert.True(Directory.Exists(".contextkeeper/claude-workflow/snapshots"));
        Assert.True(Directory.Exists(".contextkeeper/claude-workflow/compacted"));
        Assert.True(Directory.Exists(".contextkeeper/readme-workflow/snapshots"));
        
        // Check that we have test snapshots
        var claudeSnapshots = Directory.GetFiles(
            ".contextkeeper/claude-workflow/snapshots", 
            "*.md");
        Assert.Equal(4, claudeSnapshots.Length);
        
        var compactedFiles = Directory.GetFiles(
            ".contextkeeper/claude-workflow/compacted", 
            "*.md");
        Assert.Single(compactedFiles);
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