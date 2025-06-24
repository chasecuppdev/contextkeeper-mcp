using Xunit;
using ContextKeeper.Core;
using ContextKeeper.Core.Interfaces;
using ContextKeeper.Config;

namespace ContextKeeper.Tests;

/// <summary>
/// Tests for snapshot creation and management.
/// Demonstrates how to test file-based operations with proper isolation.
/// </summary>
public class SnapshotTests : TestBase, IDisposable
{
    private readonly ISnapshotManager _snapshotManager;
    private readonly IConfigurationService _configService;
    private readonly string _tempDirectory;
    private readonly string _originalDirectory;
    
    public SnapshotTests()
    {
        _snapshotManager = GetService<ISnapshotManager>();
        _configService = GetService<IConfigurationService>();
        
        // Save original directory
        _originalDirectory = Environment.CurrentDirectory;
        
        // Create isolated test environment
        _tempDirectory = CreateTempDirectory();
        CopyTestData(_tempDirectory);
        Environment.CurrentDirectory = _tempDirectory;
    }
    
    [Fact]
    public async Task CreateSnapshot_WithValidMilestone_ShouldSucceed()
    {
        // Arrange
        var profile = await _configService.GetActiveProfileAsync();
        var milestone = "test-feature";
        
        // Act
        var result = await _snapshotManager.CreateSnapshotAsync(milestone, profile);
        
        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.SnapshotPath!);
        Assert.Contains(milestone, result.SnapshotPath);
        Assert.True(File.Exists(result.SnapshotPath));
        
        // Verify file content structure
        var content = await File.ReadAllTextAsync(result.SnapshotPath);
        Assert.Contains("# CLAUDE.md Historical Snapshot", content);
        Assert.Contains($"**Milestone**: {milestone}", content);
    }
    
    [Theory]
    [InlineData("feature-123")]    // Valid
    [InlineData("bug-fix-456")]    // Valid with hyphens
    [InlineData("hotfix-789")]      // Valid
    public async Task CreateSnapshot_WithValidMilestoneFormats_ShouldSucceed(string milestone)
    {
        // Arrange
        var profile = await _configService.GetActiveProfileAsync();
        
        // Act
        var result = await _snapshotManager.CreateSnapshotAsync(milestone, profile);
        
        // Assert
        Assert.True(result.Success);
        Assert.Contains(milestone, result.SnapshotPath);
    }
    
    [Theory]
    [InlineData("Feature 123")]     // Invalid - contains space
    [InlineData("FEATURE")]          // Invalid - uppercase
    [InlineData("feature_123")]      // Invalid - underscore
    [InlineData("feature@123")]      // Invalid - special character
    [InlineData("")]                 // Invalid - empty
    public async Task CreateSnapshot_WithInvalidMilestone_ShouldFail(string milestone)
    {
        // Arrange
        var profile = await _configService.GetActiveProfileAsync();
        
        // Act
        var result = await _snapshotManager.CreateSnapshotAsync(milestone, profile);
        
        // Assert
        Assert.False(result.Success);
        Assert.True(result.Message.ToLower().Contains("must match pattern") || 
                   result.Message.ToLower().Contains("cannot be empty") ||
                   result.Message.ToLower().Contains("exceeds maximum length"),
                   $"Expected validation error but got: {result.Message}");
    }
    
    [Fact]
    public async Task CreateSnapshot_ShouldPreservePreviousState()
    {
        // This test verifies that creating a new snapshot doesn't affect existing ones
        
        // Arrange
        var profile = await _configService.GetActiveProfileAsync();
        var existingSnapshots = Directory.GetFiles(profile.Paths.Snapshots, "*.md").Length;
        
        // Act
        var result = await _snapshotManager.CreateSnapshotAsync("new-feature", profile);
        
        // Assert
        Assert.True(result.Success);
        var newSnapshotCount = Directory.GetFiles(profile.Paths.Snapshots, "*.md").Length;
        Assert.Equal(existingSnapshots + 1, newSnapshotCount);
    }
    
    [Fact]
    public async Task ListSnapshots_ShouldReturnAllFiles()
    {
        // Arrange
        var profile = await _configService.GetActiveProfileAsync();
        
        // Act
        var snapshots = Directory.GetFiles(profile.Paths.Snapshots, "*.md")
            .OrderByDescending(f => f)
            .ToList();
        
        // Assert
        Assert.NotEmpty(snapshots);
        Assert.Contains(snapshots, s => s.Contains("2024-02-01_api-endpoints"));
    }
    
    [Fact]
    public async Task CompareSnapshots_ShouldIdentifyDifferences()
    {
        // Arrange
        var profile = await _configService.GetActiveProfileAsync();
        var snapshot1 = "CLAUDE_2024-01-15_initial-setup.md";
        var snapshot2 = "CLAUDE_2024-01-20_add-authentication.md";
        
        // Act
        var comparison = await _snapshotManager.CompareSnapshotsAsync(snapshot1, snapshot2, profile);
        
        // Assert
        Assert.True(comparison.Success);
        Assert.NotEmpty(comparison.AddedSections);
        Assert.Contains("Authentication", string.Join(" ", comparison.AddedSections));
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