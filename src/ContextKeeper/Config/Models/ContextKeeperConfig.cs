namespace ContextKeeper.Config.Models;

public class ContextKeeperConfig
{
    public string Version { get; set; } = "1.0";
    public string DefaultProfile { get; set; } = "claude-workflow";
    public Dictionary<string, WorkflowProfile> Profiles { get; set; } = new();
}

public class WorkflowProfile
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DetectionConfig Detection { get; set; } = new();
    public PathConfig Paths { get; set; } = new();
    public SnapshotConfig Snapshot { get; set; } = new();
    public CompactionConfig Compaction { get; set; } = new();
    public HeaderConfig? Header { get; set; }
}

public class DetectionConfig
{
    public List<string> Files { get; set; } = new();
    public List<string> Paths { get; set; } = new();
}

public class PathConfig
{
    public string History { get; set; } = "";
    public string Snapshots { get; set; } = "";
    public string? Compacted { get; set; }
}

public class SnapshotConfig
{
    public string Prefix { get; set; } = "";
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string FilenamePattern { get; set; } = "";
    public string Validation { get; set; } = @"^[a-z0-9-]+$";
    public int MaxLength { get; set; } = 50;
}

public class CompactionConfig
{
    public int Threshold { get; set; } = 10;
    public string Strategy { get; set; } = "lsm-quarterly";
    public string ArchivePath { get; set; } = "Archived_{quarter}";
}

public class HeaderConfig
{
    public string Template { get; set; } = "";
}