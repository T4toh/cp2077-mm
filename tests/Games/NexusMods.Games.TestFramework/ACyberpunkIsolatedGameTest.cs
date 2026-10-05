using Microsoft.Extensions.DependencyInjection;
using NexusMods.Backend;
using NexusMods.CrossPlatform;
using NexusMods.Games.Generic;
using NexusMods.Games.RedEngine;
using NexusMods.Games.RedEngine.Cyberpunk2077;
using NexusMods.Paths;
using NexusMods.StandardGameLocators.TestHelpers;
using Xunit;

namespace NexusMods.Games.TestFramework;

/// <summary>
/// A override for the <see cref="AIsolatedGameTest{TGame}"/> for the <see cref="Cyberpunk2077Game"/>.
/// </summary>
public class ACyberpunkIsolatedGameTest<TTest>(ITestOutputHelper helper) : AIsolatedGameTest<TTest, Cyberpunk2077Game>(helper)
{
    /// <summary>
    /// The game's executable: a vanilla list taken from the disk is only saved when it holds this file, so the stubbed
    /// game folder starts with it like a real install.
    /// </summary>
    public static Dictionary<RelativePath, byte[]> PrimaryFile => new() { [(RelativePath)"bin/x64/Cyberpunk2077.exe"] = "Cyberpunk2077.exe"u8.ToArray() };

    protected override IServiceCollection AddServices(IServiceCollection services)
    {
        return base.AddServices(services)
            .AddOSInterop()
            .AddRuntimeDependencies()
            .AddGenericGameSupport()
            .AddUniversalGameLocator<Cyberpunk2077Game>(new Version("1.61"), PrimaryFile)
            .AddRedEngineGames();
    }
}
