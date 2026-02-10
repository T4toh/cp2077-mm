using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusMods.Abstractions.Cli;
using NexusMods.Sdk.ProxyConsole;

namespace NexusMods.SingleProcess;

/// <summary>
/// Command handler for linking a verb definition to the parser.
/// Extends AsynchronousCommandLineAction (replaces ICommandHandler removed in System.CommandLine 2.0.2).
/// </summary>
internal class CommandHandler(
    IServiceProvider serviceProvider,
    List<Func<ParseResult, CancellationToken, object?>> getters,
    MethodInfo methodInfo,
    Func<IRenderer> rendererProvider)
    : AsynchronousCommandLineAction
{
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        try
        {
            // Resolve all the parameters
            var args = GC.AllocateUninitializedArray<object?>(getters.Count);
            for (var i = 0; i < getters.Count; i++)
            {
                args[i] = getters[i](parseResult, cancellationToken);
            }

            // Invoke the method
            return await (Task<int>)methodInfo.Invoke(null, args)!;
        }
        catch (Exception ex)
        {
            serviceProvider.GetRequiredService<ILogger<CommandHandler>>().LogError(ex, "An error occurred while executing the command {0}", methodInfo.Name);
            await rendererProvider().Error(ex, "An error occurred while executing the command");
            return -1;
        }
    }
}
