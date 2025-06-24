using System.Text.Json.Nodes;

namespace ContextKeeper.Core.Interfaces;

/// <summary>
/// Provides the main service interface for ContextKeeper operations.
/// </summary>
public interface IContextKeeperService
{
    /// <summary>
    /// Creates a new snapshot with the given milestone description.
    /// </summary>
    /// <param name="milestoneDescription">The description of the milestone to snapshot.</param>
    /// <returns>A JSON object containing the operation result.</returns>
    Task<JsonObject> CreateSnapshot(string milestoneDescription);
    
    /// <summary>
    /// Checks whether compaction is needed based on the current snapshot count.
    /// </summary>
    /// <returns>A JSON object containing the compaction status.</returns>
    Task<JsonObject> CheckCompactionNeeded();
    
    /// <summary>
    /// Searches through snapshot history for the given search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <returns>A JSON object containing the search results.</returns>
    Task<JsonObject> SearchHistory(string searchTerm, int maxResults = 5);
    
    /// <summary>
    /// Gets the architectural evolution of a specific component.
    /// </summary>
    /// <param name="componentName">The name of the component to track.</param>
    /// <returns>A JSON object containing the evolution history.</returns>
    Task<JsonObject> GetArchitecturalEvolution(string componentName);
    
    /// <summary>
    /// Compares two snapshots and identifies differences.
    /// </summary>
    /// <param name="snapshot1">The first snapshot filename.</param>
    /// <param name="snapshot2">The second snapshot filename.</param>
    /// <returns>A JSON object containing the comparison results.</returns>
    Task<JsonObject> CompareSnapshots(string snapshot1, string snapshot2);
    
    /// <summary>
    /// Initializes a new ContextKeeper project with the specified profile.
    /// </summary>
    /// <param name="profileName">The name of the profile to use, or null to auto-detect.</param>
    /// <returns>A JSON object containing the initialization result.</returns>
    Task<JsonObject> InitializeProject(string? profileName = null);
}