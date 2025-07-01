using Microsoft.Extensions.Logging;
using System.Text.Json;
using ContextKeeper.Config.Models;
using ContextKeeper.Json;

namespace ContextKeeper.Config;

public interface IConfigurationService
{
    Task<ContextKeeperConfig> GetConfigAsync();
    Task SaveConfigAsync(ContextKeeperConfig config);
    Task InitializeProjectAsync();
}

public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private ContextKeeperConfig? _cachedConfig;
    private readonly string _configPath;
    
    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger;
        _configPath = Path.Combine(Directory.GetCurrentDirectory(), "contextkeeper.config.json");
    }
    
    public async Task<ContextKeeperConfig> GetConfigAsync()
    {
        if (_cachedConfig != null)
        {
            return _cachedConfig;
        }
        
        // Check for demo mode
        var demoMode = Environment.GetEnvironmentVariable("CONTEXTKEEPER_DEMO_MODE") == "true";
        var historyPath = Environment.GetEnvironmentVariable("CONTEXTKEEPER_HISTORY_PATH");
        
        // Try to load from local config file
        if (File.Exists(_configPath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_configPath);
                _cachedConfig = JsonSerializer.Deserialize(json, ContextKeeperJsonContext.Default.ContextKeeperConfig);
                if (_cachedConfig != null)
                {
                    // Override paths if in demo mode
                    if (demoMode && !string.IsNullOrEmpty(historyPath))
                    {
                        _cachedConfig.Paths.History = historyPath;
                        _cachedConfig.Paths.Snapshots = Path.Combine(historyPath, "snapshots");
                        _cachedConfig.Paths.Archived = Path.Combine(historyPath, "archived");
                        _logger.LogInformation("Demo mode activated: using history path {Path}", historyPath);
                    }
                    
                    _logger.LogDebug("Loaded configuration from {Path}", _configPath);
                    return _cachedConfig;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load config from {Path}, using defaults", _configPath);
            }
        }
        
        // Return default configuration
        _cachedConfig = GetDefaultConfig();
        _logger.LogDebug("Using default configuration");
        return _cachedConfig;
    }
    
    public async Task SaveConfigAsync(ContextKeeperConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, ContextKeeperJsonContext.Default.ContextKeeperConfig);
            await File.WriteAllTextAsync(_configPath, json);
            _cachedConfig = config;
            _logger.LogInformation("Configuration saved to {Path}", _configPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration to {Path}", _configPath);
            throw;
        }
    }
    
    public async Task InitializeProjectAsync()
    {
        var config = await GetConfigAsync();
        
        // Create necessary directories
        Directory.CreateDirectory(config.Paths.History);
        Directory.CreateDirectory(config.Paths.Snapshots);
        Directory.CreateDirectory(config.Paths.Archived);
        
        // Save config if it doesn't exist
        if (!File.Exists(_configPath))
        {
            await SaveConfigAsync(config);
        }
        
        _logger.LogInformation("Project initialized with ContextKeeper");
    }
    
    private ContextKeeperConfig GetDefaultConfig()
    {
        // Check for demo mode
        var demoMode = Environment.GetEnvironmentVariable("CONTEXTKEEPER_DEMO_MODE") == "true";
        var historyPath = Environment.GetEnvironmentVariable("CONTEXTKEEPER_HISTORY_PATH") ?? ".contextkeeper";
        
        if (demoMode)
        {
            _logger.LogInformation("Creating default config in demo mode with path: {Path}", historyPath);
        }
        
        return new ContextKeeperConfig
        {
            Version = "2.0",
            Paths = new PathConfig
            {
                History = demoMode ? historyPath : ".contextkeeper",
                Snapshots = demoMode ? Path.Combine(historyPath, "snapshots") : ".contextkeeper/snapshots",
                Archived = demoMode ? Path.Combine(historyPath, "archived") : ".contextkeeper/archived"
            },
            Snapshot = new SnapshotConfig
            {
                DateFormat = "yyyy-MM-dd",
                FilenamePattern = "SNAPSHOT_{date}_{type}_{milestone}.md",
                AutoCapture = true,
                AutoCaptureIntervalMinutes = 30
            },
            Compaction = new CompactionConfig
            {
                Threshold = 20,
                MaxAgeInDays = 90,
                AutoCompact = true
            },
            ContextTracking = new ContextTrackingConfig
            {
                TrackOpenFiles = true,
                TrackGitState = true,
                TrackRecentCommands = true,
                DocumentationFiles = new List<string> { "*.md", "*.txt", "*.adoc" },
                IgnorePatterns = new List<string> { "node_modules", "bin", "obj", ".git" }
            }
        };
    }
}