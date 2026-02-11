using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusMods.CLI.Types;
using NexusMods.Sdk.ProxyConsole;

namespace NexusMods.SingleProcess;

/// <summary>
/// A configurator for the commandline parser. It looks for all verb definitions (created by AddVerb)
/// and adds them to the parser. It also adds all injected types to the binding context. The RunAsync method
/// can be used to run the parser and execute the matching verb.
/// </summary>
public class CommandLineConfigurator
{
    private readonly RootCommand _rootCommand;
    private readonly IServiceProvider _provider;
    private readonly MethodInfo _makeOptionMethod;

    private ILogger _logger;

    /// <summary>
    /// Current renderer, set before each invocation in RunAsync.
    /// </summary>
    internal IRenderer? CurrentRenderer { get; private set; }

    /// <summary>
    /// DI constructor
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="verbDefinitions"></param>
    /// <param name="moduleDefinitions"></param>
    public CommandLineConfigurator(IServiceProvider provider, IEnumerable<VerbDefinition> verbDefinitions, IEnumerable<ModuleDefinition> moduleDefinitions)
    {
        _provider = provider;
        _logger = provider.GetRequiredService<ILogger<CommandLineConfigurator>>();
        _makeOptionMethod = GetType().GetMethod(nameof(MakeOption), BindingFlags.Instance | BindingFlags.NonPublic)!;
        _rootCommand = MakeRootCommand(verbDefinitions, moduleDefinitions);
    }

    /// <summary>
    /// Builds the root command and returns it.
    /// </summary>
    private RootCommand MakeRootCommand(IEnumerable<VerbDefinition> verbDefinitions, IEnumerable<ModuleDefinition> moduleDefinitions)
    {
        var rootCommand = new RootCommand("");

        var modules = new Dictionary<string, Command>();

        // Create the modules first so we can tie verbs to them
        foreach (var module in moduleDefinitions.OrderBy(v => v.Name.Length).ThenBy(v => v.Name.Last()))
        {
            if (modules.ContainsKey(module.Name))
                throw new InvalidOperationException($"Module {module.Name} already exists can't define it again");

            var nameParts = module.Name.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var localName = nameParts[^1];
            var command = new Command(localName, module.Description);
            modules.Add(module.Name, command);

            if (nameParts.Length > 1)
            {
                var parentName = string.Join(" ", nameParts[..^1]);
                if (!modules.TryGetValue(parentName, out var parent))
                    throw new InvalidOperationException($"Parent module {module.Name} does not exist");

                parent.Add(command);
            }
            else
            {
                rootCommand.Add(command);
            }

        }

        foreach (var verbDefinition in verbDefinitions.OrderBy(v => v.Name.Last()))
        {
            Command parentCommand = rootCommand;
            var nameParts = verbDefinition.Name.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length > 1)
            {
                var moduleName = string.Join(" ", nameParts[..^1]);
                if (!modules.TryGetValue(moduleName, out var moduleCommand))
                    throw new InvalidOperationException($"Module {moduleName} does not exist");
                parentCommand = moduleCommand;
            }

            var command = new Command(nameParts[^1], verbDefinition.Description);
            var getters = new List<Func<ParseResult, CancellationToken, object?>>();

            foreach (var optionDefinition in verbDefinition.Options)
            {
                if (optionDefinition.IsInjected)
                {
                    // Injected options are pulled from the service provider or runtime context
                    if (optionDefinition.Type == typeof(IRenderer))
                        getters.Add((_, _) => CurrentRenderer!);
                    else if (optionDefinition.Type == typeof(CancellationToken))
                        getters.Add((_, ct) => ct);
                    else
                        getters.Add((_, _) => _provider.GetRequiredService(optionDefinition.Type));
                }
                else
                {
                    var option = (Option)_makeOptionMethod.MakeGenericMethod(optionDefinition.Type)
                        .Invoke(this, [optionDefinition, getters])!;

                    option.Required = !optionDefinition.IsOptional;

                    command.Add(option);
                }
            }
            command.Action = new CommandHandler(_provider, getters, verbDefinition.Info, () => CurrentRenderer!);

            parentCommand.Add(command);
        }
        return rootCommand;
    }

    private Option MakeOption<T>(OptionDefinition optionDefinition, List<Func<ParseResult, CancellationToken, object?>> getters) where T : notnull
    {
        var option = new Option<T>("--" + optionDefinition.LongName, ["-" + optionDefinition.ShortName]);
        option.Description = optionDefinition.HelpText;

        option.CustomParser = result =>
        {
            var service = _provider.GetService<IOptionParser<T>>();
            if (service is null) return default!;
            if (service.TryParse(result.Tokens.Single().Value, out var itm, out var error)) return itm;
            result.AddError(error);
            return default!;
        };

        getters.Add((pr, _) => pr.GetValue(option));

        return option;
    }

    private async Task<bool> RunLink(string[] args, CancellationToken cancellationToken)
    {
        // NOTE(erri120): The CLI framework we're using was designed for verbs and options only.
        // This method circumvents the entire framework to deal with this invocation:
        // NexusMods.App nxm://something
        // This invocation can't be parsed with the CLI framework, so we hand-role the parsing.
        // See https://github.com/Nexus-Mods/NexusMods.App/issues/1677 for details.

        if (args.Length != 1) return false;
        if (!Uri.TryCreate(args[0], UriKind.Absolute, out var uri)) return false;

        var handlers = _provider.GetServices<IIpcProtocolHandler>().ToArray();
        var handler = handlers.FirstOrDefault(handler => handler.Protocol.Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase));

        if (handler is null) return false;

        try
        {
            _logger.LogInformation("Using handler {Handler} for `{Scheme}`", handler.GetType(), uri.Scheme);
            await handler.Handle(uri.ToString(), cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception while running handler {Handler} for `{Uri}`", handler.GetType(), uri);
        }

        return true;
    }

    /// <summary>
    /// Runs the commandline parser and executes the verb using the given renderer and arguments.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="renderer"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<int> RunAsync(string[] args, IRenderer renderer, CancellationToken token)
    {
        if (await RunLink(args, token)) return 0;
        if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("Received command `{Command}`", string.Join(" ", args));

        CurrentRenderer = renderer;

        var textWriter = new RendererTextWriter(renderer);
        var config = new InvocationConfiguration
        {
            Output = textWriter,
            Error = textWriter,
        };

        var parseResult = _rootCommand.Parse(args);
        return await parseResult.InvokeAsync(config, token);
    }
}
