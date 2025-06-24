using ContextKeeper.Config;
using ContextKeeper.Config.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace ContextKeeper.Tests.Helpers;

/// <summary>
/// Provides mock configuration services for testing without file I/O.
/// </summary>
public static class MockConfigurationService
{
    /// <summary>
    /// Creates a mock IConfigurationService that returns built-in profiles without file I/O.
    /// </summary>
    public static Mock<IConfigurationService> Create()
    {
        var mock = new Mock<IConfigurationService>();
        
        // Setup GetActiveProfileAsync to return claude-workflow by default
        mock.Setup(x => x.GetActiveProfileAsync())
            .ReturnsAsync(GetClaudeProfile());
        
        // Setup GetProfileAsync to return appropriate profiles
        mock.Setup(x => x.GetProfileAsync("claude-workflow"))
            .ReturnsAsync(GetClaudeProfile());
        mock.Setup(x => x.GetProfileAsync("readme-workflow"))
            .ReturnsAsync(GetReadmeProfile());
        mock.Setup(x => x.GetProfileAsync(It.IsNotIn("claude-workflow", "readme-workflow")))
            .ReturnsAsync((WorkflowProfile?)null);
        
        // Setup DetectProfileAsync to return claude-workflow if CLAUDE.md exists
        mock.Setup(x => x.DetectProfileAsync())
            .ReturnsAsync(() =>
            {
                if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "CLAUDE.md")))
                    return GetClaudeProfile();
                if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "README.md")))
                    return GetReadmeProfile();
                return null;
            });
        
        // Setup LoadConfigAsync to return both profiles
        mock.Setup(x => x.LoadConfigAsync())
            .ReturnsAsync(new ContextKeeperConfig
            {
                Version = "1.0",
                DefaultProfile = "claude-workflow",
                Profiles = new Dictionary<string, WorkflowProfile>
                {
                    ["claude-workflow"] = GetClaudeProfile(),
                    ["readme-workflow"] = GetReadmeProfile()
                }
            });
        
        return mock;
    }
    
    /// <summary>
    /// Gets the default claude-workflow profile.
    /// </summary>
    public static WorkflowProfile GetClaudeProfile()
    {
        return new WorkflowProfile
        {
            Name = "claude-workflow",
            Description = "LSM-tree inspired development history for CLAUDE.md projects",
            Detection = new DetectionConfig
            {
                Files = new List<string> { "CLAUDE.md" },
                Paths = new List<string> { ".contextkeeper" }
            },
            Paths = new PathConfig
            {
                History = ".contextkeeper/claude-workflow",
                Snapshots = ".contextkeeper/claude-workflow/snapshots",
                Compacted = ".contextkeeper/claude-workflow/compacted"
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
            },
            Header = new HeaderConfig
            {
                Template = @"# {document} Historical Snapshot
**Date**: {date}
**Milestone**: {milestone}
**Previous State**: {previous}
**Compaction Status**: {status}

## Changes in This Version
- [To be filled by developer]

## Context for Future Reference
- [To be filled by developer]

---
{content}"
            }
        };
    }
    
    /// <summary>
    /// Gets the default readme-workflow profile.
    /// </summary>
    public static WorkflowProfile GetReadmeProfile()
    {
        return new WorkflowProfile
        {
            Name = "readme-workflow",
            Description = "Standard documentation workflow for README.md based projects",
            Detection = new DetectionConfig
            {
                Files = new List<string> { "README.md" },
                Paths = new List<string> { }
            },
            Paths = new PathConfig
            {
                History = ".contextkeeper/readme-workflow",
                Snapshots = ".contextkeeper/readme-workflow/snapshots",
                Compacted = ".contextkeeper/readme-workflow/compacted"
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
                Strategy = "lsm-yearly",
                ArchivePath = "Archived_{year}"
            },
            Header = new HeaderConfig
            {
                Template = @"# {document} Documentation Snapshot
**Date**: {date}
**Milestone**: {milestone}
**Previous State**: {previous}

## Summary of Changes
- [To be filled by developer]

---
{content}"
            }
        };
    }
}