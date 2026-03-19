using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Wolfgang.Etl.Xml.Tests.Unit.TestModels;

/// <summary>
/// A <see cref="MemoryStream"/> that captures its buffer into a list when disposed,
/// so the content can be inspected after the stream is closed.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class CapturingMemoryStream : MemoryStream
{
    private readonly IList<byte[]> _capturedBuffers;



    public CapturingMemoryStream(IList<byte[]> capturedBuffers)
    {
        _capturedBuffers = capturedBuffers;
    }



    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _capturedBuffers.Add(ToArray());
        }

        base.Dispose(disposing);
    }
}
