using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ContextKeeper.Core;
using ContextKeeper.Core.Interfaces;
using ContextKeeper.Config;
using ContextKeeper.CodeAnalysis;

namespace ContextKeeper.Tests;

/// <summary>
/// Base class for tests that provides common test infrastructure.
/// This demonstrates the Dependency Injection pattern in testing.
/// </summary>
public abstract class TestBase : IDisposable
{
    protected readonly IHost Host;
    protected readonly IServiceProvider Services;
    protected readonly string TestDataPath;
    
    protected TestBase() : this(useMockConfiguration: false)
    {
    }
    
    /// <summary>
    /// Initializes the test base with optional mock configuration.
    /// </summary>
    /// <param name="useMockConfiguration">If true, uses mocked configuration service to prevent file I/O.</param>
    protected TestBase(bool useMockConfiguration)
    {
        // Set up test data path relative to test project
        // When running tests, we're already in the output directory
        TestDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
        
        // Create a host with all the necessary services
        // This mimics how the real application runs
        // Use new HostBuilder() instead of CreateDefaultBuilder() to avoid file system issues
        Host = new HostBuilder()
            .ConfigureServices((context, services) =>
            {
                // Configure logging for tests
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                });
                
                // Register configuration service (mocked or real)
                services.AddSingleton<ProfileDetector>(); // Always needed
                if (useMockConfiguration)
                {
                    var mockConfig = Helpers.MockConfigurationService.Create();
                    services.AddSingleton<IConfigurationService>(mockConfig.Object);
                }
                else
                {
                    services.AddSingleton<IConfigurationService, ConfigurationService>();
                }
                
                // Register all other services with their interfaces
                services.AddSingleton<ISnapshotManager, SnapshotManager>();
                services.AddSingleton<ISearchEngine, SearchEngine>();
                services.AddSingleton<IEvolutionTracker, EvolutionTracker>();
                services.AddSingleton<ICompactionEngine, CompactionEngine>();
                services.AddSingleton<IContextKeeperService, ContextKeeperService>();
                
                // Add C# Code Analysis services
                services.AddSingleton<WorkspaceManager>();
                services.AddSingleton<SymbolSearchService>();
                services.AddSingleton<CodeSearchTools>();
            })
            .Build();
            
        Services = Host.Services;
    }
    
    /// <summary>
    /// Gets a required service from the DI container.
    /// This is a helper method to reduce boilerplate in tests.
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        return Services.GetRequiredService<T>();
    }
    
    /// <summary>
    /// Creates a temporary directory for test isolation.
    /// Each test can have its own workspace.
    /// </summary>
    protected string CreateTempDirectory()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "ContextKeeperTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempPath);
        return tempPath;
    }
    
    /// <summary>
    /// Copies test data to a temporary directory for isolation.
    /// This prevents tests from interfering with each other.
    /// </summary>
    protected void CopyTestData(string destination)
    {
        CopyDirectory(TestDataPath, destination);
    }
    
    private void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        
        foreach (var file in Directory.GetFiles(source))
        {
            var destFile = Path.Combine(destination, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }
        
        foreach (var dir in Directory.GetDirectories(source))
        {
            var destDir = Path.Combine(destination, Path.GetFileName(dir));
            CopyDirectory(dir, destDir);
        }
    }
    
    public virtual void Dispose()
    {
        Host?.Dispose();
    }
}