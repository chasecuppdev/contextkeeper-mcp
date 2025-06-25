using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Nodes;
using ContextKeeper.Core;
using ContextKeeper.Core.Interfaces;
using ContextKeeper.Config;
using ContextKeeper.Protocol;
using ContextKeeper.Json;
using ContextKeeper.CodeAnalysis;
using ContextKeeper.Utils;
using ModelContextProtocol.Server;
using Microsoft.Build.Locator;

namespace ContextKeeper;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Register MSBuild first (required for Roslyn)
        try
        {
            MSBuildLocator.RegisterDefaults();
        }
        catch (InvalidOperationException)
        {
            // Already registered
        }
        
        // If no args, run as MCP server
        if (args.Length == 0)
        {
            return await RunMcpServer();
        }
        
        // Otherwise, run CLI
        var rootCommand = new RootCommand("ContextKeeper - AI-powered development context management");
        
        // Add subcommands
        rootCommand.AddCommand(CreateInitCommand());
        rootCommand.AddCommand(CreateSnapshotCommand());
        rootCommand.AddCommand(CreateCaptureCommand());
        rootCommand.AddCommand(CreateCheckCommand());
        rootCommand.AddCommand(CreateSearchCommand());
        rootCommand.AddCommand(CreateEvolutionCommand());
        rootCommand.AddCommand(CreateCompareCommand());
        rootCommand.AddCommand(CreateServerCommand());
        
        return await rootCommand.InvokeAsync(args);
    }
    
    static Command CreateInitCommand()
    {
        var command = new Command("init", "Initialize ContextKeeper in the current project");
        var gitHooksOption = new Option<bool>(
            "--git-hooks",
            getDefaultValue: () => true,
            "Install git hooks for automatic context capture"
        );
        command.AddOption(gitHooksOption);
        
        command.SetHandler(async (bool gitHooks) =>
        {
            var host = CreateHost();
            var configService = host.Services.GetRequiredService<IConfigurationService>();
            var gitHelper = host.Services.GetRequiredService<GitHelper>();
            
            try
            {
                // Initialize project configuration
                await configService.InitializeProjectAsync();
                Console.WriteLine("✓ ContextKeeper initialized");
                
                // Install git hooks if requested
                if (gitHooks && await gitHelper.IsGitRepositoryAsync(Directory.GetCurrentDirectory()))
                {
                    await gitHelper.InitializeGitHooksAsync(Directory.GetCurrentDirectory());
                    Console.WriteLine("✓ Git hooks installed");
                }
                
                Console.WriteLine("\nNext steps:");
                Console.WriteLine("- Run 'contextkeeper snapshot <milestone>' to create a manual snapshot");
                Console.WriteLine("- Git hooks will auto-capture on commits and branch switches");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Initialization failed: {ex.Message}");
                Environment.Exit(1);
            }
        }, gitHooksOption);
        
        return command;
    }
    
    static Command CreateSnapshotCommand()
    {
        var command = new Command("snapshot", "Create a new snapshot");
        var milestoneArg = new Argument<string>("milestone", "Milestone description (kebab-case)");
        command.AddArgument(milestoneArg);
        
        command.SetHandler(async (string milestone) =>
        {
            var host = CreateHost();
            var service = host.Services.GetRequiredService<IContextKeeperService>();
            var result = await service.CreateSnapshot(milestone);
            Console.WriteLine(result.ToJsonString());
        }, milestoneArg);
        
        return command;
    }
    
    static Command CreateCaptureCommand()
    {
        var command = new Command("capture", "Capture development context (used by git hooks)");
        
        var typeOption = new Option<string>(
            "--type",
            getDefaultValue: () => "manual",
            "Type of capture (manual, pre-commit, checkout)"
        );
        command.AddOption(typeOption);
        
        var autoOption = new Option<bool>(
            "--auto",
            "Running in automatic mode (quieter output)"
        );
        command.AddOption(autoOption);
        
        command.SetHandler(async (string type, bool auto) =>
        {
            var host = CreateHost();
            var service = host.Services.GetRequiredService<IContextKeeperService>();
            
            // Generate automatic milestone based on type
            string milestone = type switch
            {
                "pre-commit" => $"pre-commit-{DateTime.Now:yyyyMMdd-HHmmss}",
                "checkout" => $"checkout-{DateTime.Now:yyyyMMdd-HHmmss}",
                _ => $"manual-{DateTime.Now:yyyyMMdd-HHmmss}"
            };
            
            var result = await service.CreateSnapshot(milestone);
            
            if (!auto)
            {
                Console.WriteLine(result.ToJsonString());
            }
            else if (result["success"]?.GetValue<bool>() == true)
            {
                Console.WriteLine($"[ContextKeeper] Captured {type} context");
            }
        }, typeOption, autoOption);
        
        return command;
    }
    
    static Command CreateCheckCommand()
    {
        var command = new Command("check", "Check snapshot status and auto-compaction info");
        
        command.SetHandler(async () =>
        {
            var host = CreateHost();
            var service = host.Services.GetRequiredService<IContextKeeperService>();
            var result = await service.CheckCompactionNeeded();
            Console.WriteLine(result.ToJsonString());
        });
        
        return command;
    }
    
    static Command CreateSearchCommand()
    {
        var command = new Command("search", "Search through history");
        var termArg = new Argument<string>("term", "Search term");
        var maxOption = new Option<int>("--max", getDefaultValue: () => 5, "Maximum results");
        
        command.AddArgument(termArg);
        command.AddOption(maxOption);
        
        command.SetHandler(async (string term, int max) =>
        {
            var host = CreateHost();
            var service = host.Services.GetRequiredService<IContextKeeperService>();
            var result = await service.SearchHistory(term, max);
            Console.WriteLine(result.ToJsonString());
        }, termArg, maxOption);
        
        return command;
    }
    
    static Command CreateEvolutionCommand()
    {
        var command = new Command("evolution", "Track component evolution");
        var componentArg = new Argument<string>("component", "Component name");
        command.AddArgument(componentArg);
        
        command.SetHandler(async (string component) =>
        {
            var host = CreateHost();
            var service = host.Services.GetRequiredService<IContextKeeperService>();
            var result = await service.GetArchitecturalEvolution(component);
            Console.WriteLine(result.ToJsonString());
        }, componentArg);
        
        return command;
    }
    
    static Command CreateCompareCommand()
    {
        var command = new Command("compare", "Compare two snapshots");
        var snapshot1Arg = new Argument<string>("snapshot1", "First snapshot filename");
        var snapshot2Arg = new Argument<string>("snapshot2", "Second snapshot filename");
        
        command.AddArgument(snapshot1Arg);
        command.AddArgument(snapshot2Arg);
        
        command.SetHandler(async (string snapshot1, string snapshot2) =>
        {
            var host = CreateHost();
            var service = host.Services.GetRequiredService<IContextKeeperService>();
            var result = await service.CompareSnapshots(snapshot1, snapshot2);
            Console.WriteLine(result.ToJsonString());
        }, snapshot1Arg, snapshot2Arg);
        
        return command;
    }
    
    static Command CreateServerCommand()
    {
        var command = new Command("server", "Run as MCP server");
        
        command.SetHandler(async () =>
        {
            await RunMcpServer();
        });
        
        return command;
    }
    
    static async Task<int> RunMcpServer()
    {
        var builder = Host.CreateApplicationBuilder();
        
        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });
        
        // Register services
        RegisterServices(builder.Services);
        
        // Configure MCP Server
        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithTools<CodeSearchTools>()
            .WithTools<ContextKeeperMcpTools>();
        
        var host = builder.Build();
        
        Console.Error.WriteLine("ContextKeeper MCP Server started (stdio mode with C# code search)");
        
        await host.RunAsync();
        return 0;
    }
    
    static IHost CreateHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddConsole(options =>
                    {
                        options.LogToStandardErrorThreshold = LogLevel.Information;
                    });
                });
                
                // Register services
                RegisterServices(services);
            })
            .Build();
    }
    
    static void RegisterServices(IServiceCollection services)
    {
        // Core ContextKeeper services with interfaces
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<ISnapshotManager, SnapshotManager>();
        services.AddSingleton<ISearchEngine, SearchEngine>();
        services.AddSingleton<IEvolutionTracker, EvolutionTracker>();
        services.AddSingleton<ICompactionEngine, CompactionEngine>();
        services.AddSingleton<IContextKeeperService, ContextKeeperService>();
        
        // Code analysis services
        services.AddSingleton<WorkspaceManager>();
        services.AddSingleton<SymbolSearchService>();
        services.AddSingleton<CodeSearchTools>();
        
        // Context capture services
        services.AddSingleton<GitHelper>();
        services.AddSingleton<IContextCaptureService, ContextCaptureService>();
        
        // MCP Tools
        services.AddSingleton<ContextKeeperMcpTools>();
    }
}