using ContextKeeper.Config.Models;

namespace ContextKeeper.Core.Interfaces;

/// <summary>
/// Provides functionality to manage snapshot compaction using LSM-tree inspired strategies.
/// </summary>
public interface ICompactionEngine
{
    /// <summary>
    /// Checks whether compaction is needed based on the current snapshot count and threshold.
    /// </summary>
    /// <param name="config">The context keeper configuration containing compaction settings.</param>
    /// <returns>The compaction status indicating whether compaction is needed.</returns>
    Task<CompactionStatus> CheckCompactionNeededAsync(ContextKeeperConfig config);
    
    /// <summary>
    /// Performs compaction by consolidating multiple snapshots into a single compacted file.
    /// </summary>
    /// <param name="config">The context keeper configuration containing compaction settings.</param>
    /// <returns>The result of the compaction operation.</returns>
    Task<CompactionResult> PerformCompactionAsync(ContextKeeperConfig config);
}