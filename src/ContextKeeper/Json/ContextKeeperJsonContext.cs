using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using ContextKeeper.Config;
using ContextKeeper.Config.Models;
using ContextKeeper.Core;
using ContextKeeper.Core.Models;
using ContextKeeper.Protocol;

namespace ContextKeeper.Json;

// Core JSON types
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(JsonArray))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(JsonValue))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(List<object>))]
[JsonSerializable(typeof(string[]))]

// Configuration types
[JsonSerializable(typeof(ContextKeeperConfig))]
[JsonSerializable(typeof(PathConfig))]
[JsonSerializable(typeof(SnapshotConfig))]
[JsonSerializable(typeof(CompactionConfig))]
[JsonSerializable(typeof(ContextTrackingConfig))]

// Development context types
[JsonSerializable(typeof(DevelopmentContext))]
[JsonSerializable(typeof(WorkspaceContext))]
[JsonSerializable(typeof(FileContext))]
[JsonSerializable(typeof(List<FileContext>))]
[JsonSerializable(typeof(FilePosition))]
[JsonSerializable(typeof(GitContext))]
[JsonSerializable(typeof(CommitInfo))]
[JsonSerializable(typeof(List<CommitInfo>))]
[JsonSerializable(typeof(CommandHistory))]
[JsonSerializable(typeof(List<CommandHistory>))]
[JsonSerializable(typeof(ContextMetadata))]

// Core types from CompactionModels
[JsonSerializable(typeof(CompactionStatus))]
[JsonSerializable(typeof(CompactionResult))]
[JsonSerializable(typeof(TimelineResult))]
[JsonSerializable(typeof(TimelineEvent))]
[JsonSerializable(typeof(List<TimelineEvent>))]
[JsonSerializable(typeof(SearchResult))]
[JsonSerializable(typeof(SearchMatch))]
[JsonSerializable(typeof(List<SearchMatch>))]
[JsonSerializable(typeof(EvolutionResult))]
[JsonSerializable(typeof(EvolutionStep))]
[JsonSerializable(typeof(List<EvolutionStep>))]

// Result types from Models
[JsonSerializable(typeof(SnapshotResult))]
[JsonSerializable(typeof(ComparisonResult))]

// Protocol types
[JsonSerializable(typeof(JsonRpcRequest))]
[JsonSerializable(typeof(JsonRpcResponse))]
[JsonSerializable(typeof(JsonRpcError))]

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class ContextKeeperJsonContext : JsonSerializerContext
{
}