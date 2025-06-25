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
    /// Creates a mock IConfigurationService that returns default configuration without file I/O.
    /// </summary>
    public static Mock<IConfigurationService> Create()
    {
        var mock = new Mock<IConfigurationService>();
        
        // Setup GetConfigAsync to return default config
        mock.Setup(x => x.GetConfigAsync())
            .ReturnsAsync(GetDefaultConfig());
        
        // Setup SaveConfigAsync
        mock.Setup(x => x.SaveConfigAsync(It.IsAny<ContextKeeperConfig>()))
            .Returns(Task.CompletedTask);
        
        // Setup InitializeProjectAsync
        mock.Setup(x => x.InitializeProjectAsync())
            .Returns(Task.CompletedTask);
        
        return mock;
    }
    
    /// <summary>
    /// Gets the default configuration.
    /// </summary>
    public static ContextKeeperConfig GetDefaultConfig()
    {
        return new ContextKeeperConfig
        {
            Version = "2.0",
            Paths = new PathConfig
            {
                History = ".contextkeeper",
                Snapshots = ".contextkeeper/snapshots",
                Archived = ".contextkeeper/archived"
            },
            Snapshot = new SnapshotConfig
            {
                DateFormat = "yyyy-MM-dd",
                FilenamePattern = "SNAPSHOT_{date}_{type}_{milestone}.md",
                AutoCapture = true,
                AutoCaptureIntervalMinutes = 30
            },
            Compaction = new CompactionConfig
            {
                Threshold = 20,
                MaxAgeInDays = 90,
                AutoCompact = true
            },
            ContextTracking = new ContextTrackingConfig
            {
                TrackOpenFiles = true,
                TrackGitState = true,
                TrackRecentCommands = true,
                DocumentationFiles = new List<string> { "*.md", "*.txt", "*.adoc" },
                IgnorePatterns = new List<string> { "node_modules", "bin", "obj", ".git" }
            }
        };
    }
    
    /// <summary>
    /// Gets a configuration suitable for testing with lower thresholds.
    /// </summary>
    public static ContextKeeperConfig GetTestConfig()
    {
        var config = GetDefaultConfig();
        config.Compaction.Threshold = 5; // Lower threshold for testing
        return config;
    }
}