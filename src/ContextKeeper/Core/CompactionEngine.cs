using Microsoft.Extensions.Logging;
using ContextKeeper.Config.Models;
using ContextKeeper.Core.Interfaces;

namespace ContextKeeper.Core;

public class CompactionEngine : ICompactionEngine
{
    private readonly ILogger<CompactionEngine> _logger;
    
    public CompactionEngine(ILogger<CompactionEngine> logger)
    {
        _logger = logger;
    }
    
    public async Task<CompactionStatus> CheckCompactionNeededAsync(ContextKeeperConfig config)
    {
        var snapshotsDir = Path.Combine(Directory.GetCurrentDirectory(), config.Paths.Snapshots);
        if (!Directory.Exists(snapshotsDir))
        {
            return new CompactionStatus
            {
                SnapshotCount = 0,
                CompactionNeeded = false,
                RecommendedAction = "No snapshots found"
            };
        }
        
        var pattern = "SNAPSHOT_*.md";
        var snapshots = Directory.GetFiles(snapshotsDir, pattern)
            .OrderBy(f => f)
            .ToList();
        
        var threshold = config.Compaction.Threshold;
        var maxAgeInDays = config.Compaction.MaxAgeInDays;
        
        // Check for old snapshots
        var oldSnapshots = new List<string>();
        var cutoffDate = DateTime.Now.AddDays(-maxAgeInDays);
        
        foreach (var snapshot in snapshots)
        {
            var fileInfo = new FileInfo(snapshot);
            if (fileInfo.CreationTimeUtc < cutoffDate)
            {
                oldSnapshots.Add(snapshot);
            }
        }
        
        var compactionNeeded = snapshots.Count >= threshold || oldSnapshots.Count > 0;
        var reason = "";
        
        if (snapshots.Count >= threshold)
        {
            reason = $"Snapshot count ({snapshots.Count}) exceeds threshold ({threshold})";
        }
        else if (oldSnapshots.Count > 0)
        {
            reason = $"{oldSnapshots.Count} snapshots older than {maxAgeInDays} days";
        }
        
        return new CompactionStatus
        {
            SnapshotCount = snapshots.Count,
            CompactionNeeded = compactionNeeded,
            OldestSnapshot = snapshots.FirstOrDefault(),
            NewestSnapshot = snapshots.LastOrDefault(),
            RecommendedAction = compactionNeeded 
                ? $"Auto-compaction will trigger: {reason}"
                : $"No compaction needed - {snapshots.Count}/{threshold} snapshots",
            Threshold = threshold,
            AutoCompactEnabled = config.Compaction.AutoCompact
        };
    }
    
    public async Task<CompactionResult> PerformCompactionAsync(ContextKeeperConfig config)
    {
        var status = await CheckCompactionNeededAsync(config);
        
        // Only compact if auto-compact is enabled or if manually triggered
        if (!status.CompactionNeeded)
        {
            return new CompactionResult
            {
                Success = false,
                Message = "Compaction not needed"
            };
        }
        
        if (!config.Compaction.AutoCompact)
        {
            return new CompactionResult
            {
                Success = false,
                Message = "Auto-compaction is disabled. Enable it in configuration to proceed."
            };
        }
        
        try
        {
            var snapshotsDir = Path.Combine(Directory.GetCurrentDirectory(), config.Paths.Snapshots);
            var archivedDir = Path.Combine(Directory.GetCurrentDirectory(), config.Paths.Archived);
            Directory.CreateDirectory(archivedDir);
            
            // Get snapshots to compact (older than max age)
            var cutoffDate = DateTime.Now.AddDays(-config.Compaction.MaxAgeInDays);
            var snapshotsToCompact = Directory.GetFiles(snapshotsDir, "SNAPSHOT_*.md")
                .Where(f => new FileInfo(f).CreationTimeUtc < cutoffDate)
                .OrderBy(f => f)
                .ToList();
            
            if (snapshotsToCompact.Count == 0)
            {
                // If no old snapshots, compact oldest half if over threshold
                var allSnapshots = Directory.GetFiles(snapshotsDir, "SNAPSHOT_*.md")
                    .OrderBy(f => f)
                    .ToList();
                    
                if (allSnapshots.Count >= config.Compaction.Threshold)
                {
                    snapshotsToCompact = allSnapshots.Take(allSnapshots.Count / 2).ToList();
                }
            }
            
            if (snapshotsToCompact.Count == 0)
            {
                return new CompactionResult
                {
                    Success = false,
                    Message = "No snapshots ready for compaction"
                };
            }
            
            // Build compacted content
            var compactedContent = await BuildCompactedContentAsync(snapshotsToCompact, config);
            
            // Create compacted filename
            var dateRange = GetDateRange(snapshotsToCompact);
            var compactedFilename = $"ARCHIVED_{dateRange}_COMPACTED.md";
            var compactedPath = Path.Combine(archivedDir, compactedFilename);
            
            // Write compacted file
            await File.WriteAllTextAsync(compactedPath, compactedContent);
            
            // Delete original snapshots
            foreach (var snapshot in snapshotsToCompact)
            {
                File.Delete(snapshot);
            }
            
            _logger.LogInformation(
                "Auto-compacted {Count} snapshots into {Filename}", 
                snapshotsToCompact.Count, 
                compactedFilename);
            
            return new CompactionResult
            {
                Success = true,
                Message = $"Compacted {snapshotsToCompact.Count} snapshots",
                CompactedFile = compactedPath,
                ArchivedSnapshots = snapshotsToCompact
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform compaction");
            return new CompactionResult
            {
                Success = false,
                Message = $"Compaction failed: {ex.Message}"
            };
        }
    }
    
    private async Task<string> BuildCompactedContentAsync(List<string> snapshots, ContextKeeperConfig config)
    {
        var content = new System.Text.StringBuilder();
        
        // Header
        content.AppendLine("# Archived Snapshots");
        content.AppendLine($"**Archived Date**: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        content.AppendLine($"**Snapshot Count**: {snapshots.Count}");
        content.AppendLine($"**Date Range**: {GetDateRange(snapshots)}");
        content.AppendLine();
        content.AppendLine("## Summary");
        content.AppendLine("This file contains archived snapshots that have been compacted for long-term storage.");
        content.AppendLine();
        
        // Add each snapshot with separator
        foreach (var snapshot in snapshots)
        {
            content.AppendLine("---");
            content.AppendLine();
            content.AppendLine($"## Original File: {Path.GetFileName(snapshot)}");
            content.AppendLine();
            
            var snapshotContent = await File.ReadAllTextAsync(snapshot);
            content.AppendLine(snapshotContent);
            content.AppendLine();
        }
        
        return content.ToString();
    }
    
    private string GetDateRange(List<string> snapshots)
    {
        if (snapshots.Count == 0) return "unknown";
        
        // Extract dates from filenames (SNAPSHOT_yyyy-MM-dd_*)
        var dates = new List<DateTime>();
        foreach (var snapshot in snapshots)
        {
            var filename = Path.GetFileName(snapshot);
            var parts = filename.Split('_');
            if (parts.Length >= 2 && DateTime.TryParse(parts[1], out var date))
            {
                dates.Add(date);
            }
        }
        
        if (dates.Count == 0) return "unknown";
        
        var minDate = dates.Min();
        var maxDate = dates.Max();
        
        return minDate.Date == maxDate.Date 
            ? minDate.ToString("yyyy-MM-dd")
            : $"{minDate:yyyy-MM-dd}_to_{maxDate:yyyy-MM-dd}";
    }
}