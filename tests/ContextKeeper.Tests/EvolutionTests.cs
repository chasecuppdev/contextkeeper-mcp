using Xunit;
using ContextKeeper.Core;
using ContextKeeper.Core.Interfaces;
using ContextKeeper.Config;

namespace ContextKeeper.Tests;

/// <summary>
/// Tests for tracking component evolution over time.
/// This demonstrates how to test temporal data and history tracking.
/// </summary>
public class EvolutionTests : TestBase, IDisposable
{
    private readonly IEvolutionTracker _evolutionTracker;
    private readonly IConfigurationService _configService;
    private readonly string _tempDirectory;
    
    public EvolutionTests() : base(useMockConfiguration: false)
    {
        _evolutionTracker = GetService<IEvolutionTracker>();
        _configService = GetService<IConfigurationService>();
        
        // Create isolated test environment with CLAUDE project
        _tempDirectory = CreateIsolatedEnvironment(TestScenario.ClaudeOnly);
        SetCurrentDirectory(_tempDirectory);
        
        // Create test snapshots with expected evolution content
        CreateEvolutionTestData().GetAwaiter().GetResult();
    }
    
    private async Task CreateEvolutionTestData()
    {
        var config = await _configService.GetConfigAsync();
        var snapshotPath = Path.Combine(Environment.CurrentDirectory, config.Paths.Snapshots);
        Directory.CreateDirectory(snapshotPath);
        
        // Create snapshots showing evolution of components
        var snapshot1 = Path.Combine(snapshotPath, "SNAPSHOT_2024-01-15_manual_initial-setup.md");
        await File.WriteAllTextAsync(snapshot1, @"# Development Context Snapshot
**Timestamp**: 2024-01-15 10:00:00 UTC
**Type**: manual
**Milestone**: initial-setup

## Documentation
### CLAUDE.md
## Architecture
- Clean Architecture: Planned
- Authentication: Planned
- JWT: Planned
- Repository Pattern: Planned
- Task Management: Planned
- Project Structure: Planned
");
        
        var snapshot2 = Path.Combine(snapshotPath, "SNAPSHOT_2024-01-20_manual_add-authentication.md");
        await File.WriteAllTextAsync(snapshot2, @"# Development Context Snapshot
**Timestamp**: 2024-01-20 10:00:00 UTC
**Type**: manual
**Milestone**: add-authentication

## Documentation
### CLAUDE.md
## Architecture
- Clean Architecture: In Progress
- Authentication: Completed
- JWT: Completed
- Repository Pattern: In Progress
- Task Management: Planned
- Project Structure: In Progress
");
        
        var snapshot3 = Path.Combine(snapshotPath, "SNAPSHOT_2024-01-25_manual_database-integration.md");
        await File.WriteAllTextAsync(snapshot3, @"# Development Context Snapshot
**Timestamp**: 2024-01-25 10:00:00 UTC
**Type**: manual
**Milestone**: database-integration

## Documentation
### CLAUDE.md
## Architecture
- Clean Architecture: Completed
- Authentication: Completed
- JWT: Completed
- Repository Pattern: Completed
- Task Management: In Progress
- Project Structure: Completed
");
        
        var snapshot4 = Path.Combine(snapshotPath, "SNAPSHOT_2024-02-01_manual_api-endpoints.md");
        await File.WriteAllTextAsync(snapshot4, @"# Development Context Snapshot
**Timestamp**: 2024-02-01 10:00:00 UTC
**Type**: manual
**Milestone**: api-endpoints

## Documentation
### CLAUDE.md
## Architecture
- Clean Architecture: Completed
- Authentication: Completed
- JWT: Completed
- Repository Pattern: Completed
- Task Management: Completed
- Project Structure: Completed
- CQRS: Implemented
");
        
        // Create archived/compacted file
        var archivedPath = Path.Combine(Environment.CurrentDirectory, config.Paths.Archived);
        Directory.CreateDirectory(archivedPath);
        
        var compactedFile = Path.Combine(archivedPath, "ARCHIVED_2024-01-01_2024-03-31_COMPACTED.md");
        await File.WriteAllTextAsync(compactedFile, @"# Archived Snapshots: Q1 2024
**Period**: 2024-01-01 to 2024-03-31
**Type**: Compacted
**Total Snapshots**: 4

## Q1 2024 Summary
Completed Clean Architecture implementation.
Implemented Authentication and JWT.
Completed Repository Pattern.
Finished Task and Project management features.
");
    }
    
    [Fact]
    public async Task GetEvolution_ForTrackedComponent_ShouldReturnHistory()
    {
        // Arrange
        var config = await _configService.GetConfigAsync();
        var componentName = "Authentication";
        
        // Act
        var evolution = await _evolutionTracker.GetEvolutionAsync(componentName, config);
        
        // Assert
        Assert.NotNull(evolution);
        Assert.NotEmpty(evolution.Steps);
        Assert.Contains(evolution.Steps, s => s.Status == "Planned");
        Assert.Contains(evolution.Steps, s => s.Status == "Completed");
        Assert.NotEmpty(evolution.Summary);
    }
    
    [Theory]
    [InlineData("JWT")]
    [InlineData("Repository")]
    [InlineData("Task")]
    [InlineData("Project")]
    public async Task GetEvolution_ForVariousComponents_ShouldTrackProgress(string componentName)
    {
        // Arrange
        var config = await _configService.GetConfigAsync();
        
        // Act
        var evolution = await _evolutionTracker.GetEvolutionAsync(componentName, config);
        
        // Assert
        Assert.NotNull(evolution);
        Assert.NotEmpty(evolution.Steps);
        
        // Verify chronological order
        var dates = evolution.Steps.Select(s => s.Date).ToList();
        Assert.Equal(dates.OrderBy(d => d).ToList(), dates);
    }
    
    [Fact]
    public async Task GetEvolution_ShouldIdentifyStatusChanges()
    {
        // Tests that evolution tracking can identify status progression
        
        // Arrange
        var config = await _configService.GetConfigAsync();
        
        // Act
        var evolution = await _evolutionTracker.GetEvolutionAsync("Authentication", config);
        
        // Assert
        var steps = evolution.Steps.OrderBy(s => s.Date).ToList();
        
        // Should show progression: Planned -> In Progress -> Completed
        Assert.Contains(steps, s => s.Status == "Planned");
        Assert.Contains(steps, s => s.Status == "Completed");
        
        // Later steps should not be "Planned" if earlier ones are "Completed"
        var firstCompleted = steps.FirstOrDefault(s => s.Status == "Completed");
        if (firstCompleted != null)
        {
            var afterCompleted = steps.Where(s => s.Date > firstCompleted.Date);
            Assert.DoesNotContain(afterCompleted, s => s.Status == "Planned");
        }
    }
    
    [Fact]
    public async Task GetEvolution_ForNonExistentComponent_ShouldReturnEmpty()
    {
        // Arrange
        var config = await _configService.GetConfigAsync();
        
        // Act
        var evolution = await _evolutionTracker.GetEvolutionAsync("NonExistentComponent", config);
        
        // Assert
        Assert.NotNull(evolution);
        Assert.Empty(evolution.Steps);
        Assert.Contains("not found", evolution.Summary, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public async Task GetTimeline_ShouldReturnAllSnapshots()
    {
        // Tests the timeline functionality
        
        // Arrange
        var config = await _configService.GetConfigAsync();
        
        // Act
        var timeline = await _evolutionTracker.GetTimelineAsync(config);
        
        // Assert
        Assert.NotNull(timeline);
        Assert.NotEmpty(timeline.Events);
        
        // Should include both regular and compacted snapshots
        Assert.Contains(timeline.Events, e => e.Type == "Snapshot");
        Assert.Contains(timeline.Events, e => e.Type == "Archived" || e.Type == "Compacted");
        
        // Verify chronological order
        var dates = timeline.Events.Select(e => e.Date).ToList();
        Assert.Equal(dates.OrderBy(d => d).ToList(), dates);
    }
    
    [Fact]
    public async Task GetTimeline_ShouldExtractMilestones()
    {
        // Arrange
        var config = await _configService.GetConfigAsync();
        
        // Act
        var timeline = await _evolutionTracker.GetTimelineAsync(config);
        
        // Assert
        var milestones = timeline.Events.Select(e => e.Milestone).Where(m => !string.IsNullOrEmpty(m)).ToList();
        
        Assert.Contains("initial-setup", milestones);
        Assert.Contains("add-authentication", milestones);
        Assert.Contains("database-integration", milestones);
        Assert.Contains("api-endpoints", milestones);
    }
    
    [Fact]
    public async Task ComponentEvolution_ShouldShowArchitecturalDecisions()
    {
        // This test verifies that evolution tracking captures architectural decisions
        
        // Arrange
        var config = await _configService.GetConfigAsync();
        
        // Act
        var cleanArchEvolution = await _evolutionTracker.GetEvolutionAsync("Clean Architecture", config);
        var cqrsEvolution = await _evolutionTracker.GetEvolutionAsync("CQRS", config);
        
        // Assert
        // Clean Architecture should be mentioned from the beginning
        Assert.NotEmpty(cleanArchEvolution.Steps);
        Assert.True(cleanArchEvolution.Steps.First().Date <= new DateTime(2024, 1, 15));
        
        // CQRS should appear later with API implementation
        Assert.NotEmpty(cqrsEvolution.Steps);
        Assert.True(cqrsEvolution.Steps.First().Date >= new DateTime(2024, 2, 1));
    }
    
    public override void Dispose()
    {
        // Base class handles directory restoration and cleanup
        base.Dispose();
    }
}