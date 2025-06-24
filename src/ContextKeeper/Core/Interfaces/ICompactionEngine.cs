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
    /// <param name="profile">The workflow profile containing compaction configuration.</param>
    /// <returns>The compaction status indicating whether compaction is needed.</returns>
    Task<CompactionStatus> CheckCompactionNeededAsync(WorkflowProfile profile);
    
    /// <summary>
    /// Performs compaction by consolidating multiple snapshots into a single compacted file.
    /// </summary>
    /// <param name="profile">The workflow profile containing compaction configuration.</param>
    /// <returns>The result of the compaction operation.</returns>
    Task<CompactionResult> PerformCompactionAsync(WorkflowProfile profile);
}