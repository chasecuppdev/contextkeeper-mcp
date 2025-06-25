using System.Text.Json.Serialization;

namespace ContextKeeper.Core;

/// <summary>
/// Status of compaction check.
/// </summary>
public class CompactionStatus
{
    [JsonPropertyName("snapshot_count")]
    public int SnapshotCount { get; set; }
    
    [JsonPropertyName("compaction_needed")]
    public bool CompactionNeeded { get; set; }
    
    [JsonPropertyName("oldest_snapshot")]
    public string? OldestSnapshot { get; set; }
    
    [JsonPropertyName("newest_snapshot")]
    public string? NewestSnapshot { get; set; }
    
    [JsonPropertyName("recommended_action")]
    public string RecommendedAction { get; set; } = "";
    
    [JsonPropertyName("threshold")]
    public int Threshold { get; set; }
    
    [JsonPropertyName("auto_compact_enabled")]
    public bool AutoCompactEnabled { get; set; }
}

/// <summary>
/// Result of compaction operation.
/// </summary>
public class CompactionResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
    
    [JsonPropertyName("compacted_file")]
    public string? CompactedFile { get; set; }
    
    [JsonPropertyName("archived_snapshots")]
    public List<string> ArchivedSnapshots { get; set; } = new();
}

