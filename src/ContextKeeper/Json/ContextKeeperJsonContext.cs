using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using ContextKeeper.Config;
using ContextKeeper.Config.Models;
using ContextKeeper.Core;
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
[JsonSerializable(typeof(WorkflowProfile))]
[JsonSerializable(typeof(List<WorkflowProfile>))]
[JsonSerializable(typeof(DetectionConfig))]
[JsonSerializable(typeof(PathConfig))]
[JsonSerializable(typeof(SnapshotConfig))]
[JsonSerializable(typeof(CompactionConfig))]
[JsonSerializable(typeof(HeaderConfig))]

// Core types
[JsonSerializable(typeof(CompactionStatus))]
[JsonSerializable(typeof(CompactionResult))]
[JsonSerializable(typeof(TimelineResult))]
[JsonSerializable(typeof(TimelineEvent))]
[JsonSerializable(typeof(List<TimelineEvent>))]
[JsonSerializable(typeof(SnapshotResult))]
[JsonSerializable(typeof(ComparisonResult))]
[JsonSerializable(typeof(SearchResult))]
[JsonSerializable(typeof(SearchMatch))]
[JsonSerializable(typeof(List<SearchMatch>))]
[JsonSerializable(typeof(EvolutionResult))]
[JsonSerializable(typeof(EvolutionStep))]
[JsonSerializable(typeof(List<EvolutionStep>))]

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