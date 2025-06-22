using Microsoft.Extensions.Logging;
using ContextKeeper.Config.Models;

namespace ContextKeeper.Core;

public class CompactionEngine
{
    private readonly ILogger<CompactionEngine> _logger;
    
    public CompactionEngine(ILogger<CompactionEngine> logger)
    {
        _logger = logger;
    }
    
    public Task<CompactionStatus> CheckCompactionNeededAsync(WorkflowProfile profile)
    {
        var snapshotsDir = profile.Paths.Snapshots;
        if (!Directory.Exists(snapshotsDir))
        {
            return Task.FromResult(new CompactionStatus
            {
                SnapshotCount = 0,
                CompactionNeeded = false,
                RecommendedAction = "No snapshots found"
            });
        }
        
        var pattern = profile.Snapshot.Prefix + "*.md";
        var snapshots = Directory.GetFiles(snapshotsDir, pattern)
            .Where(f => !f.Contains("COMPACTED"))
            .OrderBy(f => f)
            .ToList();
        
        var threshold = profile.Compaction.Threshold;
        var compactionNeeded = snapshots.Count >= threshold;
        
        return Task.FromResult(new CompactionStatus
        {
            SnapshotCount = snapshots.Count,
            CompactionNeeded = compactionNeeded,
            OldestSnapshot = snapshots.FirstOrDefault(),
            NewestSnapshot = snapshots.LastOrDefault(),
            RecommendedAction = compactionNeeded 
                ? $"Compaction recommended - {snapshots.Count}/{threshold} snapshots exist"
                : $"No compaction needed - {snapshots.Count}/{threshold} snapshots",
            Threshold = threshold
        });
    }
    
    public async Task<CompactionResult> PerformCompactionAsync(WorkflowProfile profile)
    {
        var status = await CheckCompactionNeededAsync(profile);
        if (!status.CompactionNeeded)
        {
            return new CompactionResult
            {
                Success = false,
                Message = "Compaction not needed yet"
            };
        }
        
        try
        {
            var snapshotsDir = profile.Paths.Snapshots;
            var compactedDir = profile.Paths.Compacted ?? Path.Combine(snapshotsDir, "Compacted");
            Directory.CreateDirectory(compactedDir);
            
            var pattern = profile.Snapshot.Prefix + "*.md";
            var snapshots = Directory.GetFiles(snapshotsDir, pattern)
                .Where(f => !f.Contains("COMPACTED"))
                .OrderBy(f => f)
                .ToList();
            
            // Determine compaction strategy
            var compactedContent = await BuildCompactedContentAsync(snapshots, profile);
            
            // Create compacted filename based on strategy
            var compactedFilename = GenerateCompactedFilename(snapshots, profile);
            var compactedPath = Path.Combine(compactedDir, compactedFilename);
            
            // Write compacted file
            await File.WriteAllTextAsync(compactedPath, compactedContent);
            
            // Archive original snapshots
            var archiveDir = Path.Combine(compactedDir, profile.Compaction.ArchivePath.Replace("{quarter}", GetQuarter()));
            Directory.CreateDirectory(archiveDir);
            
            foreach (var snapshot in snapshots)
            {
                var destPath = Path.Combine(archiveDir, Path.GetFileName(snapshot));
                File.Move(snapshot, destPath);
            }
            
            _logger.LogInformation($"Compacted {snapshots.Count} snapshots into {compactedFilename}");
            
            return new CompactionResult
            {
                Success = true,
                Message = $"Successfully compacted {snapshots.Count} snapshots",
                CompactedFile = compactedFilename,
                ArchivedCount = snapshots.Count
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
    
    private async Task<string> BuildCompactedContentAsync(List<string> snapshots, WorkflowProfile profile)
    {
        var content = new List<string>();
        content.Add($"# Compacted History - {profile.Name} Profile");
        content.Add($"**Created**: {DateTime.Now:yyyy-MM-dd}");
        content.Add($"**Snapshots**: {snapshots.Count}");
        content.Add($"**Period**: {Path.GetFileName(snapshots.First())} to {Path.GetFileName(snapshots.Last())}");
        content.Add("");
        content.Add("## Summary of Changes");
        content.Add("");
        
        // Extract key changes from each snapshot
        foreach (var snapshot in snapshots)
        {
            var snapshotContent = await File.ReadAllTextAsync(snapshot);
            var filename = Path.GetFileName(snapshot);
            
            // Extract milestone and changes section
            var lines = snapshotContent.Split('\n');
            string? milestone = null;
            var inChangesSection = false;
            var changes = new List<string>();
            
            foreach (var line in lines)
            {
                if (line.StartsWith("**Milestone**:"))
                {
                    milestone = line.Replace("**Milestone**:", "").Trim();
                }
                else if (line.StartsWith("## Changes in This Version"))
                {
                    inChangesSection = true;
                }
                else if (inChangesSection && line.StartsWith("## "))
                {
                    break;
                }
                else if (inChangesSection && line.StartsWith("- "))
                {
                    changes.Add(line);
                }
            }
            
            if (milestone != null)
            {
                content.Add($"### {milestone}");
                content.Add($"*File: {filename}*");
                content.AddRange(changes.Any() ? changes : new[] { "- No changes documented" });
                content.Add("");
            }
        }
        
        content.Add("---");
        content.Add("");
        content.Add("## Latest State");
        content.Add("");
        
        // Include the latest snapshot's full content
        var latestSnapshot = await File.ReadAllTextAsync(snapshots.Last());
        content.Add(latestSnapshot);
        
        return string.Join("\n", content);
    }
    
    private string GenerateCompactedFilename(List<string> snapshots, WorkflowProfile profile)
    {
        var firstFile = Path.GetFileNameWithoutExtension(snapshots.First());
        var lastFile = Path.GetFileNameWithoutExtension(snapshots.Last());
        
        // Extract dates from filenames
        var firstDate = firstFile.Split('_')[1];
        var lastDate = lastFile.Split('_')[1];
        
        return $"{profile.Snapshot.Prefix}COMPACTED_{firstDate}_to_{lastDate}.md";
    }
    
    private string GetQuarter()
    {
        var month = DateTime.Now.Month;
        var quarter = (month - 1) / 3 + 1;
        return $"{DateTime.Now.Year}-Q{quarter}";
    }
}

public class CompactionStatus
{
    public int SnapshotCount { get; set; }
    public bool CompactionNeeded { get; set; }
    public string? OldestSnapshot { get; set; }
    public string? NewestSnapshot { get; set; }
    public string RecommendedAction { get; set; } = "";
    public int Threshold { get; set; }
}

public class CompactionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string? CompactedFile { get; set; }
    public int ArchivedCount { get; set; }
}