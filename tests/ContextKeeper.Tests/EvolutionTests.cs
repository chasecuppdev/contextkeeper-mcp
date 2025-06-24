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
    }
    
    [Fact]
    public async Task GetEvolution_ForTrackedComponent_ShouldReturnHistory()
    {
        // Arrange
        var profile = await _configService.GetActiveProfileAsync();
        var componentName = "Authentication";
        
        // Act
        var evolution = await _evolutionTracker.GetEvolutionAsync(componentName, profile);
        
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
        var profile = await _configService.GetActiveProfileAsync();
        
        // Act
        var evolution = await _evolutionTracker.GetEvolutionAsync(componentName, profile);
        
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
        var profile = await _configService.GetActiveProfileAsync();
        
        // Act
        var evolution = await _evolutionTracker.GetEvolutionAsync("Authentication", profile);
        
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
        var profile = await _configService.GetActiveProfileAsync();
        
        // Act
        var evolution = await _evolutionTracker.GetEvolutionAsync("NonExistentComponent", profile);
        
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
        var profile = await _configService.GetActiveProfileAsync();
        
        // Act
        var timeline = await _evolutionTracker.GetTimelineAsync(profile);
        
        // Assert
        Assert.NotNull(timeline);
        Assert.NotEmpty(timeline.Events);
        
        // Should include both regular and compacted snapshots
        Assert.Contains(timeline.Events, e => e.Type == "Snapshot");
        Assert.Contains(timeline.Events, e => e.Type == "Compacted");
        
        // Verify chronological order
        var dates = timeline.Events.Select(e => e.Date).ToList();
        Assert.Equal(dates.OrderBy(d => d).ToList(), dates);
    }
    
    [Fact]
    public async Task GetTimeline_ShouldExtractMilestones()
    {
        // Arrange
        var profile = await _configService.GetActiveProfileAsync();
        
        // Act
        var timeline = await _evolutionTracker.GetTimelineAsync(profile);
        
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
        var profile = await _configService.GetActiveProfileAsync();
        
        // Act
        var cleanArchEvolution = await _evolutionTracker.GetEvolutionAsync("Clean Architecture", profile);
        var cqrsEvolution = await _evolutionTracker.GetEvolutionAsync("CQRS", profile);
        
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