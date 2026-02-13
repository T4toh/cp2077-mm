using NexusMods.Sdk.NexusModsApi;

namespace NexusMods.Games.RedEngine.Cyberpunk2077;

public record EssentialMod(string Name, ModId ModId, string Description);

public static class EssentialMods
{
    public static readonly EssentialMod[] Cyberpunk2077Essentials =
    [
        new("Redscript", ModId.From(1511), "Compiler for Cyberpunk 2077 scripts. Required for many mods that change game logic."),
        new("RED4ext", ModId.From(2380), "Scripting library and mod loader for REDengine 4. Required for many advanced mods."),
        new("Cyber Engine Tweaks", ModId.From(107), "Scripting framework and utility for Cyberpunk 2077. Provides a console and many fixes."),
        new("ArchiveXL", ModId.From(4198), "A RED4ext plugin that allows mods to load custom resources without touching original game files."),
        new("TweakXL", ModId.From(4197), "A RED4ext plugin that allows mods to modify the game's TweakDB."),
        new("Codeware", ModId.From(7780), "A RED4ext plugin and script library providing utility classes for other mods."),
        new("Equipment-EX", ModId.From(6945), "A transmog and clothing system that uses ArchiveXL and TweakXL.")
    ];
}
