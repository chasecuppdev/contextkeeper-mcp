using Microsoft.Extensions.Logging;
using ContextKeeper.Config.Models;
using ContextKeeper.Core.Interfaces;

namespace ContextKeeper.Core;

public class SearchEngine : ISearchEngine
{
    private readonly ILogger<SearchEngine> _logger;
    
    public SearchEngine(ILogger<SearchEngine> logger)
    {
        _logger = logger;
    }
    
    public async Task<SearchResult> SearchAsync(string searchTerm, int maxResults, ContextKeeperConfig config)
    {
        var results = new SearchResult
        {
            SearchTerm = searchTerm,
            Matches = new List<SearchMatch>()
        };
        
        // Search in both snapshots and compacted directories
        var snapshotsDir = Path.Combine(Directory.GetCurrentDirectory(), config.Paths.Snapshots);
        var compactedDir = Path.Combine(Directory.GetCurrentDirectory(), config.Paths.Archived);
        
        var searchDirs = new List<string>();
        if (Directory.Exists(snapshotsDir))
            searchDirs.Add(snapshotsDir);
        if (Directory.Exists(compactedDir))
            searchDirs.Add(compactedDir);
            
        if (searchDirs.Count == 0)
        {
            _logger.LogWarning("No search directories found");
            return results;
        }
        
        // Get all files from both directories
        var files = searchDirs
            .SelectMany(dir => Directory.GetFiles(dir, "*.md"))
            .OrderByDescending(f => f)
            .ToList();
        
        foreach (var file in files)
        {
            if (results.Matches.Count >= maxResults)
                break;
                
            var content = await File.ReadAllTextAsync(file);
            var lines = content.Split('\n');
            
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    results.Matches.Add(new SearchMatch
                    {
                        FileName = Path.GetFileName(file),
                        LineNumber = i + 1,
                        Context = GetContextLines(lines, i, 2),
                        MatchedLine = lines[i].Trim()
                    });
                    
                    if (results.Matches.Count >= maxResults)
                        break;
                }
            }
        }
        
        results.TotalMatches = results.Matches.Count;
        return results;
    }
    
    public Task<List<string>> SearchFilesAsync(string pattern, ContextKeeperConfig config)
    {
        var matches = new List<string>();
        var snapshotsDir = Path.Combine(Directory.GetCurrentDirectory(), config.Paths.Snapshots);
        
        if (!Directory.Exists(snapshotsDir))
        {
            _logger.LogWarning("Snapshots directory not found: {Directory}", snapshotsDir);
            return Task.FromResult(matches);
        }
        
        var files = Directory.GetFiles(snapshotsDir, pattern);
        foreach (var file in files)
        {
            matches.Add(Path.GetFileName(file));
        }
        
        return Task.FromResult(matches);
    }
    
    private string GetContextLines(string[] lines, int index, int contextSize)
    {
        // Check for demo mode context size override
        var demoContextSize = Environment.GetEnvironmentVariable("CONTEXTKEEPER_DEMO_CONTEXT_SIZE");
        if (!string.IsNullOrEmpty(demoContextSize) && int.TryParse(demoContextSize, out var demoSize))
        {
            contextSize = Math.Min(contextSize, demoSize / 20); // Divide by 20 to get line count from char count
        }
        
        var start = Math.Max(0, index - contextSize);
        var end = Math.Min(lines.Length - 1, index + contextSize);
        
        var contextLines = new List<string>();
        for (int i = start; i <= end; i++)
        {
            var prefix = i == index ? ">>> " : "    ";
            var line = lines[i];
            
            // Truncate long lines in demo mode
            if (!string.IsNullOrEmpty(demoContextSize) && line.Length > 100)
            {
                line = line.Substring(0, 97) + "...";
            }
            
            contextLines.Add($"{prefix}{line}");
        }
        
        return string.Join("\n", contextLines);
    }
}

public class SearchResult
{
    public string SearchTerm { get; set; } = "";
    public int TotalMatches { get; set; }
    public List<SearchMatch> Matches { get; set; } = new();
}

public class SearchMatch
{
    public string FileName { get; set; } = "";
    public int LineNumber { get; set; }
    public string Context { get; set; } = "";
    public string MatchedLine { get; set; } = "";
}