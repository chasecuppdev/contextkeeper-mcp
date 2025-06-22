using Xunit;
using ContextKeeper.Core;
using ContextKeeper.Config;

namespace ContextKeeper.Tests;

/// <summary>
/// Tests for search functionality across snapshots.
/// Demonstrates testing text search and indexing capabilities.
/// </summary>
public class SearchTests : TestBase
{
    private readonly SearchEngine _searchEngine;
    private readonly IConfigurationService _configService;
    
    public SearchTests()
    {
        _searchEngine = GetService<SearchEngine>();
        _configService = GetService<IConfigurationService>();
    }
    
    [Fact]
    public async Task Search_ForExistingTerm_ShouldReturnMatches()
    {
        // Arrange
        var profile = await _configService.GetActiveProfileAsync();
        var searchTerm = "PostgreSQL";
        
        // Act
        var results = await _searchEngine.SearchAsync(searchTerm, maxResults: 10, profile);
        
        // Assert
        Assert.NotNull(results);
        Assert.True(results.TotalMatches > 0);
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
        var profile = await _configService.GetActiveProfileAsync();
        
        // Act
        var results = await _searchEngine.SearchAsync(searchTerm, maxResults: 20, profile);
        
        // Assert
        Assert.True(results.TotalMatches >= minExpectedMatches, 
            $"Expected at least {minExpectedMatches} matches for '{searchTerm}', but found {results.TotalMatches}");
    }
    
    [Fact]
    public async Task Search_WithCaseInsensitive_ShouldWork()
    {
        // Arrange
        var profile = await _configService.GetActiveProfileAsync();
        
        // Act
        var upperResults = await _searchEngine.SearchAsync("POSTGRESQL", maxResults: 10, profile);
        var lowerResults = await _searchEngine.SearchAsync("postgresql", maxResults: 10, profile);
        
        // Assert
        Assert.Equal(upperResults.TotalMatches, lowerResults.TotalMatches);
    }
    
    [Fact]
    public async Task Search_WithMaxResults_ShouldLimitResults()
    {
        // Arrange
        var profile = await _configService.GetActiveProfileAsync();
        var maxResults = 2;
        
        // Act
        var results = await _searchEngine.SearchAsync("the", maxResults, profile);
        
        // Assert
        Assert.True(results.Matches.Count <= maxResults);
        Assert.True(results.TotalMatches >= results.Matches.Count);
    }
    
    [Fact]
    public async Task Search_ForNonExistentTerm_ShouldReturnEmpty()
    {
        // Arrange
        var profile = await _configService.GetActiveProfileAsync();
        
        // Act
        var results = await _searchEngine.SearchAsync("XYZ123NonExistent", maxResults: 10, profile);
        
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
        var profile = await _configService.GetActiveProfileAsync();
        
        // Act
        var results = await _searchEngine.SearchAsync("JWT Bearer", maxResults: 5, profile);
        
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
        var profile = await _configService.GetActiveProfileAsync();
        
        // Act
        var results = await _searchEngine.SearchAsync("Q1 2024", maxResults: 10, profile);
        
        // Assert
        Assert.True(results.TotalMatches > 0);
        Assert.Contains(results.Matches, m => m.FileName.Contains("COMPACTED"));
    }
    
    [Fact]
    public async Task SearchFiles_WithPattern_ShouldFilterCorrectly()
    {
        // Tests the file pattern search functionality
        
        // Arrange
        var profile = await _configService.GetActiveProfileAsync();
        
        // Act
        var authFiles = await _searchEngine.SearchFilesAsync("*auth*", profile);
        var apiFiles = await _searchEngine.SearchFilesAsync("*api*", profile);
        
        // Assert
        Assert.NotEmpty(authFiles);
        Assert.Contains(authFiles, f => f.Contains("add-authentication"));
        
        Assert.NotEmpty(apiFiles);
        Assert.Contains(apiFiles, f => f.Contains("api-endpoints"));
    }
}