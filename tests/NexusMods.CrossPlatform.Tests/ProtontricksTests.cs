using FluentAssertions;
using NexusMods.Games.Generic.Dependencies;

namespace NexusMods.CrossPlatform.Tests;

public class ProtontricksTests
{
    [Theory]
    [InlineData("protontricks (1.11.1)\n", "1.11.1")]
    [InlineData("protontricks (1.11.1)", "1.11.1")]
    [InlineData("protontricks", null)]
    public void TestTryParseVersion(string input, string? expectedRawVersion)
    {
        _ = ProtontricksNativeDependency.TryParseVersion(input, out var rawVersion, out _);
        rawVersion.Should().Be(expectedRawVersion);
    }

    [Fact]
    public async Task TestMakeInstallCommand()
    {
        var native = await new ProtontricksNativeDependency(runner: null!).MakeInstallCommand(1091500, ["d3dcompiler_47", "vcrun2022"]);
        native.TargetFilePath.Should().Be("protontricks");
        native.Arguments.Should().Be("1091500 -q d3dcompiler_47 vcrun2022");

        var flatpak = await new ProtontricksFlatpakDependency(runner: null!).MakeInstallCommand(1091500, ["vcrun2022"]);
        flatpak.TargetFilePath.Should().Be("flatpak");
        flatpak.Arguments.Should().Be("run com.github.Matoking.protontricks 1091500 -q vcrun2022");
    }
}
