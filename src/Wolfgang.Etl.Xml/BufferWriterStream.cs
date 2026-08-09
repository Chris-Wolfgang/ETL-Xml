using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// A write-only <see cref="Stream"/> adapter over an <see cref="IBufferWriter{T}"/> of bytes (#8).
/// The XML writer streams its serialized bytes straight into the caller's buffer writer (for
/// example a <c>System.IO.Pipelines.PipeWriter</c> or an <c>ArrayBufferWriter&lt;byte&gt;</c>) with no
/// intermediate <see cref="MemoryStream"/>. Reads and seeks are not supported; the underlying
/// buffer writer is never disposed (the caller owns it).
/// </summary>
internal sealed class BufferWriterStream : Stream
{
    private readonly IBufferWriter<byte> _writer;


    internal BufferWriterStream(IBufferWriter<byte> writer) =>
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));


    public override bool CanWrite => true;


    public override bool CanRead => false;


    public override bool CanSeek => false;


    public override long Length => throw new NotSupportedException();


    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }


    // The XmlSerializer writes synchronously, so the synchronous Write override is the adapter's
    // contract — the banned-API analyzer's "use WriteAsync" rule does not apply to a Stream
    // implementation whose whole purpose is to receive those synchronous writes.
#pragma warning disable RS0030
    public override void Write(byte[] buffer, int offset, int count)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        WriteBytes(buffer.AsSpan(offset, count));
    }
#pragma warning restore RS0030


#if NET8_0_OR_GREATER
    public override void Write(ReadOnlySpan<byte> buffer) => WriteBytes(buffer);
#endif


    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        if (buffer is null)
        {
            return Task.FromException(new ArgumentNullException(nameof(buffer)));
        }

        WriteBytes(buffer.AsSpan(offset, count));
        return Task.CompletedTask;
    }


    public override void Flush()
    {
        // Nothing to flush — writes are committed to the buffer writer synchronously.
    }


    public override Task FlushAsync(CancellationToken cancellationToken) =>
        cancellationToken.IsCancellationRequested ? Task.FromCanceled(cancellationToken) : Task.CompletedTask;


    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();


    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();


    public override void SetLength(long value) => throw new NotSupportedException();


    private void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
        {
            return;
        }

        var span = _writer.GetSpan(bytes.Length);
        bytes.CopyTo(span);
        _writer.Advance(bytes.Length);
    }
}
