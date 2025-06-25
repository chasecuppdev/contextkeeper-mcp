using ContextKeeper.Config.Models;

namespace ContextKeeper.Core.Interfaces;

/// <summary>
/// Interface for searching through project snapshots.
/// Provides full-text search capabilities across historical data.
/// </summary>
public interface ISearchEngine
{
    /// <summary>
    /// Searches for a term across all snapshots.
    /// </summary>
    /// <param name="searchTerm">The term to search for</param>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <param name="config">The context keeper configuration to use</param>
    /// <returns>Search results with context</returns>
    Task<SearchResult> SearchAsync(string searchTerm, int maxResults, ContextKeeperConfig config);
    
    /// <summary>
    /// Searches for files matching a pattern.
    /// </summary>
    /// <param name="pattern">File pattern to match (e.g., "*.md")</param>
    /// <param name="config">The context keeper configuration to use</param>
    /// <returns>List of matching filenames</returns>
    Task<List<string>> SearchFilesAsync(string pattern, ContextKeeperConfig config);
}