using System.Diagnostics;
using ContextKeeper.Core.Models;
using Microsoft.Extensions.Logging;

namespace ContextKeeper.Utils;

/// <summary>
/// Helper class for Git operations and context capture.
/// </summary>
public class GitHelper
{
    private readonly ILogger<GitHelper> _logger;
    
    public GitHelper(ILogger<GitHelper> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Captures the current Git context for a directory.
    /// </summary>
    public async Task<GitContext> CaptureGitContextAsync(string directory)
    {
        var context = new GitContext();
        
        if (!await IsGitRepositoryAsync(directory))
        {
            _logger.LogDebug("Directory {Directory} is not a git repository", directory);
            return context;
        }
        
        try
        {
            // Get current branch
            context.Branch = await RunGitCommandAsync(directory, "rev-parse --abbrev-ref HEAD");
            
            // Get current commit hash
            context.Commit = await RunGitCommandAsync(directory, "rev-parse HEAD");
            
            // Get commit message
            context.CommitMessage = await RunGitCommandAsync(directory, "log -1 --pretty=%B");
            
            // Get uncommitted files
            var status = await RunGitCommandAsync(directory, "status --porcelain");
            if (!string.IsNullOrWhiteSpace(status))
            {
                var lines = status.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.Length > 3)
                    {
                        var file = line.Substring(3).Trim();
                        if (line.StartsWith("A ") || line.StartsWith("M ") || line.StartsWith("D "))
                        {
                            context.StagedFiles.Add(file);
                        }
                        else
                        {
                            context.UncommittedFiles.Add(file);
                        }
                    }
                }
            }
            
            // Get recent commits
            var recentCommitsOutput = await RunGitCommandAsync(directory, 
                "log -5 --pretty=format:%H|%s|%an|%ai");
            if (!string.IsNullOrWhiteSpace(recentCommitsOutput))
            {
                var commits = recentCommitsOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var commit in commits)
                {
                    var parts = commit.Split('|');
                    if (parts.Length >= 4)
                    {
                        context.RecentCommits.Add(new CommitInfo
                        {
                            Hash = parts[0],
                            Message = parts[1],
                            Author = parts[2],
                            Date = DateTime.TryParse(parts[3], out var date) ? date : DateTime.MinValue
                        });
                    }
                }
            }
            
            // Get remotes
            var remotesOutput = await RunGitCommandAsync(directory, "remote -v");
            if (!string.IsNullOrWhiteSpace(remotesOutput))
            {
                var remotes = remotesOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var remote in remotes)
                {
                    var parts = remote.Split('\t');
                    if (parts.Length >= 2)
                    {
                        var name = parts[0];
                        var url = parts[1].Split(' ')[0]; // Remove (fetch) or (push)
                        context.Remotes[name] = url;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error capturing git context for {Directory}", directory);
        }
        
        return context;
    }
    
    /// <summary>
    /// Checks if a directory is a Git repository.
    /// </summary>
    public async Task<bool> IsGitRepositoryAsync(string directory)
    {
        try
        {
            var result = await RunGitCommandAsync(directory, "rev-parse --git-dir");
            return !string.IsNullOrWhiteSpace(result);
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Initializes Git hooks for auto-capture.
    /// </summary>
    public async Task InitializeGitHooksAsync(string directory)
    {
        if (!await IsGitRepositoryAsync(directory))
        {
            throw new InvalidOperationException($"Directory {directory} is not a git repository");
        }
        
        var gitDir = await RunGitCommandAsync(directory, "rev-parse --git-dir");
        gitDir = gitDir.Trim();
        
        var hooksDir = Path.Combine(directory, gitDir, "hooks");
        Directory.CreateDirectory(hooksDir);
        
        // Create pre-commit hook
        var preCommitHook = Path.Combine(hooksDir, "pre-commit");
        await File.WriteAllTextAsync(preCommitHook, @"#!/bin/sh
# ContextKeeper auto-capture hook
if command -v contextkeeper >/dev/null 2>&1; then
    contextkeeper capture --type pre-commit --auto
fi
");
        
        // Create post-checkout hook
        var postCheckoutHook = Path.Combine(hooksDir, "post-checkout");
        await File.WriteAllTextAsync(postCheckoutHook, @"#!/bin/sh
# ContextKeeper auto-capture hook
if command -v contextkeeper >/dev/null 2>&1; then
    contextkeeper capture --type checkout --auto
fi
");
        
        // Make hooks executable on Unix-like systems
        if (!OperatingSystem.IsWindows())
        {
            await RunCommandAsync("chmod", "+x " + preCommitHook, directory);
            await RunCommandAsync("chmod", "+x " + postCheckoutHook, directory);
        }
        
        _logger.LogInformation("Git hooks initialized in {Directory}", directory);
    }
    
    private async Task<string> RunGitCommandAsync(string workingDirectory, string arguments)
    {
        return await RunCommandAsync("git", arguments, workingDirectory);
    }
    
    private async Task<string> RunCommandAsync(string command, string arguments, string workingDirectory)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"Command '{command} {arguments}' failed: {error}");
        }
        
        return output.Trim();
    }
}