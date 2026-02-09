using JetBrains.Annotations;
using NexusMods.Abstractions.Diagnostics;
using NexusMods.Abstractions.Diagnostics.Emitters;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Generators.Diagnostics;
using NexusMods.Sdk.Loadouts;

namespace Examples.Diagnostics;

// Needs to be static
internal static partial class Diagnostics
{
    [DiagnosticTemplate]
    [UsedImplicitly]
    internal static IDiagnosticTemplate ModCompatabilityObsoleteTemplate = DiagnosticTemplateBuilder
        .Start()
        .WithId(new DiagnosticId("Examples", number: 6))
        .WithTitle("Mod is obsolete")
        .WithSeverity(DiagnosticSeverity.Warning)
        .WithSummary("Mod is obsolete")
        .WithDetails("""
Mod {Mod} has been made obsolete:

> {ModName} is obsolete because {ReasonPhrase}
""")
        .WithMessageData(messageBuilder => messageBuilder
            .AddValue<string>("ModName")
            .AddValue<string>("ReasonPhrase")
        )
        .Finish();
}

file class MyDiagnosticLoadoutEmitter : ILoadoutDiagnosticEmitter
{
    public async IAsyncEnumerable<Diagnostic> Diagnose(
        Loadout.ReadOnly loadout,
        CancellationToken cancellationToken)
    {
        var someMod = LoadoutItem.FindByLoadout(loadout.Db, loadout).First();

        // this "Create" method was generated for you
        yield return Diagnostics.CreateModCompatabilityObsolete(
            ModName: someMod.Name,
            ReasonPhrase: "it's incompatible"
        );

        await Task.CompletedTask;
    }
}
