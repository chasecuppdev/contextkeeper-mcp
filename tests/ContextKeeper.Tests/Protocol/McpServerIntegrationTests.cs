using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using ContextKeeper.Protocol;
using ContextKeeper.CodeAnalysis;

namespace ContextKeeper.Tests.Protocol;

/// <summary>
/// Integration tests for MCP Server setup and configuration.
/// These tests verify that the MCP server is correctly configured with all tools.
/// </summary>
public class McpServerIntegrationTests : TestBase
{
    [Fact]
    public void McpServer_ShouldRegisterAllTools()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging();
        
        // Register all ContextKeeper services (from TestBase)
        services.AddSingleton<ContextKeeper.Config.IConfigurationService, ContextKeeper.Config.ConfigurationService>();
        services.AddSingleton<ContextKeeper.Core.Interfaces.ISnapshotManager, ContextKeeper.Core.SnapshotManager>();
        services.AddSingleton<ContextKeeper.Core.Interfaces.ISearchEngine, ContextKeeper.Core.SearchEngine>();
        services.AddSingleton<ContextKeeper.Core.Interfaces.IEvolutionTracker, ContextKeeper.Core.EvolutionTracker>();
        services.AddSingleton<ContextKeeper.Core.Interfaces.ICompactionEngine, ContextKeeper.Core.CompactionEngine>();
        services.AddSingleton<ContextKeeper.Core.IContextCaptureService, ContextKeeper.Core.ContextCaptureService>();
        services.AddSingleton<ContextKeeper.Utils.GitHelper>();
        services.AddSingleton<ContextKeeper.Core.Interfaces.IContextKeeperService, ContextKeeper.Core.ContextKeeperService>();
        services.AddSingleton<WorkspaceManager>();
        services.AddSingleton<SymbolSearchService>();
        services.AddSingleton<CodeSearchTools>();
        services.AddSingleton<ContextKeeperMcpTools>();
        
        // Configure MCP Server
        services
            .AddMcpServer()
            .WithTools<CodeSearchTools>()
            .WithTools<ContextKeeperMcpTools>();
        
        // Act
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert - Verify all tools are registered
        serviceProvider.GetService<CodeSearchTools>().Should().NotBeNull();
        serviceProvider.GetService<ContextKeeperMcpTools>().Should().NotBeNull();
    }
    
    [Fact]
    public void CodeSearchTools_ShouldHaveAllMcpToolMethods()
    {
        // This test uses reflection to verify all expected MCP tool methods exist
        var toolType = typeof(CodeSearchTools);
        var mcpToolAttribute = typeof(McpServerToolAttribute);
        
        // Get all methods with McpServerTool attribute
        var toolMethods = toolType.GetMethods()
            .Where(m => m.GetCustomAttributes(mcpToolAttribute, false).Any())
            .Select(m => m.Name)
            .ToList();
        
        // Assert expected tools exist
        toolMethods.Should().Contain("FindSymbolDefinitions");
        toolMethods.Should().Contain("FindSymbolReferences");
        toolMethods.Should().Contain("NavigateInheritanceHierarchy");
        toolMethods.Should().Contain("SearchSymbolsByPattern");
        toolMethods.Should().Contain("GetSymbolDocumentation");
        
        // Should have at least 5 tools
        toolMethods.Count.Should().BeGreaterThanOrEqualTo(5);
    }
    
    [Fact]
    public void ContextKeeperMcpTools_ShouldHaveAllMcpToolMethods()
    {
        // Verify ContextKeeper tools
        var toolType = typeof(ContextKeeperMcpTools);
        var mcpToolAttribute = typeof(McpServerToolAttribute);
        
        var toolMethods = toolType.GetMethods()
            .Where(m => m.GetCustomAttributes(mcpToolAttribute, false).Any())
            .Select(m => m.Name)
            .ToList();
        
        // Assert expected tools exist (new method names)
        toolMethods.Should().Contain("Snapshot");
        toolMethods.Should().Contain("GetStatus");
        toolMethods.Should().Contain("SearchEvolution");
        toolMethods.Should().Contain("TrackComponent");
        toolMethods.Should().Contain("CompareSnapshots");
        toolMethods.Should().Contain("GetTimeline");
        
        // Should have at least 6 tools
        toolMethods.Count.Should().BeGreaterThanOrEqualTo(6);
    }
    
    [Fact]
    public void AllMcpTools_ShouldHaveDescriptions()
    {
        // This test ensures all MCP tools have proper descriptions for discoverability
        var toolTypes = new[] { typeof(CodeSearchTools), typeof(ContextKeeperMcpTools) };
        var mcpToolAttribute = typeof(McpServerToolAttribute);
        var descriptionAttribute = typeof(System.ComponentModel.DescriptionAttribute);
        
        foreach (var toolType in toolTypes)
        {
            var toolMethods = toolType.GetMethods()
                .Where(m => m.GetCustomAttributes(mcpToolAttribute, false).Any());
            
            foreach (var method in toolMethods)
            {
                // Each tool method should have a description
                var hasDescription = method.GetCustomAttributes(descriptionAttribute, false).Any();
                hasDescription.Should().BeTrue($"Method {method.Name} should have a Description attribute");
                
                // Each parameter should also have descriptions
                var parameters = method.GetParameters()
                    .Where(p => p.ParameterType != typeof(CancellationToken));
                
                foreach (var param in parameters)
                {
                    var paramDescription = param.GetCustomAttributes(descriptionAttribute, false).Any();
                    paramDescription.Should().BeTrue(
                        $"Parameter {param.Name} in method {method.Name} should have a Description attribute");
                }
            }
        }
    }
    
    [Fact]
    public void McpTools_ShouldReturnJsonStrings()
    {
        // All MCP tool methods should return Task<string> (JSON responses)
        var toolTypes = new[] { typeof(CodeSearchTools), typeof(ContextKeeperMcpTools) };
        var mcpToolAttribute = typeof(McpServerToolAttribute);
        
        foreach (var toolType in toolTypes)
        {
            var toolMethods = toolType.GetMethods()
                .Where(m => m.GetCustomAttributes(mcpToolAttribute, false).Any());
            
            foreach (var method in toolMethods)
            {
                // Return type should be Task<string>
                method.ReturnType.Should().Be(typeof(Task<string>),
                    $"Method {method.Name} should return Task<string> for JSON responses");
            }
        }
    }
    
    [Fact]
    public void McpTools_ShouldSupportCancellation()
    {
        // All MCP tool methods should accept CancellationToken
        var toolTypes = new[] { typeof(CodeSearchTools), typeof(ContextKeeperMcpTools) };
        var mcpToolAttribute = typeof(McpServerToolAttribute);
        
        foreach (var toolType in toolTypes)
        {
            var toolMethods = toolType.GetMethods()
                .Where(m => m.GetCustomAttributes(mcpToolAttribute, false).Any());
            
            foreach (var method in toolMethods)
            {
                // Should have CancellationToken as last parameter
                var lastParam = method.GetParameters().LastOrDefault();
                lastParam.Should().NotBeNull();
                lastParam!.ParameterType.Should().Be(typeof(CancellationToken),
                    $"Method {method.Name} should have CancellationToken as last parameter");
                lastParam.HasDefaultValue.Should().BeTrue(
                    $"CancellationToken in {method.Name} should have default value");
            }
        }
    }
}