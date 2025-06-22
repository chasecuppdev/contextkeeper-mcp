using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using ContextKeeper.Config.Models;
using ContextKeeper.Utils;

namespace ContextKeeper.Core;

public class SnapshotManager
{
    private readonly ILogger<SnapshotManager> _logger;
    
    public SnapshotManager(ILogger<SnapshotManager> logger)
    {
        _logger = logger;
    }
    
    public async Task<SnapshotResult> CreateSnapshotAsync(string milestoneDescription, WorkflowProfile profile)
    {
        // Validate milestone description
        var validation = ValidationHelpers.ValidateMilestone(
            milestoneDescription, 
            profile.Snapshot.Validation, 
            profile.Snapshot.MaxLength
        );
        
        if (!validation.IsValid)
        {
            return new SnapshotResult
            {
                Success = false,
                Message = validation.Message
            };
        }
        
        // Create snapshot filename
        var date = DateTime.Now.ToString(profile.Snapshot.DateFormat);
        var filename = profile.Snapshot.FilenamePattern
            .Replace("{prefix}", profile.Snapshot.Prefix)
            .Replace("{date}", date)
            .Replace("{milestone}", milestoneDescription);
            
        var snapshotPath = Path.Combine(profile.Paths.Snapshots, filename);
        
        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(snapshotPath)!);
        
        // Find the main document file
        var documentPath = FindMainDocument(profile);
        if (documentPath == null)
        {
            return new SnapshotResult
            {
                Success = false,
                Message = "Main document not found"
            };
        }
        
        // Read current content
        var currentContent = await File.ReadAllTextAsync(documentPath);
        
        // Get previous snapshot
        var previousSnapshot = GetPreviousSnapshot(profile);
        
        // Build snapshot content with header
        var snapshotContent = BuildSnapshotContent(
            profile, 
            currentContent, 
            date, 
            milestoneDescription, 
            previousSnapshot
        );
        
        // Write snapshot
        await File.WriteAllTextAsync(snapshotPath, snapshotContent);
        
        _logger.LogInformation($"Created snapshot: {filename}");
        
        return new SnapshotResult
        {
            Success = true,
            SnapshotPath = snapshotPath,
            Message = $"Snapshot created successfully: {filename}"
        };
    }
    
    public async Task<ComparisonResult> CompareSnapshotsAsync(
        string snapshot1, 
        string snapshot2, 
        WorkflowProfile profile)
    {
        var path1 = Path.Combine(profile.Paths.Snapshots, snapshot1);
        var path2 = Path.Combine(profile.Paths.Snapshots, snapshot2);
        
        if (!File.Exists(path1) || !File.Exists(path2))
        {
            return new ComparisonResult
            {
                Success = false,
                Message = "One or both snapshots not found"
            };
        }
        
        var content1 = await File.ReadAllTextAsync(path1);
        var content2 = await File.ReadAllTextAsync(path2);
        
        // Extract sections and compare
        var sections1 = ExtractSections(content1);
        var sections2 = ExtractSections(content2);
        
        var addedSections = sections2.Keys.Except(sections1.Keys).ToList();
        var removedSections = sections1.Keys.Except(sections2.Keys).ToList();
        var modifiedSections = sections1.Keys.Intersect(sections2.Keys)
            .Where(k => sections1[k] != sections2[k])
            .ToList();
        
        return new ComparisonResult
        {
            Success = true,
            AddedSections = addedSections,
            RemovedSections = removedSections,
            ModifiedSections = modifiedSections,
            Summary = $"Changes: {addedSections.Count} added, {removedSections.Count} removed, {modifiedSections.Count} modified"
        };
    }
    
    private string? FindMainDocument(WorkflowProfile profile)
    {
        // Check each detection file
        foreach (var detectionFile in profile.Detection.Files)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), detectionFile);
            if (File.Exists(path))
            {
                return path;
            }
        }
        
        return null;
    }
    
    private string GetPreviousSnapshot(WorkflowProfile profile)
    {
        var snapshotsDir = profile.Paths.Snapshots;
        if (!Directory.Exists(snapshotsDir))
            return "None";
            
        var pattern = profile.Snapshot.Prefix + "*.md";
        var lastSnapshot = Directory.GetFiles(snapshotsDir, pattern)
            .Where(f => !f.Contains("COMPACTED"))
            .OrderByDescending(f => f)
            .FirstOrDefault();
        
        return lastSnapshot != null ? Path.GetFileName(lastSnapshot) : "None";
    }
    
    private string BuildSnapshotContent(
        WorkflowProfile profile,
        string currentContent,
        string date,
        string milestoneDescription,
        string previousSnapshot)
    {
        if (string.IsNullOrEmpty(profile.Header?.Template))
        {
            // Default header if no template specified
            return $@"# Historical Snapshot
**Date**: {date}
**Milestone**: {milestoneDescription.Replace("-", " ")}
**Previous State**: {previousSnapshot}
**Profile**: {profile.Name}

---
{currentContent}";
        }
        
        // Use template
        var header = profile.Header.Template
            .Replace("{document}", Path.GetFileNameWithoutExtension(profile.Detection.Files.First()))
            .Replace("{date}", date)
            .Replace("{milestone}", milestoneDescription.Replace("-", " "))
            .Replace("{previous}", previousSnapshot)
            .Replace("{status}", "Individual Snapshot")
            .Replace("{content}", currentContent);
            
        return header;
    }
    
    private Dictionary<string, string> ExtractSections(string content)
    {
        var sections = new Dictionary<string, string>();
        var lines = content.Split('\n');
        string currentSection = "";
        var sectionContent = new List<string>();
        
        foreach (var line in lines)
        {
            if (line.StartsWith("## "))
            {
                if (!string.IsNullOrEmpty(currentSection))
                {
                    sections[currentSection] = string.Join("\n", sectionContent);
                }
                currentSection = line.Substring(3).Trim();
                sectionContent.Clear();
            }
            else
            {
                sectionContent.Add(line);
            }
        }
        
        if (!string.IsNullOrEmpty(currentSection))
        {
            sections[currentSection] = string.Join("\n", sectionContent);
        }
        
        return sections;
    }
}

public class SnapshotResult
{
    public bool Success { get; set; }
    public string? SnapshotPath { get; set; }
    public string Message { get; set; } = "";
}

public class ComparisonResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public List<string> AddedSections { get; set; } = new();
    public List<string> RemovedSections { get; set; } = new();
    public List<string> ModifiedSections { get; set; } = new();
    public string Summary { get; set; } = "";
}