using FluentAssertions;
using NexusMods.Abstractions.Loadouts.Synchronizers;
using NexusMods.Hashing.xxHash3;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using Xunit;

namespace NexusMods.DataModel.Synchronizer.Tests;

public class BaselineRuleTests
{
    private static GamePath P(string path) => new(LocationId.Game, path);
    private static (Hash, Size) F(string content) => (content.xxHash3AsUtf8(), Size.FromLong(content.Length));

    private static Dictionary<GamePath, (Hash Hash, Size Size)> Apply(
        Dictionary<GamePath, (Hash Hash, Size Size)> previous,
        (string Path, string Content)[] disk,
        Dictionary<GamePath, Hash?> owned) =>
        BaselineRule.Apply(previous, disk.Select(d => (P(d.Path), d.Content.xxHash3AsUtf8(), Size.FromLong(d.Content.Length))), owned);

    [Fact]
    public void FirstTime_EverythingOnDiskIsOriginal()
    {
        var result = Apply([], [("bin/a.exe", "a"), ("r6/b.ini", "b")], []);
        result.Should().BeEquivalentTo(new Dictionary<GamePath, (Hash, Size)> { [P("bin/a.exe")] = F("a"), [P("r6/b.ini")] = F("b") });
    }

    [Fact]
    public void ModOwnedPathWithModContent_KeepsPreviousEntry()
    {
        var result = Apply(new() { [P("r6/b.ini")] = F("vanilla") }, [("r6/b.ini", "mod")], new() { [P("r6/b.ini")] = "mod".xxHash3AsUtf8() });
        result[P("r6/b.ini")].Should().Be(F("vanilla"));
    }

    [Fact]
    public void ModOwnedPathWithoutPreviousEntry_StaysOut()
    {
        var result = Apply([], [("archive/pc/mod/a.archive", "mod")], new() { [P("archive/pc/mod/a.archive")] = "mod".xxHash3AsUtf8() });
        result.Should().BeEmpty();
    }

    [Fact]
    public void ModOwnedPathOverwrittenByAPatch_AdoptsTheNewContent()
    {
        var result = Apply(new() { [P("r6/b.ini")] = F("v1") }, [("r6/b.ini", "v2")], new() { [P("r6/b.ini")] = "mod".xxHash3AsUtf8() });
        result[P("r6/b.ini")].Should().Be(F("v2"));
    }

    [Fact]
    public void ExternalChange_KeepsPreviousEntryWhateverTheDiskHas()
    {
        var result = Apply(new() { [P("r6/b.ini")] = F("vanilla") }, [("r6/b.ini", "edited by hand")], new() { [P("r6/b.ini")] = null });
        result[P("r6/b.ini")].Should().Be(F("vanilla"));
    }

    [Fact]
    public void DeletedOnPurpose_KeepsPreviousEntry()
    {
        var result = Apply(new() { [P("bin/intro.bk2")] = F("intro") }, [], new() { [P("bin/intro.bk2")] = null });
        result[P("bin/intro.bk2")].Should().Be(F("intro"));
    }

    [Fact]
    public void IntrinsicPath_KeepsPreviousEntry()
    {
        var result = Apply([], [("r6/cache/modded/mods.json", "generated")], new() { [P("r6/cache/modded/mods.json")] = null });
        result.Should().BeEmpty();
    }

    [Fact]
    public void PatchedOriginal_IsAdopted()
    {
        var result = Apply(new() { [P("bin/a.exe")] = F("v1") }, [("bin/a.exe", "v2")], []);
        result[P("bin/a.exe")].Should().Be(F("v2"));
    }

    [Fact]
    public void OriginalGoneFromDisk_LeavesTheList()
    {
        var result = Apply(new() { [P("bin/old.dll")] = F("old") }, [], []);
        result.Should().BeEmpty();
    }
}
