using Microsoft.Extensions.Logging;
using System.Text.Json;
using ContextKeeper.Config.Models;

namespace ContextKeeper.Config;

public interface IConfigurationService
{
    Task<WorkflowProfile> GetActiveProfileAsync();
    Task<WorkflowProfile?> GetProfileAsync(string profileName);
    Task<WorkflowProfile?> DetectProfileAsync();
    Task<ContextKeeperConfig> LoadConfigAsync();
}

public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly ProfileDetector _profileDetector;
    private ContextKeeperConfig? _cachedConfig;
    private readonly string _configPath;
    
    public ConfigurationService(ILogger<ConfigurationService> logger, ProfileDetector profileDetector)
    {
        _logger = logger;
        _profileDetector = profileDetector;
        _configPath = Path.Combine(Directory.GetCurrentDirectory(), "contextkeeper.config.json");
    }
    
    public async Task<WorkflowProfile> GetActiveProfileAsync()
    {
        var config = await LoadConfigAsync();
        
        // Check environment variable first
        var envProfile = Environment.GetEnvironmentVariable("CONTEXTKEEPER_PROFILE");
        if (!string.IsNullOrEmpty(envProfile) && config.Profiles.ContainsKey(envProfile))
        {
            return config.Profiles[envProfile];
        }
        
        // Use default profile from config
        if (config.Profiles.ContainsKey(config.DefaultProfile))
        {
            return config.Profiles[config.DefaultProfile];
        }
        
        // Try to auto-detect
        var detected = await DetectProfileAsync();
        if (detected != null)
        {
            return detected;
        }
        
        // Fall back to first available profile
        if (config.Profiles.Any())
        {
            return config.Profiles.First().Value;
        }
        
        // Return default CLAUDE.md profile if nothing else
        return GetDefaultClaudeProfile();
    }
    
    public async Task<WorkflowProfile?> GetProfileAsync(string profileName)
    {
        var config = await LoadConfigAsync();
        return config.Profiles.TryGetValue(profileName, out var profile) ? profile : null;
    }
    
    public async Task<WorkflowProfile?> DetectProfileAsync()
    {
        // Try to detect based on project structure
        var detectedProfile = await _profileDetector.DetectProfileAsync();
        if (detectedProfile != null)
        {
            return detectedProfile;
        }
        
        // Load built-in profiles and check
        var builtInProfiles = await LoadBuiltInProfilesAsync();
        foreach (var profile in builtInProfiles.Values)
        {
            if (await _profileDetector.MatchesProfileAsync(profile))
            {
                return profile;
            }
        }
        
        return null;
    }
    
    public async Task<ContextKeeperConfig> LoadConfigAsync()
    {
        if (_cachedConfig != null)
        {
            return _cachedConfig;
        }
        
        // Try to load from local config file
        if (File.Exists(_configPath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_configPath);
                _cachedConfig = JsonSerializer.Deserialize<ContextKeeperConfig>(json);
                if (_cachedConfig != null)
                {
                    return _cachedConfig;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load config from {Path}", _configPath);
            }
        }
        
        // Fall back to built-in profiles
        _cachedConfig = new ContextKeeperConfig
        {
            Version = "1.0",
            DefaultProfile = "claude-workflow",
            Profiles = await LoadBuiltInProfilesAsync()
        };
        
        return _cachedConfig;
    }
    
    private async Task<Dictionary<string, WorkflowProfile>> LoadBuiltInProfilesAsync()
    {
        var profiles = new Dictionary<string, WorkflowProfile>();
        
        // Add default CLAUDE.md profile
        profiles["claude-workflow"] = GetDefaultClaudeProfile();
        
        // Load from profiles directory if available
        var profilesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles");
        if (Directory.Exists(profilesDir))
        {
            var profileFiles = Directory.GetFiles(profilesDir, "*.json");
            foreach (var file in profileFiles)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var profile = JsonSerializer.Deserialize<WorkflowProfile>(json);
                    if (profile != null && !string.IsNullOrEmpty(profile.Name))
                    {
                        profiles[profile.Name] = profile;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load profile from {File}", file);
                }
            }
        }
        
        return profiles;
    }
    
    private WorkflowProfile GetDefaultClaudeProfile()
    {
        return new WorkflowProfile
        {
            Name = "claude-workflow",
            Description = "LSM-tree inspired development history for CLAUDE.md projects",
            Detection = new DetectionConfig
            {
                Files = new List<string> { "CLAUDE.md" },
                Paths = new List<string> { "FeatureData/DataHistory" }
            },
            Paths = new PathConfig
            {
                History = "FeatureData/DataHistory",
                Snapshots = "FeatureData/DataHistory/CLAUDE",
                Compacted = "FeatureData/DataHistory/Compacted"
            },
            Snapshot = new SnapshotConfig
            {
                Prefix = "CLAUDE_",
                DateFormat = "yyyy-MM-dd",
                FilenamePattern = "{prefix}{date}_{milestone}.md",
                Validation = @"^[a-z0-9-]+$",
                MaxLength = 50
            },
            Compaction = new CompactionConfig
            {
                Threshold = 10,
                Strategy = "lsm-quarterly",
                ArchivePath = "Archived_{quarter}"
            },
            Header = new HeaderConfig
            {
                Template = @"# {document} Historical Snapshot
**Date**: {date}
**Milestone**: {milestone}
**Previous State**: {previous}
**Compaction Status**: {status}

## Changes in This Version
- [To be filled by developer]

## Context for Future Reference
- [To be filled by developer]

---
{content}"
            }
        };
    }
}