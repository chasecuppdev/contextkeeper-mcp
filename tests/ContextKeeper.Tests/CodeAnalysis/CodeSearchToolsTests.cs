using Xunit;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using ContextKeeper.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;
using Moq;

namespace ContextKeeper.Tests.CodeAnalysis;

/// <summary>
/// Tests for CodeSearchTools - the MCP tool integration for C# code search.
/// These tests demonstrate testing JSON serialization and MCP tool contracts.
/// </summary>
public class CodeSearchToolsTests : TestBase, IAsyncLifetime
{
    private readonly CodeSearchTools _codeSearchTools;
    private readonly WorkspaceManager _workspaceManager;
    private readonly ILogger<CodeSearchToolsTests> _logger;
    private Solution _testSolution = null!;
    private string _testSolutionPath = null!;
    private string _tempDirectory = null!;
    private string _originalDirectory = null!;
    
    public CodeSearchToolsTests()
    {
        _codeSearchTools = GetService<CodeSearchTools>();
        _workspaceManager = GetService<WorkspaceManager>();
        _logger = GetService<ILogger<CodeSearchToolsTests>>();
    }
    
    public async Task InitializeAsync()
    {
        // Save original directory
        _originalDirectory = Environment.CurrentDirectory;
        
        // Create isolated test environment
        _tempDirectory = CreateTempDirectory();
        CopyTestData(_tempDirectory);
        Environment.CurrentDirectory = _tempDirectory;
        
        // Use the test solution from current directory
        _testSolutionPath = Path.Combine(Environment.CurrentDirectory, "TestSolution");
        var solution = await _workspaceManager.LoadSolutionAsync(
            Path.Combine(_testSolutionPath, "TestSolution.sln"));
        _testSolution = solution ?? throw new InvalidOperationException("Failed to load test solution");
    }
    
    public Task DisposeAsync()
    {
        // Restore original directory
        Environment.CurrentDirectory = _originalDirectory;
        
        // Clean up temporary directory
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
        return Task.CompletedTask;
    }
    
    #region FindSymbolDefinitions Tool Tests
    
    [Fact]
    public async Task FindSymbolDefinitions_WithValidInput_ShouldReturnDefinitions()
    {
        // Arrange
        var solutionPath = Path.Combine(_testSolutionPath, "TestSolution.sln");
        var symbolName = "User";
        var symbolKind = "Class";
        
        // Act
        var result = await _codeSearchTools.FindSymbolDefinitions(
            solutionPath, 
            symbolName, 
            true, 
            symbolKind);
        
        // Assert
        result.Should().NotBeNull();
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        if (jsonResult!["Success"]!.GetValue<bool>() == false)
        {
            var error = jsonResult["Error"]?.ToString() ?? "Unknown error";
            throw new Exception($"API returned error: {error}. Full response: {result}");
        }
        jsonResult!["Success"]!.GetValue<bool>().Should().BeTrue();
        jsonResult["Count"]!.GetValue<int>().Should().BeGreaterThan(0);
        
        // Verify the structure of returned data
        var results = jsonResult["Results"] as JsonArray;
        results.Should().NotBeNull();
        var firstResult = results![0] as JsonObject;
        firstResult!["Name"].Should().NotBeNull();
        firstResult["Kind"].Should().NotBeNull();
        firstResult["Locations"].Should().NotBeNull();
    }
    
    [Fact]
    public async Task FindSymbolDefinitions_WithInvalidSymbolKind_ShouldStillWork()
    {
        // Arrange
        var solutionPath = Path.Combine(_testSolutionPath, "TestSolution.sln");
        var symbolName = "Product";
        var invalidKind = "InvalidKind"; // Invalid SymbolKind - should be ignored
        
        // Act
        var result = await _codeSearchTools.FindSymbolDefinitions(
            solutionPath,
            symbolName,
            true,
            invalidKind);
        
        // Assert
        result.Should().NotBeNull();
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        // Should still succeed but return all symbol kinds
        jsonResult!["Success"]!.GetValue<bool>().Should().BeTrue();
    }
    
    [Fact]
    public async Task FindSymbolDefinitions_WithNonExistentFile_ShouldReturnError()
    {
        // Arrange
        var invalidPath = Path.Combine(_testSolutionPath, "NonExistent.csproj");
        
        // Act
        var result = await _codeSearchTools.FindSymbolDefinitions(
            invalidPath,
            "User",
            true);
        
        // Assert
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        jsonResult!["Success"]!.GetValue<bool>().Should().BeFalse();
        jsonResult["Error"].Should().NotBeNull();
    }
    
    #endregion
    
    #region FindSymbolReferences Tool Tests
    
    [Fact]
    public async Task FindSymbolReferences_ForMethod_ShouldFindAllCalls()
    {
        // Arrange
        var solutionPath = Path.Combine(_testSolutionPath, "TestSolution.sln");
        var symbolName = "GetByIdAsync";
        var containingType = "Repository";
        
        // Act
        var result = await _codeSearchTools.FindSymbolReferences(
            solutionPath,
            symbolName,
            containingType);
        
        // Assert
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        jsonResult!["Success"]!.GetValue<bool>().Should().BeTrue();
        jsonResult["References"].Should().NotBeNull();
        
        var references = jsonResult["References"] as JsonArray;
        references!.Count.Should().BeGreaterThan(0);
        
        // Verify reference structure
        var firstRef = references[0] as JsonObject;
        firstRef!["Location"].Should().NotBeNull();
        var location = firstRef["Location"] as JsonObject;
        location!["FilePath"].Should().NotBeNull();
        location["Line"].Should().NotBeNull();
    }
    
    #endregion
    
    #region NavigateInheritanceHierarchy Tool Tests
    
    [Fact]
    public async Task NavigateInheritanceHierarchy_ForInterface_ShouldFindImplementations()
    {
        // Arrange
        var solutionPath = Path.Combine(_testSolutionPath, "TestSolution.sln");
        var typeName = "IRepository`1";
        
        // Act
        var result = await _codeSearchTools.NavigateInheritanceHierarchy(
            solutionPath,
            typeName,
            includeBaseTypes: false,
            includeDerivedTypes: true,
            includeInterfaces: true);
        
        // Assert
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        jsonResult!["Success"]!.GetValue<bool>().Should().BeTrue();
        jsonResult["Type"].Should().NotBeNull();
        jsonResult["DerivedTypes"].Should().NotBeNull();
        
        var derivedTypes = jsonResult["DerivedTypes"] as JsonArray;
        derivedTypes!.Count.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public async Task NavigateInheritanceHierarchy_ForClass_ShouldFindBaseTypes()
    {
        // Arrange
        var solutionPath = Path.Combine(_testSolutionPath, "TestSolution.sln");
        var typeName = "UserController";
        
        // Act
        var result = await _codeSearchTools.NavigateInheritanceHierarchy(
            solutionPath,
            typeName,
            includeBaseTypes: true,
            includeDerivedTypes: false,
            includeInterfaces: false);
        
        // Assert
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        jsonResult!["Success"]!.GetValue<bool>().Should().BeTrue();
        jsonResult["BaseTypes"].Should().NotBeNull();
    }
    
    #endregion
    
    #region SearchSymbolsByPattern Tool Tests
    
    [Fact]
    public async Task SearchSymbolsByPattern_WithWildcard_ShouldMatchPattern()
    {
        // Arrange
        var solutionPath = Path.Combine(_testSolutionPath, "TestSolution.sln");
        var pattern = "User*";
        var symbolKinds = "Class";
        
        // Act
        var result = await _codeSearchTools.SearchSymbolsByPattern(
            solutionPath,
            pattern,
            symbolKinds);
        
        // Assert
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        jsonResult!["Success"]!.GetValue<bool>().Should().BeTrue();
        var results = jsonResult["Results"] as JsonArray;
        results!.Count.Should().BeGreaterThan(0);
        
        // All results should start with "Calc"
        foreach (var node in results)
        {
            var symbol = node as JsonObject;
            symbol!["Name"]!.ToString().Should().StartWith("User");
        }
    }
    
    #endregion
    
    #region GetSymbolDocumentation Tool Tests
    
    [Fact]
    public async Task GetSymbolDocumentation_ForDocumentedSymbol_ShouldReturnXmlDocs()
    {
        // Arrange
        var solutionPath = Path.Combine(_testSolutionPath, "TestSolution.sln");
        var symbolName = "User";
        
        // Act
        var result = await _codeSearchTools.GetSymbolDocumentation(
            solutionPath,
            symbolName,
            includeInherited: true);
        
        // Assert
        var jsonResult = JsonSerializer.Deserialize<JsonObject>(result);
        jsonResult!["Success"]!.GetValue<bool>().Should().BeTrue();
        jsonResult["Symbol"].Should().NotBeNull();
        jsonResult["Documentation"].Should().NotBeNull();
        
        var docs = jsonResult["Documentation"] as JsonObject;
        docs!["Summary"].Should().NotBeNull();
    }
    
    #endregion
    
    #region AOT Compatibility Tests
    
    [Fact]
    public void JsonSerialization_ShouldBeAotCompatible()
    {
        // This test ensures our JSON serialization works with Native AOT
        // Arrange
        var testObject = new JsonObject
        {
            ["test"] = "value",
            ["number"] = 42,
            ["array"] = new JsonArray { "item1", "item2" }
        };
        
        // Act & Assert - Should not throw
        var serialized = JsonSerializer.Serialize(testObject);
        var deserialized = JsonSerializer.Deserialize<JsonObject>(serialized);
        
        deserialized.Should().NotBeNull();
        deserialized!["test"]!.ToString().Should().Be("value");
    }
    
    #endregion
    
    #region Integration Tests
    
    [Fact]
    public async Task FullWorkflow_FindDefinitionThenReferences_ShouldWork()
    {
        // This tests a common workflow: find a symbol, then find its references
        
        // Step 1: Find symbol definition
        var findDefInput = new JsonObject
        {
            ["solutionPath"] = Path.Combine(_testSolutionPath, "TestSolution.sln"),
            ["symbolName"] = "GetByIdAsync",
            ["symbolKind"] = "Method"
        };
        
        var definitions = await _codeSearchTools.FindSymbolDefinitions(
            Path.Combine(_testSolutionPath, "TestSolution.sln"),
            "GetByIdAsync",
            true,
            "Method");
        
        var defResult = JsonSerializer.Deserialize<JsonObject>(definitions);
        defResult!["Success"]!.GetValue<bool>().Should().BeTrue();
        defResult["Count"]!.GetValue<int>().Should().BeGreaterThan(0);
        
        // Step 2: Find references to that symbol
        var findRefInput = new JsonObject
        {
            ["solutionPath"] = Path.Combine(_testSolutionPath, "TestSolution.sln"),
            ["symbolName"] = "GetByIdAsync",
            ["symbolKind"] = "Method",
            ["containerName"] = "Repository"
        };
        
        var references = await _codeSearchTools.FindSymbolReferences(
            Path.Combine(_testSolutionPath, "TestSolution.sln"),
            "GetByIdAsync",
            "Repository");
        
        var refResult = JsonSerializer.Deserialize<JsonObject>(references);
        refResult!["Success"]!.GetValue<bool>().Should().BeTrue();
    }
    
    #endregion
}