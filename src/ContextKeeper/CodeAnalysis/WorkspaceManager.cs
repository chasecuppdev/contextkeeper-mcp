using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ContextKeeper.CodeAnalysis;

public class WorkspaceManager
{
    private readonly MSBuildWorkspace _workspace;
    private readonly ConcurrentDictionary<string, Solution> _solutionCache = new();
    private readonly ILogger<WorkspaceManager> _logger;
    private static bool _msbuildRegistered = false;
    private static readonly object _registrationLock = new();

    public WorkspaceManager(ILogger<WorkspaceManager> logger)
    {
        _logger = logger;
        
        // Ensure MSBuild is registered only once
        lock (_registrationLock)
        {
            if (!_msbuildRegistered)
            {
                try
                {
                    MSBuildLocator.RegisterDefaults();
                    _msbuildRegistered = true;
                    _logger.LogInformation("MSBuild registered successfully");
                }
                catch (InvalidOperationException)
                {
                    // Already registered
                    _msbuildRegistered = true;
                }
            }
        }
        
        _workspace = MSBuildWorkspace.Create(new Dictionary<string, string>
        {
            { "CheckForSystemRuntimeDependency", "true" },
            { "DesignTimeBuild", "true" }
        });
        
        _workspace.WorkspaceFailed += (sender, e) =>
        {
            _logger.LogWarning("Workspace warning: {Message}", e.Diagnostic.Message);
        };
    }

    public async Task<Solution?> LoadSolutionAsync(string solutionPath)
    {
        try
        {
            return await _solutionCache.GetOrAddAsync(solutionPath, async path =>
            {
                _logger.LogInformation("Loading solution: {Path}", path);
                
                if (!File.Exists(path))
                {
                    _logger.LogError("Solution file not found: {Path}", path);
                    throw new FileNotFoundException($"Solution file not found: {path}");
                }
                
                return await _workspace.OpenSolutionAsync(path);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading solution: {Path}", solutionPath);
            return null;
        }
    }

    public async Task<Project?> LoadProjectAsync(string projectPath)
    {
        try
        {
            _logger.LogInformation("Loading project: {Path}", projectPath);
            
            if (!File.Exists(projectPath))
            {
                _logger.LogError("Project file not found: {Path}", projectPath);
                return null;
            }
            
            return await _workspace.OpenProjectAsync(projectPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading project: {Path}", projectPath);
            return null;
        }
    }

    public void ClearCache()
    {
        _solutionCache.Clear();
        _logger.LogInformation("Solution cache cleared");
    }
}

// Extension method for concurrent dictionary
public static class ConcurrentDictionaryExtensions
{
    public static async Task<TValue> GetOrAddAsync<TKey, TValue>(
        this ConcurrentDictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TKey, Task<TValue>> valueFactory) where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var existingValue))
            return existingValue;

        var newValue = await valueFactory(key);
        return dictionary.GetOrAdd(key, newValue);
    }
}