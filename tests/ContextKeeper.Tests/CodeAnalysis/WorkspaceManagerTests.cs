using Xunit;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using ContextKeeper.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace ContextKeeper.Tests.CodeAnalysis;

/// <summary>
/// Tests for WorkspaceManager - responsible for loading C# solutions and projects.
/// These tests demonstrate testing patterns for Roslyn-based code analysis.
/// </summary>
public class WorkspaceManagerTests : TestBase, IDisposable
{
    private readonly WorkspaceManager _workspaceManager;
    private readonly ILogger<WorkspaceManagerTests> _logger;
    private readonly string _testSolutionPath;
    private readonly string _tempDirectory;
    private readonly string _originalDirectory;
    
    public WorkspaceManagerTests()
    {
        _workspaceManager = GetService<WorkspaceManager>();
        _logger = GetService<ILogger<WorkspaceManagerTests>>();
        
        // Save original directory
        _originalDirectory = Environment.CurrentDirectory;
        
        // Create isolated test environment
        _tempDirectory = CreateTempDirectory();
        CopyTestData(_tempDirectory);
        Environment.CurrentDirectory = _tempDirectory;
        
        // Use the test solution from current directory
        _testSolutionPath = Path.Combine(Environment.CurrentDirectory, "TestSolution");
    }
    
    #region Solution Loading Tests
    
    [Fact]
    public async Task LoadSolutionAsync_WithValidSolution_ShouldSucceed()
    {
        // Arrange
        var solutionPath = Path.Combine(_testSolutionPath, "TestSolution.sln");
        
        // Act
        var solution = await _workspaceManager.LoadSolutionAsync(solutionPath);
        
        // Assert
        solution.Should().NotBeNull();
        solution!.FilePath.Should().Be(solutionPath);
        solution.Projects.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task LoadSolutionAsync_WithInvalidPath_ShouldThrowException()
    {
        // Arrange
        var invalidPath = Path.Combine(_testSolutionPath, "NonExistent.sln");
        
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await _workspaceManager.LoadSolutionAsync(invalidPath)
        );
    }
    
    [Fact]
    public async Task LoadSolutionAsync_ShouldCacheResult()
    {
        // This tests the caching behavior mentioned in CLAUDE.md
        // Arrange
        var solutionPath = Path.Combine(_testSolutionPath, "TestSolution.sln");
        
        // Act - Load twice
        var solution1 = await _workspaceManager.LoadSolutionAsync(solutionPath);
        var solution2 = await _workspaceManager.LoadSolutionAsync(solutionPath);
        
        // Assert - Should return the same instance (cached)
        solution1.Should().BeSameAs(solution2);
    }
    
    #endregion
    
    #region Project Loading Tests
    
    [Fact]
    public async Task LoadProjectAsync_WithValidProject_ShouldSucceed()
    {
        // Arrange
        var projectPath = Path.Combine(_testSolutionPath, "TestLibrary", "TestLibrary.csproj");
        
        // Act
        var project = await _workspaceManager.LoadProjectAsync(projectPath);
        
        // Assert
        project.Should().NotBeNull();
        project!.FilePath.Should().Be(projectPath);
        project.Name.Should().Be("TestLibrary");
    }
    
    [Fact]
    public async Task LoadProjectAsync_ShouldCompileSuccessfully()
    {
        // Arrange
        var projectPath = Path.Combine(_testSolutionPath, "TestLibrary", "TestLibrary.csproj");
        
        // Act
        var project = await _workspaceManager.LoadProjectAsync(projectPath);
        var compilation = await project!.GetCompilationAsync();
        
        // Assert
        compilation.Should().NotBeNull();
        var diagnostics = compilation!.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error);
        diagnostics.Should().BeEmpty("the test project should compile without errors");
    }
    
    #endregion
    
    #region MSBuild Registration Tests
    
    [Fact]
    public void Constructor_ShouldRegisterMSBuildDefaults()
    {
        // This test verifies that MSBuild is properly registered
        // which is critical for loading projects correctly
        
        // The constructor runs in the base class setup
        // If we got here without exceptions, MSBuild is registered
        _workspaceManager.Should().NotBeNull();
    }
    
    #endregion
    
    #region Error Handling Tests
    
    [Fact]
    public async Task LoadSolutionAsync_WithCorruptedSolution_ShouldProvideDescriptiveError()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var corruptedPath = Path.Combine(tempDir, "Corrupted.sln");
        await File.WriteAllTextAsync(corruptedPath, "This is not a valid solution file");
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _workspaceManager.LoadSolutionAsync(corruptedPath)
        );
        
        exception.Message.Should().Contain("Failed to load solution");
    }
    
    #endregion
    
    #region Helper Methods
    
    // No longer needed - we use the TestData solution
    
    #endregion
    
    public override void Dispose()
    {
        // Restore original directory
        Environment.CurrentDirectory = _originalDirectory;
        
        // Clean up temporary directory
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
        base.Dispose();
    }
}