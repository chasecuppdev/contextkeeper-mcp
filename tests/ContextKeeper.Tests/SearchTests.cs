using Xunit;
using ContextKeeper.Core;
using ContextKeeper.Core.Interfaces;
using ContextKeeper.Config;

namespace ContextKeeper.Tests;

/// <summary>
/// Tests for search functionality across snapshots.
/// Demonstrates testing text search and indexing capabilities.
/// </summary>
public class SearchTests : TestBase, IDisposable
{
    private readonly ISearchEngine _searchEngine;
    private readonly IConfigurationService _configService;
    
    public SearchTests()
    {
        _searchEngine = GetService<ISearchEngine>();
        _configService = GetService<IConfigurationService>();
        
        // Create isolated environment and ensure we're in TestData
        var testDir = CreateIsolatedEnvironment(TestScenario.Mixed);
        SetCurrentDirectory(testDir);
        
        // Create test snapshots with expected content
        CreateTestSnapshots().GetAwaiter().GetResult();
    }
    
    private async Task CreateTestSnapshots()
    {
        var config = await _configService.GetConfigAsync();
        var snapshotPath = Path.Combine(Environment.CurrentDirectory, config.Paths.Snapshots);
        Directory.CreateDirectory(snapshotPath);
        
        // Create snapshots with expected content
        var snapshot1 = Path.Combine(snapshotPath, "SNAPSHOT_2024-01-15_manual_initial-setup.md");
        await File.WriteAllTextAsync(snapshot1, @"# Development Context Snapshot
**Timestamp**: 2024-01-15 10:00:00 UTC
**Type**: manual
**Milestone**: initial-setup

## Documentation
### CLAUDE.md
Initial project setup with PostgreSQL database.
Clean Architecture implementation.
");
        
        var snapshot2 = Path.Combine(snapshotPath, "SNAPSHOT_2024-01-20_manual_add-authentication.md");
        await File.WriteAllTextAsync(snapshot2, @"# Development Context Snapshot
**Timestamp**: 2024-01-20 10:00:00 UTC
**Type**: manual
**Milestone**: add-authentication

## Documentation
### CLAUDE.md
Implemented JWT Bearer authentication.
Added Repository pattern.
Clean Architecture with JWT.
");
        
        var snapshot3 = Path.Combine(snapshotPath, "SNAPSHOT_2024-02-01_manual_api-endpoints.md");
        await File.WriteAllTextAsync(snapshot3, @"# Development Context Snapshot
**Timestamp**: 2024-02-01 10:00:00 UTC
**Type**: manual
**Milestone**: api-endpoints

## Documentation
### CLAUDE.md
Added API endpoints with Repository pattern.
PostgreSQL integration complete.
Clean Architecture principles applied.
");
        
        // Create archived directory and compacted file
        var archivedPath = Path.Combine(Environment.CurrentDirectory, config.Paths.Archived);
        Directory.CreateDirectory(archivedPath);
        
        var compactedFile = Path.Combine(archivedPath, "ARCHIVED_2024-01-01_2024-03-31_COMPACTED.md");
        await File.WriteAllTextAsync(compactedFile, @"# Archived Snapshots: Q1 2024
**Period**: 2024-01-01 to 2024-03-31
**Total Snapshots**: 3

## Q1 2024 Summary
Completed authentication implementation.
Integrated PostgreSQL database.
Applied Clean Architecture patterns.
");
    }
    
    [Fact]
    public async Task Search_ForExistingTerm_ShouldReturnMatches()
    {
        // Arrange
        var config = await _configService.GetConfigAsync();
        var searchTerm = "PostgreSQL";
        
        // Act
        var results = await _searchEngine.SearchAsync(searchTerm, maxResults: 10, config);
        
        // Assert
        Assert.NotNull(results);
        Assert.True(results.TotalMatches > 0, $"No matches found for '{searchTerm}'");
        Assert.NotEmpty(results.Matches);
        
        // Verify match details
        var firstMatch = results.Matches.First();
        Assert.NotEmpty(firstMatch.FileName);
        Assert.True(firstMatch.LineNumber > 0);
        Assert.Contains(searchTerm, firstMatch.MatchedLine, StringComparison.OrdinalIgnoreCase);
    }
    
    [Theory]
    [InlineData("JWT", 2)]           // Should be in auth and api snapshots
    [InlineData("Repository", 2)]     // Should be in database and api snapshots
    [InlineData("Clean Architecture", 3)] // Should be in multiple snapshots
    public async Task Search_ShouldFindExpectedOccurrences(string searchTerm, int minExpectedMatches)
    {
        // Arrange
        var config = await _configService.GetConfigAsync();
        
        // Act
        var results = await _searchEngine.SearchAsync(searchTerm, maxResults: 20, config);
        
        // Assert
        Assert.True(results.TotalMatches >= minExpectedMatches, 
            $"Expected at least {minExpectedMatches} matches for '{searchTerm}', but found {results.TotalMatches}");
    }
    
    [Fact]
    public async Task Search_WithCaseInsensitive_ShouldWork()
    {
        // Arrange
        var config = await _configService.GetConfigAsync();
        
        // Act
        var upperResults = await _searchEngine.SearchAsync("POSTGRESQL", maxResults: 10, config);
        var lowerResults = await _searchEngine.SearchAsync("postgresql", maxResults: 10, config);
        
        // Assert
        Assert.Equal(upperResults.TotalMatches, lowerResults.TotalMatches);
    }
    
    [Fact]
    public async Task Search_WithMaxResults_ShouldLimitResults()
    {
        // Arrange
        var config = await _configService.GetConfigAsync();
        var maxResults = 2;
        
        // Act
        var results = await _searchEngine.SearchAsync("the", maxResults, config);
        
        // Assert
        Assert.True(results.Matches.Count <= maxResults);
        Assert.True(results.TotalMatches >= results.Matches.Count);
    }
    
    [Fact]
    public async Task Search_ForNonExistentTerm_ShouldReturnEmpty()
    {
        // Arrange
        var config = await _configService.GetConfigAsync();
        
        // Act
        var results = await _searchEngine.SearchAsync("XYZ123NonExistent", maxResults: 10, config);
        
        // Assert
        Assert.NotNull(results);
        Assert.Equal(0, results.TotalMatches);
        Assert.Empty(results.Matches);
    }
    
    [Fact]
    public async Task Search_ShouldIncludeContext()
    {
        // This tests that search results include surrounding context
        
        // Arrange
        var config = await _configService.GetConfigAsync();
        
        // Act
        var results = await _searchEngine.SearchAsync("JWT Bearer", maxResults: 5, config);
        
        // Assert
        Assert.NotEmpty(results.Matches);
        foreach (var match in results.Matches)
        {
            Assert.NotEmpty(match.Context);
            // Context should be longer than just the matched line
            Assert.True(match.Context.Length > match.MatchedLine.Length);
        }
    }
    
    [Fact]
    public async Task Search_InCompactedFiles_ShouldAlsoWork()
    {
        // Verifies that search includes compacted archives
        
        // Arrange
        var config = await _configService.GetConfigAsync();
        
        // Act
        var results = await _searchEngine.SearchAsync("Q1 2024", maxResults: 10, config);
        
        // Assert
        Assert.True(results.TotalMatches > 0);
        Assert.Contains(results.Matches, m => m.FileName.Contains("ARCHIVED"));
    }
    
    [Fact]
    public async Task SearchFiles_WithPattern_ShouldFilterCorrectly()
    {
        // Tests the file pattern search functionality
        
        // Arrange
        var config = await _configService.GetConfigAsync();
        
        // Act
        var authFiles = await _searchEngine.SearchFilesAsync("*auth*", config);
        var apiFiles = await _searchEngine.SearchFilesAsync("*api*", config);
        
        // Assert
        Assert.NotEmpty(authFiles);
        Assert.Contains(authFiles, f => f.Contains("authentication"));
        
        Assert.NotEmpty(apiFiles);
        Assert.Contains(apiFiles, f => f.Contains("api-endpoints"));
    }
    
    public override void Dispose()
    {
        base.Dispose();
    }
}