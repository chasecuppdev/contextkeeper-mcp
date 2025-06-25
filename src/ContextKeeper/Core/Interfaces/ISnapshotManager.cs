using ContextKeeper.Config.Models;
using ContextKeeper.Core.Models;

namespace ContextKeeper.Core.Interfaces;

/// <summary>
/// Interface for managing project documentation snapshots.
/// This abstraction allows for easier testing and future implementations.
/// </summary>
public interface ISnapshotManager
{
    /// <summary>
    /// Creates a snapshot of the current project state.
    /// </summary>
    /// <param name="milestoneDescription">Description of the milestone (must match validation pattern)</param>
    /// <param name="config">The context keeper configuration to use</param>
    /// <returns>Result containing success status and snapshot path</returns>
    Task<SnapshotResult> CreateSnapshotAsync(string milestoneDescription, ContextKeeperConfig config);
    
    /// <summary>
    /// Compares two snapshots and identifies changes.
    /// </summary>
    /// <param name="snapshot1">First snapshot filename</param>
    /// <param name="snapshot2">Second snapshot filename</param>
    /// <param name="config">The context keeper configuration to use</param>
    /// <returns>Comparison result with added/removed/modified sections</returns>
    Task<ComparisonResult> CompareSnapshotsAsync(string snapshot1, string snapshot2, ContextKeeperConfig config);
}