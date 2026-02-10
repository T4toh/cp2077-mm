using System.Text;
using NexusMods.Sdk.ProxyConsole;

namespace NexusMods.SingleProcess;

/// <summary>
/// A TextWriter that delegates to an IRenderer, replacing the old IConsole/IStandardStreamWriter
/// adapters removed in System.CommandLine 2.0.2.
/// </summary>
internal class RendererTextWriter(IRenderer renderer) : TextWriter
{
    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(string? value)
    {
        renderer.RenderAsync(new Text { Template = value ?? string.Empty })
            .AsTask()
            .Wait(CancellationToken.None);
    }
}
