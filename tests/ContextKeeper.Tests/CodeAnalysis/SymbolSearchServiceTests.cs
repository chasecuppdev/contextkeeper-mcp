using Xunit;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using ContextKeeper.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ContextKeeper.Tests.CodeAnalysis;

/// <summary>
/// Tests for SymbolSearchService - responsible for searching symbols in C# code.
/// These tests demonstrate testing async operations and caching behavior.
/// </summary>
public class SymbolSearchServiceTests : TestBase, IAsyncLifetime
{
    private readonly SymbolSearchService _symbolSearchService;
    private readonly WorkspaceManager _workspaceManager;
    private readonly ILogger<SymbolSearchServiceTests> _logger;
    private Solution _testSolution = null!;
    private string _testSolutionPath = null!;
    private string _tempDirectory = null!;
    private string _originalDirectory = null!;
    
    public SymbolSearchServiceTests()
    {
        _symbolSearchService = GetService<SymbolSearchService>();
        _workspaceManager = GetService<WorkspaceManager>();
        _logger = GetService<ILogger<SymbolSearchServiceTests>>();
    }
    
    /// <summary>
    /// IAsyncLifetime allows async setup/teardown - perfect for loading test solutions
    /// </summary>
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
    
    #region FindSymbolsAsync Tests
    
    [Fact]
    public async Task FindSymbolsAsync_WithNameFilter_ShouldReturnMatchingSymbols()
    {
        // Arrange
        var compilation = await GetTestCompilation();
        
        // Act
        var symbols = await _symbolSearchService.FindSymbolsAsync(
            _testSolution, 
            "User");
        
        // Assert
        symbols.Should().NotBeEmpty();
        var userClass = symbols.FirstOrDefault(s => s.Name == "User" && s.Kind == SymbolKind.NamedType);
        userClass.Should().NotBeNull();
    }
    
    [Fact]
    public async Task FindSymbolsAsync_WithKindFilter_ShouldReturnOnlySpecificKind()
    {
        // Arrange
        var compilation = await GetTestCompilation();
        
        // Act
        var methods = await _symbolSearchService.FindSymbolsByPatternAsync(
            _testSolution, 
            "Get*",  // Search for methods starting with Get using wildcard
            filter: SymbolFilter.Member);
        
        // Assert
        methods.Should().NotBeEmpty();
        methods.Should().OnlyContain(s => s.Kind == SymbolKind.Method);
    }
    
    [Fact]
    public async Task FindSymbolsAsync_WithMultipleFilters_ShouldApplyAllFilters()
    {
        // Arrange
        var compilation = await GetTestCompilation();
        
        // Act
        var symbols = await _symbolSearchService.FindSymbolsAsync(
            _testSolution,
            "Service",
            ignoreCase: true,
            filter: SymbolFilter.Type);
        
        // Assert
        symbols.Should().NotBeEmpty();
        symbols.Should().OnlyContain(s => 
            s.Name.Contains("Service", StringComparison.OrdinalIgnoreCase) && 
            (s.Kind == SymbolKind.NamedType || s.Kind == SymbolKind.ErrorType));  // Interface is a NamedType in Roslyn
    }
    
    #endregion
    
    #region FindReferencesAsync Tests
    
    [Fact]
    public async Task FindReferencesAsync_ForClass_ShouldFindAllReferences()
    {
        // Arrange
        var compilation = await GetTestCompilation();
        var userClass = compilation.GetTypeByMetadataName("TestLibrary.Models.User");
        
        // Act
        var references = await _symbolSearchService.FindReferencesAsync(
            userClass!, 
            _testSolution);
        
        // Assert
        references.Should().NotBeEmpty();
        references.Should().Contain(r => r.Definition.Name == "User");
    }
    
    [Fact]
    public async Task FindReferencesAsync_ForInterface_ShouldFindImplementations()
    {
        // Arrange
        var compilation = await GetTestCompilation();
        var repository = compilation.GetTypeByMetadataName("TestLibrary.IRepository`1");
        
        // Act
        var references = await _symbolSearchService.FindReferencesAsync(
            repository!,
            _testSolution);
        
        // Assert
        references.Should().NotBeEmpty();
        // Should find references in classes that implement the interface
    }
    
    #endregion
    
    #region Pattern Search Tests
    
    [Fact]
    public async Task FindSymbolsByPatternAsync_WithWildcard_ShouldMatchPattern()
    {
        // Arrange
        var compilation = await GetTestCompilation();
        
        // Act
        var symbols = await _symbolSearchService.FindSymbolsByPatternAsync(
            _testSolution,
            "User*");
        
        // Assert
        symbols.Should().NotBeEmpty();
        // Should contain User class and UserController
        symbols.Should().Contain(s => s.Name == "User");
        symbols.Should().Contain(s => s.Name == "UserController");
    }
    
    [Fact]
    public async Task FindSymbolsByPatternAsync_WithRegex_ShouldMatchComplexPatterns()
    {
        // Arrange
        var compilation = await GetTestCompilation();
        
        // Act
        var symbols = await _symbolSearchService.FindSymbolsByPatternAsync(
            _testSolution,
            "*Controller", // Matches names ending with "Controller"
            SymbolFilter.Type); // Only search for types, not namespaces or members
        
        // Assert
        symbols.Should().NotBeEmpty();
        symbols.Should().OnlyContain(s => s.Name.EndsWith("Controller"));
        
        // Should find BaseController, UserController, ProductController
        symbols.Should().HaveCountGreaterOrEqualTo(3);
    }
    
    #endregion
    
    #region Inheritance Tests
    
    [Fact]
    public async Task FindDerivedClassesAsync_ForInterface_ShouldFindImplementations()
    {
        // Arrange
        var compilation = await GetTestCompilation();
        var baseInterface = compilation.GetTypeByMetadataName("TestLibrary.IService`1");
        
        // Act
        var derivedClasses = await _symbolSearchService.FindDerivedClassesAsync(
            baseInterface!,
            _testSolution);
        
        // Assert
        derivedClasses.Should().NotBeEmpty();
        derivedClasses.Should().Contain(c => c.Name == "Service");
    }
    
    [Fact]
    public async Task FindImplementationsAsync_ForInterface_ShouldFindImplementingClasses()
    {
        // Arrange
        var compilation = await GetTestCompilation();
        var interfaceType = compilation.GetTypeByMetadataName("TestLibrary.IRepository`1");
        
        // Act
        var implementations = await _symbolSearchService.FindImplementationsAsync(
            interfaceType!,
            _testSolution);
        
        // Assert
        implementations.Should().NotBeEmpty();
        implementations.Should().Contain(impl => impl.Name == "Repository");
    }
    
    #endregion
    
    #region Caching Tests
    
    [Fact]
    public async Task FindSymbolsAsync_WhenCalledMultipleTimes_ShouldUseCache()
    {
        // This test verifies caching behavior for performance
        // Arrange
        var compilation = await GetTestCompilation();
        
        // Act - Call twice with same parameters
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var firstCall = await _symbolSearchService.FindSymbolsAsync(_testSolution, "User");
        var firstCallTime = stopwatch.ElapsedMilliseconds;
        
        stopwatch.Restart();
        var secondCall = await _symbolSearchService.FindSymbolsAsync(_testSolution, "User");
        var secondCallTime = stopwatch.ElapsedMilliseconds;
        
        // Assert
        firstCall.Should().BeEquivalentTo(secondCall);
        // Second call should be significantly faster due to caching
        secondCallTime.Should().BeLessThan(firstCallTime / 2);
    }
    
    #endregion
    
    #region Helper Methods
    
    private async Task<Compilation> GetTestCompilation()
    {
        // Get the TestLibrary project specifically since most types are defined there
        var project = _testSolution.Projects.FirstOrDefault(p => p.Name == "TestLibrary") 
                      ?? _testSolution.Projects.First();
        var compilation = await project.GetCompilationAsync();
        
        if (compilation == null)
            throw new InvalidOperationException($"Failed to get compilation for project {project.Name}");
            
        return compilation;
    }
    #endregion
}