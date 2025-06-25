using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using ContextKeeper.Config;
using ContextKeeper.Config.Models;
using ContextKeeper.Core.Models;
using ContextKeeper.Utils;

namespace ContextKeeper.Core;

/// <summary>
/// Service responsible for capturing the complete development context.
/// </summary>
public interface IContextCaptureService
{
    Task<DevelopmentContext> CaptureContextAsync(string type = "manual", string milestone = "");
}

public class ContextCaptureService : IContextCaptureService
{
    private readonly ILogger<ContextCaptureService> _logger;
    private readonly IConfigurationService _configService;
    private readonly GitHelper _gitHelper;
    
    public ContextCaptureService(
        ILogger<ContextCaptureService> logger,
        IConfigurationService configService,
        GitHelper gitHelper)
    {
        _logger = logger;
        _configService = configService;
        _gitHelper = gitHelper;
    }
    
    public async Task<DevelopmentContext> CaptureContextAsync(string type = "manual", string milestone = "")
    {
        var context = new DevelopmentContext
        {
            Timestamp = DateTime.UtcNow,
            Type = type,
            Milestone = milestone,
            SnapshotId = GenerateSnapshotId(type, milestone)
        };
        
        var config = await _configService.GetConfigAsync();
        
        // Capture workspace context
        context.Workspace = await CaptureWorkspaceContextAsync(config);
        
        // Capture git context
        context.Git = await _gitHelper.CaptureGitContextAsync(Directory.GetCurrentDirectory());
        
        // Capture documentation
        context.Documentation = await CaptureDocumentationAsync(config);
        
        // Capture metadata
        context.Metadata = CaptureMetadata();
        
        _logger.LogInformation("Captured development context: {SnapshotId}", context.SnapshotId);
        return context;
    }
    
    private async Task<WorkspaceContext> CaptureWorkspaceContextAsync(ContextKeeperConfig config)
    {
        var workspace = new WorkspaceContext
        {
            WorkingDirectory = Directory.GetCurrentDirectory()
        };
        
        // For now, we'll capture basic workspace info
        // In a real implementation, this would integrate with IDE extensions
        // to capture open files, cursor positions, etc.
        
        // Capture recent commands from shell history if available
        await CaptureRecentCommandsAsync(workspace);
        
        return workspace;
    }
    
    private async Task CaptureRecentCommandsAsync(WorkspaceContext workspace)
    {
        try
        {
            // Try to read from common shell history files
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var historyFiles = new[]
            {
                Path.Combine(homeDir, ".bash_history"),
                Path.Combine(homeDir, ".zsh_history"),
                Path.Combine(homeDir, ".local", "share", "fish", "fish_history")
            };
            
            foreach (var historyFile in historyFiles)
            {
                if (File.Exists(historyFile))
                {
                    var lines = await File.ReadAllLinesAsync(historyFile);
                    var recentCommands = lines
                        .TakeLast(20)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => new CommandHistory
                        {
                            Command = line.Trim(),
                            Timestamp = DateTime.UtcNow, // Approximate
                            WorkingDirectory = workspace.WorkingDirectory
                        })
                        .ToList();
                    
                    workspace.RecentCommands = recentCommands;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not capture command history");
        }
    }
    
    private async Task<Dictionary<string, string>> CaptureDocumentationAsync(ContextKeeperConfig config)
    {
        var documentation = new Dictionary<string, string>();
        
        // Capture documentation from current directory
        foreach (var pattern in config.ContextTracking.DocumentationFiles)
        {
            var files = FileSystemHelpers.GetMatchingFiles(Directory.GetCurrentDirectory(), pattern);
            
            foreach (var file in files)
            {
                // Skip files in ignored directories
                if (config.ContextTracking.IgnorePatterns.Any(ignore => 
                    file.Contains(ignore, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
                
                try
                {
                    var content = await File.ReadAllTextAsync(file);
                    var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), file);
                    documentation[relativePath] = content;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not read documentation file: {File}", file);
                }
            }
        }
        
        // Also capture files from user workspace if it exists
        var workspacePath = Path.Combine(Directory.GetCurrentDirectory(), config.Paths.UserWorkspace);
        if (Directory.Exists(workspacePath))
        {
            _logger.LogDebug("Capturing documentation from user workspace: {Path}", workspacePath);
            
            foreach (var pattern in config.ContextTracking.DocumentationFiles)
            {
                var workspaceFiles = FileSystemHelpers.GetMatchingFiles(workspacePath, pattern);
                
                foreach (var file in workspaceFiles)
                {
                    try
                    {
                        var content = await File.ReadAllTextAsync(file);
                        var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), file);
                        documentation[relativePath] = content;
                        _logger.LogDebug("Captured workspace file: {File}", relativePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not read workspace file: {File}", file);
                    }
                }
            }
        }
        
        return documentation;
    }
    
    private ContextMetadata CaptureMetadata()
    {
        return new ContextMetadata
        {
            ProjectName = Path.GetFileName(Directory.GetCurrentDirectory()),
            ContextKeeperVersion = "2.0",
            OperatingSystem = RuntimeInformation.OSDescription,
            Machine = Environment.MachineName,
            User = Environment.UserName
        };
    }
    
    private string GenerateSnapshotId(string type, string milestone)
    {
        var date = DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss");
        var safeMilestone = string.IsNullOrEmpty(milestone) ? "snapshot" : milestone;
        return $"{date}_{type}_{safeMilestone}";
    }
}