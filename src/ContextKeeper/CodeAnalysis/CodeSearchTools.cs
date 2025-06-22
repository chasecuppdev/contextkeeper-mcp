using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using ContextKeeper.Json;

namespace ContextKeeper.CodeAnalysis;

[McpServerToolType]
public class CodeSearchTools
{
    private readonly SymbolSearchService _symbolService;
    private readonly ILogger<CodeSearchTools> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CodeSearchTools(SymbolSearchService symbolService, ILogger<CodeSearchTools> logger)
    {
        _symbolService = symbolService;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions 
        { 
            WriteIndented = true
        };
    }

    [McpServerTool]
    [Description("Find symbol definitions by name in a C# solution or project")]
    public async Task<string> FindSymbolDefinitions(
        [Description("Path to the .sln or .csproj file")] string solutionPath,
        [Description("Symbol name to search for")] string symbolName,
        [Description("Ignore case in search")] bool ignoreCase = true,
        [Description("Symbol type filter: Class, Method, Property, Field, Interface, Namespace, Any")] string? symbolKind = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Solution? solution = null;
            
            if (solutionPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
            {
                solution = await _symbolService.GetSolutionAsync(solutionPath);
            }
            else if (solutionPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                var project = await _symbolService.GetProjectAsync(solutionPath);
                solution = project?.Solution;
            }
            
            if (solution == null)
            {
                return SerializeToJson(new
                {
                    Success = false,
                    Error = "Failed to load solution or project"
                });
            }

            var symbols = await _symbolService.FindSymbolsAsync(
                solution, symbolName, ignoreCase, SymbolFilter.All, cancellationToken);

            // Additional filtering by specific kind if needed
            if (!string.IsNullOrEmpty(symbolKind) && Enum.TryParse<SymbolKind>(symbolKind, out var kind))
            {
                symbols = symbols.Where(s => s.Kind == kind);
            }

            var results = symbols.Select(symbol => new
            {
                Name = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                Kind = symbol.Kind.ToString(),
                ContainingNamespace = symbol.ContainingNamespace?.ToDisplayString() ?? "Global",
                ContainingType = symbol.ContainingType?.Name,
                Locations = symbol.Locations
                    .Where(loc => loc.IsInSource)
                    .Select(loc => new
                    {
                        FilePath = loc.SourceTree?.FilePath,
                        Line = loc.GetLineSpan().StartLinePosition.Line + 1,
                        Column = loc.GetLineSpan().StartLinePosition.Character + 1,
                        Preview = GetLinePreview(loc)
                    }).ToArray(),
                Documentation = GetXmlDocumentation(symbol)
            }).ToArray();

            return SerializeToJson(new
            {
                Success = true,
                Count = results.Length,
                Results = results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding symbol definitions");
            return SerializeToJson(new { Success = false, Error = ex.Message });
        }
    }

    [McpServerTool]
    [Description("Find all references to a symbol in the codebase")]
    public async Task<string> FindSymbolReferences(
        [Description("Path to the .sln or .csproj file")] string solutionPath,
        [Description("Symbol name to find references for")] string symbolName,
        [Description("Containing type name (optional)")] string? containingType = null,
        [Description("Include references in comments and strings")] bool includeStrings = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Solution? solution = null;
            
            if (solutionPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
            {
                solution = await _symbolService.GetSolutionAsync(solutionPath);
            }
            else if (solutionPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                var project = await _symbolService.GetProjectAsync(solutionPath);
                solution = project?.Solution;
            }
            
            if (solution == null)
            {
                return SerializeToJson(new
                {
                    Success = false,
                    Error = "Failed to load solution or project"
                });
            }

            var symbols = await _symbolService.FindSymbolsAsync(
                solution, symbolName, true, SymbolFilter.All, cancellationToken);

            // Filter to find the specific symbol
            var targetSymbol = symbols.FirstOrDefault(s =>
                (containingType == null || s.ContainingType?.Name == containingType));

            if (targetSymbol == null)
            {
                return SerializeToJson(new
                {
                    Success = false,
                    Error = "Symbol not found"
                });
            }

            var references = await _symbolService.FindReferencesAsync(
                targetSymbol, solution, cancellationToken);

            var results = references.SelectMany(refSymbol =>
                refSymbol.Locations.Select(location => new
                {
                    SymbolName = refSymbol.Definition.ToDisplayString(),
                    FilePath = location.Location.SourceTree?.FilePath,
                    Line = location.Location.GetLineSpan().StartLinePosition.Line + 1,
                    Column = location.Location.GetLineSpan().StartLinePosition.Character + 1,
                    Kind = location.IsImplicit ? "Implicit" : "Explicit",
                    Preview = GetLinePreview(location.Location)
                })).ToArray();

            return SerializeToJson(new
            {
                Success = true,
                Count = results.Length,
                Symbol = targetSymbol.ToDisplayString(),
                References = results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding symbol references");
            return SerializeToJson(new { Success = false, Error = ex.Message });
        }
    }

    [McpServerTool]
    [Description("Navigate the inheritance hierarchy of a type")]
    public async Task<string> NavigateInheritanceHierarchy(
        [Description("Path to the .sln or .csproj file")] string solutionPath,
        [Description("Type name to analyze")] string typeName,
        [Description("Include base types in the hierarchy")] bool includeBaseTypes = true,
        [Description("Include derived types")] bool includeDerivedTypes = true,
        [Description("Include implemented interfaces")] bool includeInterfaces = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Solution? solution = null;
            
            if (solutionPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
            {
                solution = await _symbolService.GetSolutionAsync(solutionPath);
            }
            else if (solutionPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                var project = await _symbolService.GetProjectAsync(solutionPath);
                solution = project?.Solution;
            }
            
            if (solution == null)
            {
                return SerializeToJson(new
                {
                    Success = false,
                    Error = "Failed to load solution or project"
                });
            }

            var typeSymbols = await _symbolService.FindSymbolsAsync(
                solution, typeName, true, SymbolFilter.All, cancellationToken);

            var typeSymbol = typeSymbols.OfType<INamedTypeSymbol>().FirstOrDefault();
            if (typeSymbol == null)
            {
                return SerializeToJson(new
                {
                    Success = false,
                    Error = "Type not found"
                });
            }

            var hierarchy = new
            {
                Success = true,
                Type = new
                {
                    Name = typeSymbol.ToDisplayString(),
                    Kind = typeSymbol.TypeKind.ToString(),
                    IsAbstract = typeSymbol.IsAbstract,
                    IsSealed = typeSymbol.IsSealed,
                    Location = GetPrimaryLocation(typeSymbol)
                },
                BaseTypes = includeBaseTypes ? GetBaseTypes(typeSymbol) : null,
                DerivedTypes = includeDerivedTypes ? await GetDerivedTypesAsync(typeSymbol, solution, cancellationToken) : null,
                Interfaces = includeInterfaces ? GetInterfaces(typeSymbol) : null
            };

            return SerializeToJson(hierarchy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing inheritance hierarchy");
            return SerializeToJson(new { Success = false, Error = ex.Message });
        }
    }

    [McpServerTool]
    [Description("Search for symbols by pattern with advanced filtering")]
    public async Task<string> SearchSymbolsByPattern(
        [Description("Path to the .sln or .csproj file")] string solutionPath,
        [Description("Search pattern (supports * and ? wildcards)")] string pattern,
        [Description("Symbol kinds to include (comma-separated): Class,Method,Property,Field,Interface")] string? symbolKinds = null,
        [Description("Namespace filter (partial match)")] string? namespaceFilter = null,
        [Description("Maximum results to return")] int maxResults = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Solution? solution = null;
            
            if (solutionPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
            {
                solution = await _symbolService.GetSolutionAsync(solutionPath);
            }
            else if (solutionPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                var project = await _symbolService.GetProjectAsync(solutionPath);
                solution = project?.Solution;
            }
            
            if (solution == null)
            {
                return SerializeToJson(new
                {
                    Success = false,
                    Error = "Failed to load solution or project"
                });
            }
            
            // Convert wildcards to regex pattern
            var regexPattern = pattern.Replace("*", ".*").Replace("?", ".");
            
            var symbols = await _symbolService.FindSymbolsByPatternAsync(
                solution, regexPattern, SymbolFilter.All, cancellationToken);

            // Apply filters
            var filtered = symbols.AsEnumerable();

            if (!string.IsNullOrEmpty(symbolKinds))
            {
                var kinds = symbolKinds.Split(',')
                    .Select(k => Enum.TryParse<SymbolKind>(k.Trim(), out var kind) ? kind : (SymbolKind?)null)
                    .Where(k => k.HasValue)
                    .Select(k => k!.Value)
                    .ToHashSet();
                
                filtered = filtered.Where(s => kinds.Contains(s.Kind));
            }

            if (!string.IsNullOrEmpty(namespaceFilter))
            {
                filtered = filtered.Where(s => 
                    s.ContainingNamespace?.ToDisplayString()?.Contains(namespaceFilter, StringComparison.OrdinalIgnoreCase) == true);
            }

            var results = filtered.Take(maxResults).Select(symbol => new
            {
                Name = symbol.Name,
                FullName = symbol.ToDisplayString(),
                Kind = symbol.Kind.ToString(),
                Namespace = symbol.ContainingNamespace?.ToDisplayString() ?? "Global",
                ContainingType = symbol.ContainingType?.Name,
                Location = GetPrimaryLocation(symbol),
                IsPublic = symbol.DeclaredAccessibility == Accessibility.Public,
                IsStatic = symbol.IsStatic
            }).ToArray();

            return SerializeToJson(new
            {
                Success = true,
                Pattern = pattern,
                Count = results.Length,
                TotalFound = filtered.Count(),
                Results = results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching symbols by pattern");
            return SerializeToJson(new { Success = false, Error = ex.Message });
        }
    }

    [McpServerTool]
    [Description("Get detailed documentation for a symbol")]
    public async Task<string> GetSymbolDocumentation(
        [Description("Path to the .sln or .csproj file")] string solutionPath,
        [Description("Symbol name")] string symbolName,
        [Description("Include inherited documentation")] bool includeInherited = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Solution? solution = null;
            
            if (solutionPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
            {
                solution = await _symbolService.GetSolutionAsync(solutionPath);
            }
            else if (solutionPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                var project = await _symbolService.GetProjectAsync(solutionPath);
                solution = project?.Solution;
            }
            
            if (solution == null)
            {
                return SerializeToJson(new
                {
                    Success = false,
                    Error = "Failed to load solution or project"
                });
            }

            var symbols = await _symbolService.FindSymbolsAsync(
                solution, symbolName, true, SymbolFilter.All, cancellationToken);

            var results = symbols.Select(symbol =>
            {
                var xmlDoc = symbol.GetDocumentationCommentXml();
                var parsed = ParseXmlDocumentation(xmlDoc);

                return new
                {
                    Symbol = symbol.ToDisplayString(),
                    Kind = symbol.Kind.ToString(),
                    Summary = parsed.Summary,
                    Remarks = parsed.Remarks,
                    Parameters = GetParameterDocumentation(symbol, parsed),
                    Returns = GetReturnDocumentation(symbol, parsed),
                    Exceptions = parsed.Exceptions,
                    Examples = parsed.Examples,
                    SeeAlso = parsed.SeeAlso,
                    InheritedFrom = includeInherited ? GetInheritedDocumentation(symbol) : null
                };
            }).ToArray();

            return SerializeToJson(new
            {
                Success = true,
                Count = results.Length,
                Documentation = results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting symbol documentation");
            return SerializeToJson(new { Success = false, Error = ex.Message });
        }
    }

    // Helper methods
    private static string? GetLinePreview(Location location)
    {
        if (location.SourceTree == null) return null;
        
        var line = location.GetLineSpan().StartLinePosition.Line;
        var text = location.SourceTree.GetText();
        
        if (line < text.Lines.Count)
        {
            return text.Lines[line].ToString().Trim();
        }
        
        return null;
    }

    private static object? GetPrimaryLocation(ISymbol symbol)
    {
        var location = symbol.Locations.FirstOrDefault(l => l.IsInSource);
        if (location == null) return null;

        return new
        {
            FilePath = location.SourceTree?.FilePath,
            Line = location.GetLineSpan().StartLinePosition.Line + 1,
            Column = location.GetLineSpan().StartLinePosition.Character + 1
        };
    }

    private static object[] GetBaseTypes(INamedTypeSymbol typeSymbol)
    {
        var baseTypes = new List<object>();
        var current = typeSymbol.BaseType;
        
        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            baseTypes.Add(new
            {
                Name = current.ToDisplayString(),
                Kind = current.TypeKind.ToString(),
                IsAbstract = current.IsAbstract,
                Location = GetPrimaryLocation(current)
            });
            current = current.BaseType;
        }
        
        return baseTypes.ToArray();
    }

    private async Task<object[]> GetDerivedTypesAsync(
        INamedTypeSymbol typeSymbol, 
        Solution solution, 
        CancellationToken cancellationToken)
    {
        IEnumerable<INamedTypeSymbol> derivedTypes;
        
        if (typeSymbol.TypeKind == TypeKind.Interface)
        {
            derivedTypes = await _symbolService.FindImplementationsAsync(
                typeSymbol, solution, cancellationToken);
        }
        else
        {
            derivedTypes = await _symbolService.FindDerivedClassesAsync(
                typeSymbol, solution, cancellationToken);
        }

        return derivedTypes.Select(t => new
        {
            Name = t.ToDisplayString(),
            Kind = t.TypeKind.ToString(),
            IsAbstract = t.IsAbstract,
            IsSealed = t.IsSealed,
            Location = GetPrimaryLocation(t)
        }).ToArray();
    }

    private static object[] GetInterfaces(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.AllInterfaces.Select(i => new
        {
            Name = i.ToDisplayString(),
            IsGeneric = i.IsGenericType,
            Location = GetPrimaryLocation(i)
        }).ToArray();
    }

    private static string? GetXmlDocumentation(ISymbol symbol)
    {
        var xml = symbol.GetDocumentationCommentXml();
        return string.IsNullOrWhiteSpace(xml) ? null : xml;
    }

    private static ParsedDocumentation ParseXmlDocumentation(string? xml)
    {
        var doc = new ParsedDocumentation();
        if (string.IsNullOrEmpty(xml)) return doc;

        // Extract summary
        var summaryMatch = System.Text.RegularExpressions.Regex.Match(
            xml, @"<summary>(.*?)</summary>", 
            System.Text.RegularExpressions.RegexOptions.Singleline);
        if (summaryMatch.Success)
        {
            doc.Summary = summaryMatch.Groups[1].Value.Trim();
        }

        // Extract remarks
        var remarksMatch = System.Text.RegularExpressions.Regex.Match(
            xml, @"<remarks>(.*?)</remarks>", 
            System.Text.RegularExpressions.RegexOptions.Singleline);
        if (remarksMatch.Success)
        {
            doc.Remarks = remarksMatch.Groups[1].Value.Trim();
        }

        return doc;
    }

    private static object[] GetParameterDocumentation(ISymbol symbol, ParsedDocumentation parsed)
    {
        if (symbol is IMethodSymbol method)
        {
            return method.Parameters.Select(p => new
            {
                Name = p.Name,
                Type = p.Type.ToDisplayString(),
                IsOptional = p.IsOptional,
                IsParams = p.IsParams,
                DefaultValue = p.HasExplicitDefaultValue ? p.ExplicitDefaultValue?.ToString() : null
            }).ToArray();
        }
        return Array.Empty<object>();
    }

    private static string? GetReturnDocumentation(ISymbol symbol, ParsedDocumentation parsed)
    {
        if (symbol is IMethodSymbol method && method.ReturnType.SpecialType != SpecialType.System_Void)
        {
            return method.ReturnType.ToDisplayString();
        }
        return null;
    }

    private static string? GetInheritedDocumentation(ISymbol symbol)
    {
        if (symbol is IMethodSymbol method && method.OverriddenMethod != null)
        {
            return method.OverriddenMethod.GetDocumentationCommentXml();
        }
        return null;
    }

    private class ParsedDocumentation
    {
        public string? Summary { get; set; }
        public string? Remarks { get; set; }
        public List<string> Exceptions { get; set; } = new();
        public List<string> Examples { get; set; } = new();
        public List<string> SeeAlso { get; set; } = new();
    }
    
    // Helper method to serialize results in an AOT-compatible way
    private string SerializeToJson(object value)
    {
        // Serialize to JsonNode first, then to string
        #pragma warning disable IL2026, IL3050 // Suppress AOT warnings for object serialization
        var jsonNode = JsonSerializer.SerializeToNode(value, _jsonOptions);
        #pragma warning restore IL2026, IL3050
        
        return jsonNode?.ToJsonString(_jsonOptions) ?? "{}";
    }
}