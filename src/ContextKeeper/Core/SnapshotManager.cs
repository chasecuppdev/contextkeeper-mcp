using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;
using ContextKeeper.Config.Models;
using ContextKeeper.Core.Interfaces;
using ContextKeeper.Core.Models;
using ContextKeeper.Json;
using ContextKeeper.Utils;

namespace ContextKeeper.Core;

public class SnapshotManager : ISnapshotManager
{
    private readonly ILogger<SnapshotManager> _logger;
    private readonly IContextCaptureService _contextCapture;
    private readonly ICompactionEngine _compactionEngine;
    
    public SnapshotManager(
        ILogger<SnapshotManager> logger, 
        IContextCaptureService contextCapture,
        ICompactionEngine compactionEngine)
    {
        _logger = logger;
        _contextCapture = contextCapture;
        _compactionEngine = compactionEngine;
    }
    
    public async Task<SnapshotResult> CreateSnapshotAsync(string milestoneDescription, ContextKeeperConfig config)
    {
        // Validate milestone description
        var validation = ValidationHelpers.ValidateMilestone(
            milestoneDescription, 
            "^[a-zA-Z0-9-]+$", // Default validation pattern
            50 // Default max length
        );
        
        if (!validation.IsValid)
        {
            return new SnapshotResult
            {
                Success = false,
                Message = validation.Message
            };
        }
        
        try
        {
            // Capture the full development context
            var context = await _contextCapture.CaptureContextAsync("manual", milestoneDescription);
            
            // Create snapshot filename
            var date = DateTime.Now.ToString(config.Snapshot.DateFormat);
            var filename = config.Snapshot.FilenamePattern
                .Replace("{date}", date)
                .Replace("{type}", context.Type)
                .Replace("{milestone}", milestoneDescription);
                
            var snapshotsDir = Path.Combine(Directory.GetCurrentDirectory(), config.Paths.Snapshots);
            var snapshotPath = Path.Combine(snapshotsDir, filename);
            
            // Ensure directory exists
            Directory.CreateDirectory(snapshotsDir);
            
            // Create snapshot content
            var snapshotContent = await CreateSnapshotContentAsync(context, config);
            
            // Write snapshot file
            await File.WriteAllTextAsync(snapshotPath, snapshotContent);
            
            _logger.LogInformation("Created snapshot: {Path}", snapshotPath);
            
            // Trigger auto-compaction if needed
            if (config.Compaction.AutoCompact)
            {
                await TriggerAutoCompactionAsync(config);
            }
            
            return new SnapshotResult
            {
                Success = true,
                SnapshotPath = snapshotPath,
                Message = $"Snapshot created successfully at {filename}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create snapshot");
            return new SnapshotResult
            {
                Success = false,
                Message = $"Failed to create snapshot: {ex.Message}"
            };
        }
    }
    
    public async Task<ComparisonResult> CompareSnapshotsAsync(
        string snapshot1Name, 
        string snapshot2Name, 
        ContextKeeperConfig config)
    {
        try
        {
            var snapshotsDir = Path.Combine(Directory.GetCurrentDirectory(), config.Paths.Snapshots);
            var path1 = Path.Combine(snapshotsDir, snapshot1Name);
            var path2 = Path.Combine(snapshotsDir, snapshot2Name);
            
            if (!File.Exists(path1) || !File.Exists(path2))
            {
                return new ComparisonResult
                {
                    Success = false,
                    Message = "One or both snapshot files not found"
                };
            }
            
            var content1 = await File.ReadAllTextAsync(path1);
            var content2 = await File.ReadAllTextAsync(path2);
            
            // Extract sections and compare
            var sections1 = ExtractSections(content1);
            var sections2 = ExtractSections(content2);
            
            var added = sections2.Keys.Except(sections1.Keys).ToList();
            var removed = sections1.Keys.Except(sections2.Keys).ToList();
            var modified = sections1.Keys
                .Intersect(sections2.Keys)
                .Where(key => sections1[key] != sections2[key])
                .ToList();
            
            return new ComparisonResult
            {
                Success = true,
                AddedSections = added,
                RemovedSections = removed,
                ModifiedSections = modified,
                Message = $"Comparison complete: {added.Count} added, {removed.Count} removed, {modified.Count} modified"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compare snapshots");
            return new ComparisonResult
            {
                Success = false,
                Message = $"Failed to compare snapshots: {ex.Message}"
            };
        }
    }
    
    private async Task<string> CreateSnapshotContentAsync(DevelopmentContext context, ContextKeeperConfig config)
    {
        var content = new System.Text.StringBuilder();
        
        // Header
        content.AppendLine($"# Development Context Snapshot");
        content.AppendLine($"**Timestamp**: {context.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
        content.AppendLine($"**Type**: {context.Type}");
        content.AppendLine($"**Milestone**: {context.Milestone}");
        content.AppendLine();
        
        // Git Information
        if (!string.IsNullOrEmpty(context.Git.Branch))
        {
            content.AppendLine("## Git Context");
            content.AppendLine($"- **Branch**: {context.Git.Branch}");
            content.AppendLine($"- **Commit**: {context.Git.Commit}");
            content.AppendLine($"- **Message**: {context.Git.CommitMessage}");
            
            if (context.Git.UncommittedFiles.Any())
            {
                content.AppendLine($"- **Uncommitted Files**: {context.Git.UncommittedFiles.Count}");
                foreach (var file in context.Git.UncommittedFiles.Take(10))
                {
                    content.AppendLine($"  - {file}");
                }
                if (context.Git.UncommittedFiles.Count > 10)
                {
                    content.AppendLine($"  - ... and {context.Git.UncommittedFiles.Count - 10} more");
                }
            }
            content.AppendLine();
        }
        
        // Workspace Information
        content.AppendLine("## Workspace Context");
        content.AppendLine($"- **Working Directory**: {context.Workspace.WorkingDirectory}");
        if (context.Workspace.RecentCommands.Any())
        {
            content.AppendLine("- **Recent Commands**:");
            foreach (var cmd in context.Workspace.RecentCommands.Take(5))
            {
                content.AppendLine($"  - `{cmd.Command}`");
            }
        }
        content.AppendLine();
        
        // Documentation Files
        if (context.Documentation.Any())
        {
            content.AppendLine("## Documentation");
            foreach (var (file, docContent) in context.Documentation)
            {
                content.AppendLine($"### {file}");
                content.AppendLine();
                content.AppendLine(docContent);
                content.AppendLine();
                content.AppendLine("---");
                content.AppendLine();
            }
        }
        
        // Metadata as JSON footer
        content.AppendLine("## Context Metadata");
        content.AppendLine("```json");
        var metadataJson = JsonSerializer.Serialize(context, ContextKeeperJsonContext.Default.DevelopmentContext);
        content.AppendLine(metadataJson);
        content.AppendLine("```");
        
        return content.ToString();
    }
    
    private Dictionary<string, string> ExtractSections(string content)
    {
        var sections = new Dictionary<string, string>();
        var lines = content.Split('\n');
        var currentSection = "";
        var sectionContent = new System.Text.StringBuilder();
        
        foreach (var line in lines)
        {
            if (line.StartsWith("## ") || line.StartsWith("### "))
            {
                if (!string.IsNullOrEmpty(currentSection))
                {
                    sections[currentSection] = sectionContent.ToString().Trim();
                }
                currentSection = line.TrimStart('#').Trim();
                sectionContent.Clear();
            }
            else
            {
                sectionContent.AppendLine(line);
            }
        }
        
        if (!string.IsNullOrEmpty(currentSection))
        {
            sections[currentSection] = sectionContent.ToString().Trim();
        }
        
        return sections;
    }
    
    private async Task TriggerAutoCompactionAsync(ContextKeeperConfig config)
    {
        try
        {
            var status = await _compactionEngine.CheckCompactionNeededAsync(config);
            if (status.CompactionNeeded && status.AutoCompactEnabled)
            {
                _logger.LogInformation("Auto-compaction triggered");
                var result = await _compactionEngine.PerformCompactionAsync(config);
                
                if (result.Success)
                {
                    _logger.LogInformation("Auto-compaction completed: {Message}", result.Message);
                }
                else
                {
                    _logger.LogWarning("Auto-compaction failed: {Message}", result.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during auto-compaction check");
            // Don't fail the snapshot operation due to compaction errors
        }
    }
}