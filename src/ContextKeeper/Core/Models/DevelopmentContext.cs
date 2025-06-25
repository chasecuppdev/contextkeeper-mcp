using System.Text.Json.Serialization;

namespace ContextKeeper.Core.Models;

/// <summary>
/// Represents the complete development context at a point in time.
/// This captures everything an AI assistant needs to understand the current state.
/// </summary>
public class DevelopmentContext
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("snapshot_id")]
    public string SnapshotId { get; set; } = "";
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = "manual";
    
    [JsonPropertyName("milestone")]
    public string Milestone { get; set; } = "";
    
    [JsonPropertyName("workspace")]
    public WorkspaceContext Workspace { get; set; } = new();
    
    [JsonPropertyName("git")]
    public GitContext Git { get; set; } = new();
    
    [JsonPropertyName("documentation")]
    public Dictionary<string, string> Documentation { get; set; } = new();
    
    [JsonPropertyName("metadata")]
    public ContextMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Captures the state of the development workspace.
/// </summary>
public class WorkspaceContext
{
    [JsonPropertyName("working_directory")]
    public string WorkingDirectory { get; set; } = "";
    
    [JsonPropertyName("open_files")]
    public List<FileContext> OpenFiles { get; set; } = new();
    
    [JsonPropertyName("active_file")]
    public string ActiveFile { get; set; } = "";
    
    [JsonPropertyName("active_position")]
    public FilePosition? ActivePosition { get; set; }
    
    [JsonPropertyName("recent_commands")]
    public List<CommandHistory> RecentCommands { get; set; } = new();
}

/// <summary>
/// Represents a file that was open during snapshot.
/// </summary>
public class FileContext
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = "";
    
    [JsonPropertyName("relative_path")]
    public string RelativePath { get; set; } = "";
    
    [JsonPropertyName("cursor_line")]
    public int CursorLine { get; set; }
    
    [JsonPropertyName("cursor_column")]
    public int CursorColumn { get; set; }
    
    [JsonPropertyName("language")]
    public string Language { get; set; } = "";
    
    [JsonPropertyName("is_modified")]
    public bool IsModified { get; set; }
}

/// <summary>
/// Represents a position in a file.
/// </summary>
public class FilePosition
{
    [JsonPropertyName("line")]
    public int Line { get; set; }
    
    [JsonPropertyName("column")]
    public int Column { get; set; }
    
    [JsonPropertyName("context_snippet")]
    public string ContextSnippet { get; set; } = "";
}

/// <summary>
/// Captures git repository state.
/// </summary>
public class GitContext
{
    [JsonPropertyName("branch")]
    public string Branch { get; set; } = "";
    
    [JsonPropertyName("commit")]
    public string Commit { get; set; } = "";
    
    [JsonPropertyName("commit_message")]
    public string CommitMessage { get; set; } = "";
    
    [JsonPropertyName("uncommitted_files")]
    public List<string> UncommittedFiles { get; set; } = new();
    
    [JsonPropertyName("staged_files")]
    public List<string> StagedFiles { get; set; } = new();
    
    [JsonPropertyName("recent_commits")]
    public List<CommitInfo> RecentCommits { get; set; } = new();
    
    [JsonPropertyName("remotes")]
    public Dictionary<string, string> Remotes { get; set; } = new();
}

/// <summary>
/// Information about a git commit.
/// </summary>
public class CommitInfo
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = "";
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
    
    [JsonPropertyName("author")]
    public string Author { get; set; } = "";
    
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }
}

/// <summary>
/// Represents a command that was executed.
/// </summary>
public class CommandHistory
{
    [JsonPropertyName("command")]
    public string Command { get; set; } = "";
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonPropertyName("exit_code")]
    public int? ExitCode { get; set; }
    
    [JsonPropertyName("working_directory")]
    public string WorkingDirectory { get; set; } = "";
}

/// <summary>
/// Additional metadata about the context.
/// </summary>
public class ContextMetadata
{
    [JsonPropertyName("project_name")]
    public string ProjectName { get; set; } = "";
    
    [JsonPropertyName("context_keeper_version")]
    public string ContextKeeperVersion { get; set; } = "2.0";
    
    [JsonPropertyName("os")]
    public string OperatingSystem { get; set; } = "";
    
    [JsonPropertyName("machine")]
    public string Machine { get; set; } = "";
    
    [JsonPropertyName("user")]
    public string User { get; set; } = "";
    
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();
}