using System.Text.Json.Serialization;

namespace ContextKeeper.Core.Models;

/// <summary>
/// Result of a snapshot operation.
/// </summary>
public class SnapshotResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("snapshot_path")]
    public string? SnapshotPath { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
}

/// <summary>
/// Result of comparing two snapshots.
/// </summary>
public class ComparisonResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
    
    [JsonPropertyName("added_sections")]
    public List<string> AddedSections { get; set; } = new();
    
    [JsonPropertyName("removed_sections")]
    public List<string> RemovedSections { get; set; } = new();
    
    [JsonPropertyName("modified_sections")]
    public List<string> ModifiedSections { get; set; } = new();
}