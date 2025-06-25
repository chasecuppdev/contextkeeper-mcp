namespace ContextKeeper.Config.Models;

public class ContextKeeperConfig
{
    public string Version { get; set; } = "2.0";
    public PathConfig Paths { get; set; } = new();
    public SnapshotConfig Snapshot { get; set; } = new();
    public CompactionConfig Compaction { get; set; } = new();
    public ContextTrackingConfig ContextTracking { get; set; } = new();
}

public class PathConfig
{
    public string History { get; set; } = ".contextkeeper";
    public string Snapshots { get; set; } = ".contextkeeper/snapshots";
    public string Archived { get; set; } = ".contextkeeper/archived";
    public string UserWorkspace { get; set; } = "context-workspace/workspace";
}

public class SnapshotConfig
{
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string FilenamePattern { get; set; } = "SNAPSHOT_{date}_{type}_{milestone}.md";
    public bool AutoCapture { get; set; } = true;
    public int AutoCaptureIntervalMinutes { get; set; } = 30;
}

public class CompactionConfig
{
    public int Threshold { get; set; } = 20;
    public int MaxAgeInDays { get; set; } = 90;
    public bool AutoCompact { get; set; } = true;
}

public class ContextTrackingConfig
{
    public bool TrackOpenFiles { get; set; } = true;
    public bool TrackGitState { get; set; } = true;
    public bool TrackRecentCommands { get; set; } = true;
    public List<string> DocumentationFiles { get; set; } = new() { "*.md", "*.txt", "*.adoc" };
    public List<string> IgnorePatterns { get; set; } = new() { "node_modules", "bin", "obj", ".git" };
}