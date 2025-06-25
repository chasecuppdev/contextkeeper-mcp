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
    
    public SnapshotTests() : base(useMockConfiguration: false)
    {
        _snapshotManager = GetService<ISnapshotManager>();
        _configService = GetService<IConfigurationService>();
        
        // Create isolated test environment with CLAUDE project
        _tempDirectory = CreateIsolatedEnvironment(TestScenario.ClaudeOnly);
        SetCurrentDirectory(_tempDirectory);
    }
    
    [Fact]
    public async Task CreateSnapshot_WithValidMilestone_ShouldSucceed()
    {
        // Arrange
        var config = await _configService.GetConfigAsync();
        var milestone = "test-feature";
        
        // Act
        var result = await _snapshotManager.CreateSnapshotAsync(milestone, config);
        
        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.SnapshotPath!);
        Assert.Contains(milestone, result.SnapshotPath);
        Assert.True(File.Exists(result.SnapshotPath));
        
        // Verify file content structure
        var content = await File.ReadAllTextAsync(result.SnapshotPath);
        Assert.Contains("# Development Context Snapshot", content);
        Assert.Contains($"**Milestone**: {milestone}", content);
    }
    
    [Theory]
    [InlineData("feature-123")]    // Valid
    [InlineData("bug-fix-456")]    // Valid with hyphens
    [InlineData("hotfix-789")]      // Valid
    public async Task CreateSnapshot_WithValidMilestoneFormats_ShouldSucceed(string milestone)
    {
        // Arrange
        var config = await _configService.GetConfigAsync();
        
        // Act
        var result = await _snapshotManager.CreateSnapshotAsync(milestone, config);
        
        // Assert
        Assert.True(result.Success);
        Assert.Contains(milestone, result.SnapshotPath);
    }
    
    [Theory]
    [InlineData("Feature 123")]     // Invalid - contains space
    [InlineData("FEATURE")]          // Valid in new system (uppercase allowed)
    [InlineData("feature_123")]      // Invalid - underscore not allowed
    [InlineData("feature@123")]      // Invalid - special character
    [InlineData("")]                 // Invalid - empty
    public async Task CreateSnapshot_WithInvalidMilestone_ShouldFail(string milestone)
    {
        // Arrange
        var config = await _configService.GetConfigAsync();
        
        // Act
        var result = await _snapshotManager.CreateSnapshotAsync(milestone, config);
        
        // Assert
        // In the new system, uppercase is allowed but underscores are not
        if (milestone == "FEATURE")
        {
            Assert.True(result.Success);
        }
        else
        {
            Assert.False(result.Success);
            Assert.Contains("milestone", result.Message.ToLower());
        }
    }
    
    [Fact]
    public async Task CreateSnapshot_ShouldPreservePreviousState()
    {
        // This test verifies that creating a new snapshot doesn't affect existing ones
        
        // Arrange
        var config = await _configService.GetConfigAsync();
        var snapshotPath = Path.Combine(Environment.CurrentDirectory, config.Paths.Snapshots);
        
        // Ensure snapshots directory exists
        Directory.CreateDirectory(snapshotPath);
        
        // Get list of existing files before creating new snapshot
        var existingFiles = Directory.Exists(snapshotPath) 
            ? Directory.GetFiles(snapshotPath, "*.md").Select(Path.GetFileName).Where(f => f != null).ToHashSet()!
            : new HashSet<string>();
        
        // Act
        var result = await _snapshotManager.CreateSnapshotAsync("preserve-test", config);
        
        // Assert
        Assert.True(result.Success, $"Snapshot creation failed: {result.Message}");
        Assert.NotNull(result.SnapshotPath);
        Assert.True(File.Exists(result.SnapshotPath), $"Snapshot file not created at: {result.SnapshotPath}");
        
        // Verify the new file was created
        var newFileName = Path.GetFileName(result.SnapshotPath);
        Assert.Contains("preserve-test", newFileName);
        
        // Verify all existing files are still there
        var currentFiles = Directory.GetFiles(snapshotPath, "*.md").Select(Path.GetFileName).ToHashSet();
        foreach (var existingFile in existingFiles)
        {
            Assert.Contains(existingFile, currentFiles);
        }
        
        // Verify we have exactly one more file
        Assert.Equal(existingFiles.Count + 1, currentFiles.Count);
    }
    
    [Fact]
    public async Task ListSnapshots_ShouldReturnAllFiles()
    {
        // Arrange
        var config = await _configService.GetConfigAsync();
        
        // Create a test snapshot first
        await _snapshotManager.CreateSnapshotAsync("test-list", config);
        
        // Act
        var snapshots = Directory.GetFiles(config.Paths.Snapshots, "*.md")
            .OrderByDescending(f => f)
            .ToList();
        
        // Assert
        Assert.NotEmpty(snapshots);
        Assert.Contains(snapshots, s => s.Contains("test-list"));
    }
    
    [Fact]
    public async Task CompareSnapshots_ShouldIdentifyDifferences()
    {
        // Arrange
        var config = await _configService.GetConfigAsync();
        
        // Create two snapshots with different content
        var result1 = await _snapshotManager.CreateSnapshotAsync("initial-setup", config);
        
        // Modify CLAUDE.md to add authentication content
        var claudePath = Path.Combine(Environment.CurrentDirectory, "CLAUDE.md");
        var originalContent = await File.ReadAllTextAsync(claudePath);
        await File.WriteAllTextAsync(claudePath, originalContent + "\n\n## Authentication\nAdded JWT authentication.");
        
        var result2 = await _snapshotManager.CreateSnapshotAsync("add-authentication", config);
        
        // Get just the filenames
        var snapshot1 = Path.GetFileName(result1.SnapshotPath!);
        var snapshot2 = Path.GetFileName(result2.SnapshotPath!);
        
        // Act
        var comparison = await _snapshotManager.CompareSnapshotsAsync(snapshot1, snapshot2, config);
        
        // Assert
        Assert.True(comparison.Success);
        // Either the Documentation section or CLAUDE.md subsection should be modified
        Assert.NotEmpty(comparison.ModifiedSections);
        var hasDiffererence = comparison.ModifiedSections.Contains("Documentation") || 
                             comparison.ModifiedSections.Contains("CLAUDE.md") ||
                             comparison.AddedSections.Count > 0;
        Assert.True(hasDiffererence, 
            $"Expected changes in comparison. Modified sections: [{string.Join(", ", comparison.ModifiedSections)}], " +
            $"Added sections: [{string.Join(", ", comparison.AddedSections)}]");
        
        // Restore original content
        await File.WriteAllTextAsync(claudePath, originalContent);
    }
    
    public override void Dispose()
    {
        // Base class handles directory restoration and cleanup
        base.Dispose();
    }
}