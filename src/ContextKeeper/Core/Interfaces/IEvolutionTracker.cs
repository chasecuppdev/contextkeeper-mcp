using ContextKeeper.Config.Models;

namespace ContextKeeper.Core.Interfaces;

/// <summary>
/// Provides functionality to track the evolution of components across snapshots.
/// </summary>
public interface IEvolutionTracker
{
    /// <summary>
    /// Gets the evolution history of a specific component across all snapshots.
    /// </summary>
    /// <param name="componentName">The name of the component to track.</param>
    /// <param name="config">The context keeper configuration containing snapshot settings.</param>
    /// <returns>The evolution result containing all steps where the component was found.</returns>
    Task<EvolutionResult> GetEvolutionAsync(string componentName, ContextKeeperConfig config);
    
    /// <summary>
    /// Gets a timeline of all snapshots and compaction events.
    /// </summary>
    /// <param name="config">The context keeper configuration containing snapshot settings.</param>
    /// <returns>The timeline result containing all events in chronological order.</returns>
    Task<TimelineResult> GetTimelineAsync(ContextKeeperConfig config);
}