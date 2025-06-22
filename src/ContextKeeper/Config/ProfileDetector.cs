using Microsoft.Extensions.Logging;
using ContextKeeper.Config.Models;

namespace ContextKeeper.Config;

public class ProfileDetector
{
    private readonly ILogger<ProfileDetector> _logger;
    
    public ProfileDetector(ILogger<ProfileDetector> logger)
    {
        _logger = logger;
    }
    
    public async Task<WorkflowProfile?> DetectProfileAsync()
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        // Check for CLAUDE.md
        if (File.Exists(Path.Combine(currentDir, "CLAUDE.md")))
        {
            _logger.LogInformation("Detected CLAUDE.md project");
            return GetClaudeProfile();
        }
        
        // Check for README.md with specific patterns
        var readmePath = Path.Combine(currentDir, "README.md");
        if (File.Exists(readmePath))
        {
            var content = await File.ReadAllTextAsync(readmePath);
            if (content.Contains("## History") || content.Contains("## Changelog"))
            {
                _logger.LogInformation("Detected README-based project");
                return GetReadmeProfile();
            }
        }
        
        // Check for docs directory
        if (Directory.Exists(Path.Combine(currentDir, "docs")))
        {
            _logger.LogInformation("Detected docs-based project");
            return GetDocsProfile();
        }
        
        return null;
    }
    
    public async Task<bool> MatchesProfileAsync(WorkflowProfile profile)
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        // Check detection files
        foreach (var file in profile.Detection.Files)
        {
            if (!File.Exists(Path.Combine(currentDir, file)))
            {
                return false;
            }
        }
        
        // Check detection paths
        foreach (var path in profile.Detection.Paths)
        {
            if (!Directory.Exists(Path.Combine(currentDir, path)))
            {
                return false;
            }
        }
        
        return true;
    }
    
    private WorkflowProfile GetClaudeProfile()
    {
        return new WorkflowProfile
        {
            Name = "claude-workflow",
            Description = "LSM-tree inspired development history for CLAUDE.md projects",
            Detection = new DetectionConfig
            {
                Files = new List<string> { "CLAUDE.md" },
                Paths = new List<string> { "FeatureData/DataHistory" }
            },
            Paths = new PathConfig
            {
                History = "FeatureData/DataHistory",
                Snapshots = "FeatureData/DataHistory/CLAUDE",
                Compacted = "FeatureData/DataHistory/Compacted"
            },
            Snapshot = new SnapshotConfig
            {
                Prefix = "CLAUDE_",
                DateFormat = "yyyy-MM-dd",
                FilenamePattern = "{prefix}{date}_{milestone}.md",
                Validation = @"^[a-z0-9-]+$",
                MaxLength = 50
            },
            Compaction = new CompactionConfig
            {
                Threshold = 10,
                Strategy = "lsm-quarterly",
                ArchivePath = "Archived_{quarter}"
            }
        };
    }
    
    private WorkflowProfile GetReadmeProfile()
    {
        return new WorkflowProfile
        {
            Name = "readme-workflow",
            Description = "History tracking for README-based projects",
            Detection = new DetectionConfig
            {
                Files = new List<string> { "README.md" },
                Paths = new List<string> { }
            },
            Paths = new PathConfig
            {
                History = ".history",
                Snapshots = ".history/snapshots",
                Compacted = ".history/compacted"
            },
            Snapshot = new SnapshotConfig
            {
                Prefix = "README_",
                DateFormat = "yyyy-MM-dd",
                FilenamePattern = "{prefix}{date}_{milestone}.md",
                Validation = @"^[a-z0-9-]+$",
                MaxLength = 50
            },
            Compaction = new CompactionConfig
            {
                Threshold = 20,
                Strategy = "monthly",
                ArchivePath = "Archive_{year}-{month}"
            }
        };
    }
    
    private WorkflowProfile GetDocsProfile()
    {
        return new WorkflowProfile
        {
            Name = "docs-workflow",
            Description = "History tracking for documentation projects",
            Detection = new DetectionConfig
            {
                Files = new List<string> { },
                Paths = new List<string> { "docs" }
            },
            Paths = new PathConfig
            {
                History = "docs/.history",
                Snapshots = "docs/.history/snapshots",
                Compacted = "docs/.history/compacted"
            },
            Snapshot = new SnapshotConfig
            {
                Prefix = "DOCS_",
                DateFormat = "yyyy-MM-dd",
                FilenamePattern = "{prefix}{date}_{milestone}.md",
                Validation = @"^[a-z0-9-]+$",
                MaxLength = 50
            },
            Compaction = new CompactionConfig
            {
                Threshold = 15,
                Strategy = "quarterly",
                ArchivePath = "Archive_Q{quarter}"
            }
        };
    }
}