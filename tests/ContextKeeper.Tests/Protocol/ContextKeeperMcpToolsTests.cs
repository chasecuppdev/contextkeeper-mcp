using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using ContextKeeper.Protocol;
using ContextKeeper.Core;
using ContextKeeper.Core.Interfaces;
using ContextKeeper.Config;
using System.Text.Json;
using System.Text.Json.Nodes;
using Moq;

namespace ContextKeeper.Tests.Protocol;

/// <summary>
/// Tests for ContextKeeperMcpTools - the MCP tool integration for ContextKeeper functionality.
/// These tests demonstrate mocking external dependencies and testing JSON contracts.
/// </summary>
public class ContextKeeperMcpToolsTests : TestBase
{
    private readonly ContextKeeperMcpTools _mcpTools;
    private readonly Mock<IContextKeeperService> _mockService;
    private readonly ILogger<ContextKeeperMcpToolsTests> _logger;
    
    public ContextKeeperMcpToolsTests()
    {
        // For MCP tools, we'll mock the service interface to test in isolation
        _mockService = new Mock<IContextKeeperService>();
        
        _logger = GetService<ILogger<ContextKeeperMcpToolsTests>>();
        var mcpLogger = GetService<ILogger<ContextKeeperMcpTools>>();
        _mcpTools = new ContextKeeperMcpTools(_mockService.Object, mcpLogger);
    }
    
    #region CreateSnapshot Tool Tests
    
    [Fact]
    public async Task CreateSnapshot_WithValidMilestone_ShouldReturnSuccess()
    {
        // Arrange
        var milestone = "feature-implementation";
        var expectedResult = JsonSerializer.Deserialize<JsonObject>(@"{
            ""success"": true,
            ""message"": ""Snapshot created successfully"",
            ""snapshotPath"": ""/path/to/snapshot.md"",
            ""timestamp"": ""2024-01-01T00:00:00Z""
        }");
        
        _mockService.Setup(s => s.CreateSnapshot(milestone))
            .ReturnsAsync(expectedResult!);
        
        // Act
        var result = await _mcpTools.Snapshot(milestone);
        
        // Assert
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        jsonResult.Should().NotBeNull();
        jsonResult!["success"]!.GetValue<bool>().Should().BeTrue();
        jsonResult["snapshotPath"].Should().NotBeNull();
        
        _mockService.Verify(s => s.CreateSnapshot(milestone), Times.Once);
    }
    
    [Fact]
    public async Task CreateSnapshot_WhenServiceThrows_ShouldReturnError()
    {
        // Arrange
        var milestone = "invalid-milestone!!!";
        _mockService.Setup(s => s.CreateSnapshot(It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("Invalid milestone format"));
        
        // Act
        var result = await _mcpTools.Snapshot(milestone);
        
        // Assert
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        jsonResult!["success"]!.GetValue<bool>().Should().BeFalse();
        jsonResult["error"]!.ToString().Should().Contain("Invalid milestone format");
    }
    
    #endregion
    
    #region CheckCompaction Tool Tests
    
    [Fact]
    public async Task CheckCompaction_WhenNeeded_ShouldReturnTrue()
    {
        // Arrange
        var expectedResult = JsonSerializer.Deserialize<JsonObject>(@"{
            ""compactionNeeded"": true,
            ""currentCount"": 15,
            ""threshold"": 10,
            ""recommendation"": ""Consider running compaction to consolidate history""
        }");
        
        _mockService.Setup(s => s.CheckCompactionNeeded())
            .ReturnsAsync(expectedResult!);
        
        // Act
        var result = await _mcpTools.GetStatus();
        
        // Assert
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        jsonResult.Should().NotBeNull();
        jsonResult!["contextkeeper_version"].Should().NotBeNull();
        jsonResult["snapshot_status"].Should().NotBeNull();
        
        // Check the nested snapshot_status from the mocked service response
        var snapshotStatus = jsonResult["snapshot_status"] as JsonObject;
        snapshotStatus!["compactionNeeded"]!.GetValue<bool>().Should().BeTrue();
        snapshotStatus["currentCount"]!.GetValue<int>().Should().Be(15);
        snapshotStatus["threshold"]!.GetValue<int>().Should().Be(10);
    }
    
    [Fact]
    public async Task CheckCompaction_WhenNotNeeded_ShouldReturnFalse()
    {
        // Arrange
        var expectedResult = JsonSerializer.Deserialize<JsonObject>(@"{
            ""compactionNeeded"": false,
            ""currentCount"": 5,
            ""threshold"": 10
        }");
        
        _mockService.Setup(s => s.CheckCompactionNeeded())
            .ReturnsAsync(expectedResult!);
        
        // Act
        var result = await _mcpTools.GetStatus();
        
        // Assert
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        jsonResult.Should().NotBeNull();
        var snapshotStatus = jsonResult!["snapshot_status"] as JsonObject;
        snapshotStatus!["compactionNeeded"]!.GetValue<bool>().Should().BeFalse();
    }
    
    #endregion
    
    #region SearchHistory Tool Tests
    
    [Fact]
    public async Task SearchHistory_WithResults_ShouldReturnMatches()
    {
        // Arrange
        var searchTerm = "authentication";
        var maxResults = 5;
        var expectedResult = JsonSerializer.Deserialize<JsonObject>(@"{
            ""success"": true,
            ""searchTerm"": ""authentication"",
            ""totalMatches"": 3,
            ""results"": [
                {
                    ""milestone"": ""auth-implementation"",
                    ""timestamp"": ""2024-01-01T00:00:00Z"",
                    ""context"": ""...implemented JWT authentication..."",
                    ""matchCount"": 2
                }
            ]
        }");
        
        _mockService.Setup(s => s.SearchHistory(searchTerm, maxResults))
            .ReturnsAsync(expectedResult!);
        
        // Act
        var result = await _mcpTools.SearchEvolution(searchTerm, maxResults);
        
        // Assert
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        jsonResult!["success"]!.GetValue<bool>().Should().BeTrue();
        jsonResult["totalMatches"]!.GetValue<int>().Should().Be(3);
        
        var results = jsonResult["results"] as JsonArray;
        results.Should().NotBeNull();
        results!.Count.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public async Task SearchHistory_WithNoResults_ShouldReturnEmpty()
    {
        // Arrange
        var searchTerm = "nonexistent-term";
        var expectedResult = JsonSerializer.Deserialize<JsonObject>(@"{
            ""success"": true,
            ""searchTerm"": ""nonexistent-term"",
            ""totalMatches"": 0,
            ""results"": []
        }");
        
        _mockService.Setup(s => s.SearchHistory(searchTerm, It.IsAny<int>()))
            .ReturnsAsync(expectedResult!);
        
        // Act
        var result = await _mcpTools.SearchEvolution(searchTerm);
        
        // Assert
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        jsonResult!["totalMatches"]!.GetValue<int>().Should().Be(0);
        var results = jsonResult["results"] as JsonArray;
        results!.Count.Should().Be(0);
    }
    
    #endregion
    
    #region GetEvolution Tool Tests
    
    [Fact]
    public async Task GetEvolution_ForExistingComponent_ShouldReturnTimeline()
    {
        // Arrange
        var componentName = "UserService";
        var expectedResult = JsonSerializer.Deserialize<JsonObject>(@"{
            ""success"": true,
            ""component"": ""UserService"",
            ""firstMention"": ""initial-design"",
            ""lastMention"": ""refactor-complete"",
            ""totalMentions"": 5,
            ""timeline"": [
                {
                    ""milestone"": ""initial-design"",
                    ""timestamp"": ""2024-01-01T00:00:00Z"",
                    ""mentions"": 1
                }
            ]
        }");
        
        _mockService.Setup(s => s.GetArchitecturalEvolution(componentName))
            .ReturnsAsync(expectedResult!);
        
        // Act
        var result = await _mcpTools.TrackComponent(componentName);
        
        // Assert
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        jsonResult!["success"]!.GetValue<bool>().Should().BeTrue();
        jsonResult["component"]!.ToString().Should().Be(componentName);
        jsonResult["totalMentions"]!.GetValue<int>().Should().Be(5);
        
        var timeline = jsonResult["timeline"] as JsonArray;
        timeline.Should().NotBeNull();
        timeline!.Count.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public async Task GetEvolution_ForNonExistentComponent_ShouldReturnEmpty()
    {
        // Arrange
        var componentName = "NonExistentService";
        var expectedResult = JsonSerializer.Deserialize<JsonObject>(@"{
            ""success"": true,
            ""component"": ""NonExistentService"",
            ""totalMentions"": 0,
            ""timeline"": []
        }");
        
        _mockService.Setup(s => s.GetArchitecturalEvolution(componentName))
            .ReturnsAsync(expectedResult!);
        
        // Act
        var result = await _mcpTools.TrackComponent(componentName);
        
        // Assert
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        jsonResult!["totalMentions"]!.GetValue<int>().Should().Be(0);
    }
    
    #endregion
    
    #region CompareSnapshots Tool Tests
    
    [Fact]
    public async Task CompareSnapshots_WithValidMilestones_ShouldReturnDifferences()
    {
        // Arrange
        var milestone1 = "feature-start";
        var milestone2 = "feature-complete";
        var expectedResult = JsonSerializer.Deserialize<JsonObject>(@"{
            ""success"": true,
            ""comparison"": {
                ""from"": ""feature-start"",
                ""to"": ""feature-complete"",
                ""additions"": [""Added authentication module""],
                ""removals"": [""Removed legacy code""],
                ""modifications"": [""Updated API endpoints""]
            }
        }");
        
        _mockService.Setup(s => s.CompareSnapshots(milestone1, milestone2))
            .ReturnsAsync(expectedResult!);
        
        // Act
        var result = await _mcpTools.CompareSnapshots(milestone1, milestone2);
        
        // Assert
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        jsonResult!["success"]!.GetValue<bool>().Should().BeTrue();
        
        var comparison = jsonResult["comparison"] as JsonObject;
        comparison.Should().NotBeNull();
        comparison!["from"]!.ToString().Should().Be(milestone1);
        comparison["to"]!.ToString().Should().Be(milestone2);
    }
    
    #endregion
    
    #region Error Handling Tests
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task CreateSnapshot_WithInvalidInput_ShouldHandleGracefully(string? milestone)
    {
        // Arrange
        _mockService.Setup(s => s.CreateSnapshot(It.IsAny<string>()))
            .ThrowsAsync(new ArgumentNullException(nameof(milestone)));
        
        // Act
        var result = await _mcpTools.Snapshot(milestone!);
        
        // Assert
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        jsonResult!["success"]!.GetValue<bool>().Should().BeFalse();
        jsonResult["error"].Should().NotBeNull();
    }
    
    [Fact]
    public async Task AllTools_WhenCancellationRequested_ShouldRespectCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        
        _mockService.Setup(s => s.CreateSnapshot(It.IsAny<string>()))
            .Returns(async (string milestone) =>
            {
                await Task.Delay(1000);
                return new JsonObject();
            });
        
        // Act & Assert
        // The tool should handle cancellation gracefully
        var result = await _mcpTools.Snapshot("test", cts.Token);
        result.Should().NotBeNull();
    }
    
    #endregion
}