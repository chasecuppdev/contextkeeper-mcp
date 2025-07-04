using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ContextKeeper.Core;
using ContextKeeper.Core.Interfaces;
using ContextKeeper.Config;
using ContextKeeper.Protocol;
using Xunit;
using System.Text.Json.Nodes;

namespace ContextKeeper.Tests;

public class FunctionalityTest : TestBase, IDisposable
{
    private readonly IContextKeeperService _service;

    public FunctionalityTest() : base(useMockConfiguration: true)
    {
        _service = GetService<IContextKeeperService>();
        
        // Create isolated test environment
        var testDir = CreateIsolatedEnvironment(TestScenario.Mixed);
        SetCurrentDirectory(testDir);
    }

    [Fact]
    public async Task InitializeProject_ShouldReturnJsonObject()
    {
        // Act
        var result = await _service.InitializeProject();
        
        // Assert
        Assert.NotNull(result);
        Assert.IsType<JsonObject>(result);
        Assert.True(result.ContainsKey("success"));
    }

    [Fact]
    public async Task CheckCompactionNeeded_ShouldReturnJsonObject()
    {
        // Act
        var result = await _service.CheckCompactionNeeded();
        
        // Assert
        Assert.NotNull(result);
        Assert.IsType<JsonObject>(result);
        Assert.True(result.ContainsKey("snapshotCount") || result.ContainsKey("error"));
    }

    [Fact]
    public async Task SearchHistory_ShouldReturnJsonObject()
    {
        // Act
        var result = await _service.SearchHistory("test", 5);
        
        // Assert
        Assert.NotNull(result);
        Assert.IsType<JsonObject>(result);
        Assert.True(result.ContainsKey("searchTerm") || result.ContainsKey("error"));
    }

    [Fact]
    public async Task GetArchitecturalEvolution_ShouldReturnJsonObject()
    {
        // Act
        var result = await _service.GetArchitecturalEvolution("TestComponent");
        
        // Assert
        Assert.NotNull(result);
        Assert.IsType<JsonObject>(result);
        Assert.True(result.ContainsKey("componentName") || result.ContainsKey("error"));
    }

    [Fact]
    public async Task CompareSnapshots_ShouldReturnJsonObject()
    {
        // Act
        var result = await _service.CompareSnapshots("snapshot1.md", "snapshot2.md");
        
        // Assert
        Assert.NotNull(result);
        Assert.IsType<JsonObject>(result);
        Assert.True(result.ContainsKey("success") || result.ContainsKey("error"));
    }

    [Fact]
    public void JsonRpcServer_ShouldCreateSuccessfully()
    {
        // Act
        var server = new SimpleJsonRpcServer(_service);
        
        // Assert
        Assert.NotNull(server);
    }
    
    public override void Dispose()
    {
        base.Dispose();
    }
}