using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace ContextKeeper.CodeAnalysis;

public class SymbolSearchService
{
    private readonly WorkspaceManager _workspaceManager;
    private readonly ILogger<SymbolSearchService> _logger;

    public SymbolSearchService(WorkspaceManager workspaceManager, ILogger<SymbolSearchService> logger)
    {
        _workspaceManager = workspaceManager;
        _logger = logger;
    }

    public async Task<Solution?> GetSolutionAsync(string solutionPath)
    {
        return await _workspaceManager.LoadSolutionAsync(solutionPath);
    }

    public async Task<Project?> GetProjectAsync(string projectPath)
    {
        return await _workspaceManager.LoadProjectAsync(projectPath);
    }

    public async Task<IEnumerable<ISymbol>> FindSymbolsAsync(
        Solution solution,
        string symbolName,
        bool ignoreCase = true,
        SymbolFilter filter = SymbolFilter.All,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Finding symbols '{Name}' with filter {Filter} in {ProjectCount} projects", 
                symbolName, filter, solution.Projects.Count());
            
            // FindDeclarationsAsync in newer Roslyn versions
            var allSymbols = new List<ISymbol>();
            
            foreach (var project in solution.Projects)
            {
                var compilation = await project.GetCompilationAsync(cancellationToken);
                if (compilation == null)
                {
                    _logger.LogWarning("Could not get compilation for project {Project}", project.Name);
                    continue;
                }
                
                var symbols = await SymbolFinder.FindDeclarationsAsync(
                    project, symbolName, ignoreCase, filter, cancellationToken);
                _logger.LogDebug("Found {Count} symbols in project {Project}", symbols.Count(), project.Name);
                allSymbols.AddRange(symbols);
            }
            
            _logger.LogDebug("Total symbols found: {Count}", allSymbols.Count);
            return allSymbols;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding symbols: {Name}", symbolName);
            return Enumerable.Empty<ISymbol>();
        }
    }

    public async Task<IEnumerable<ReferencedSymbol>> FindReferencesAsync(
        ISymbol symbol,
        Solution solution,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var references = await SymbolFinder.FindReferencesAsync(
                symbol, solution, cancellationToken);
            return references;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding references for symbol: {Name}", symbol.Name);
            return Enumerable.Empty<ReferencedSymbol>();
        }
    }

    public async Task<IEnumerable<ISymbol>> FindSymbolsByPatternAsync(
        Solution solution,
        string pattern,
        SymbolFilter filter = SymbolFilter.All,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var symbols = await SymbolFinder.FindSourceDeclarationsWithPatternAsync(
                solution, pattern, filter, cancellationToken);
            return symbols;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding symbols by pattern: {Pattern}", pattern);
            return Enumerable.Empty<ISymbol>();
        }
    }

    public async Task<IEnumerable<INamedTypeSymbol>> FindDerivedClassesAsync(
        INamedTypeSymbol typeSymbol,
        Solution solution,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // FindDerivedClassesAsync requires an IImmutableSet<Project>
            var projects = solution.Projects.ToImmutableHashSet();
            var derivedClasses = await SymbolFinder.FindDerivedClassesAsync(
                typeSymbol, solution, projects, cancellationToken);
            return derivedClasses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding derived classes for: {Name}", typeSymbol.Name);
            return Enumerable.Empty<INamedTypeSymbol>();
        }
    }

    public async Task<IEnumerable<INamedTypeSymbol>> FindImplementationsAsync(
        INamedTypeSymbol interfaceSymbol,
        Solution solution,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // FindImplementationsAsync signature: (symbol, solution, transitive, projects, cancellationToken)
            var implementations = await SymbolFinder.FindImplementationsAsync(
                interfaceSymbol, solution, transitive: false, cancellationToken: cancellationToken);
            return implementations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding implementations for: {Name}", interfaceSymbol.Name);
            return Enumerable.Empty<INamedTypeSymbol>();
        }
    }

    public void ClearCache()
    {
        _workspaceManager.ClearCache();
    }
}