using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using ContextKeeper.Config.Models;

namespace ContextKeeper.Core;

public class EvolutionTracker
{
    private readonly ILogger<EvolutionTracker> _logger;
    
    public EvolutionTracker(ILogger<EvolutionTracker> logger)
    {
        _logger = logger;
    }
    
    public async Task<EvolutionResult> GetEvolutionAsync(string componentName, WorkflowProfile profile)
    {
        var result = new EvolutionResult
        {
            ComponentName = componentName,
            Steps = new List<EvolutionStep>()
        };
        
        var snapshotsDir = profile.Paths.Snapshots;
        if (!Directory.Exists(snapshotsDir))
        {
            result.Summary = "No history found";
            return result;
        }
        
        var files = Directory.GetFiles(snapshotsDir, "*.md").OrderBy(f => f);
        
        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);
            var date = ExtractDateFromFilename(file, profile);
            var milestone = ExtractMilestoneFromFilename(file, profile);
            
            if (content.Contains(componentName, StringComparison.OrdinalIgnoreCase))
            {
                var status = ExtractComponentStatus(content, componentName);
                result.Steps.Add(new EvolutionStep
                {
                    Date = date,
                    Milestone = milestone,
                    Status = status,
                    FileName = Path.GetFileName(file)
                });
            }
        }
        
        result.Summary = result.Steps.Count > 0
            ? $"Component found in {result.Steps.Count} snapshots"
            : "Component not found in history";
            
        return result;
    }
    
    public Task<TimelineResult> GetTimelineAsync(WorkflowProfile profile)
    {
        var timeline = new TimelineResult
        {
            Events = new List<TimelineEvent>()
        };
        
        var snapshotsDir = profile.Paths.Snapshots;
        if (!Directory.Exists(snapshotsDir))
        {
            return Task.FromResult(timeline);
        }
        
        var files = Directory.GetFiles(snapshotsDir, "*.md").OrderBy(f => f);
        
        foreach (var file in files)
        {
            var date = ExtractDateFromFilename(file, profile);
            var milestone = ExtractMilestoneFromFilename(file, profile);
            var isCompacted = file.Contains("COMPACTED");
            
            timeline.Events.Add(new TimelineEvent
            {
                Date = date,
                Milestone = milestone,
                FileName = Path.GetFileName(file),
                Type = isCompacted ? "Compacted" : "Snapshot"
            });
        }
        
        return Task.FromResult(timeline);
    }
    
    private DateTime ExtractDateFromFilename(string filename, WorkflowProfile profile)
    {
        var pattern = profile.Snapshot.FilenamePattern
            .Replace("{prefix}", Regex.Escape(profile.Snapshot.Prefix))
            .Replace("{date}", @"(\d{4}-\d{2}-\d{2})")
            .Replace("{milestone}", ".*");
            
        var match = Regex.Match(Path.GetFileName(filename), pattern);
        return match.Success ? DateTime.Parse(match.Groups[1].Value) : DateTime.MinValue;
    }
    
    private string ExtractMilestoneFromFilename(string filename, WorkflowProfile profile)
    {
        var pattern = profile.Snapshot.FilenamePattern
            .Replace("{prefix}", Regex.Escape(profile.Snapshot.Prefix))
            .Replace("{date}", @"\d{4}-\d{2}-\d{2}")
            .Replace("{milestone}", @"(.+)")
            .Replace(".md", @"\.md");
            
        var match = Regex.Match(Path.GetFileName(filename), pattern);
        return match.Success ? match.Groups[1].Value.Replace("-", " ") : "Unknown";
    }
    
    private string ExtractComponentStatus(string content, string componentName)
    {
        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            if (line.Contains(componentName, StringComparison.OrdinalIgnoreCase))
            {
                if (line.Contains("✅") || line.Contains("COMPLETED"))
                    return "Completed";
                if (line.Contains("🚧") || line.Contains("IN PROGRESS"))
                    return "In Progress";
                if (line.Contains("❌") || line.Contains("TODO"))
                    return "Planned";
            }
        }
        return "Mentioned";
    }
}

public class EvolutionResult
{
    public string ComponentName { get; set; } = "";
    public List<EvolutionStep> Steps { get; set; } = new();
    public string Summary { get; set; } = "";
}

public class EvolutionStep
{
    public DateTime Date { get; set; }
    public string Milestone { get; set; } = "";
    public string Status { get; set; } = "";
    public string FileName { get; set; } = "";
}

public class TimelineResult
{
    public List<TimelineEvent> Events { get; set; } = new();
}

public class TimelineEvent
{
    public DateTime Date { get; set; }
    public string Milestone { get; set; } = "";
    public string FileName { get; set; } = "";
    public string Type { get; set; } = "";
}