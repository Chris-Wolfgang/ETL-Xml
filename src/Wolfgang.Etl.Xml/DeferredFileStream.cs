using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// A forward-only file stream that does not touch the file system until the first read or write.
/// The path-based pipeline factories use it so that composing a pipeline has no side effects: an
/// abandoned or never-started pipeline neither locks an input file nor truncates an output file,
/// and a dry run (which never writes) never creates or truncates the destination.
/// </summary>
internal sealed class DeferredFileStream : Stream
{
    private readonly string _path;
    private readonly bool _forWrite;
    private FileStream? _inner;
    private bool _disposed;



    private DeferredFileStream(string path, bool forWrite)
    {
        _path = path;
        _forWrite = forWrite;
    }



    internal static DeferredFileStream ForRead(string path) => new DeferredFileStream(path, forWrite: false);



    internal static DeferredFileStream ForWrite(string path) => new DeferredFileStream(path, forWrite: true);



    private FileStream Open()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DeferredFileStream));
        }

        if (_inner is null)
        {
            _inner = _forWrite
                ? new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true)
                : new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        }

        return _inner;
    }



    /// <inheritdoc />
    public override bool CanRead => !_disposed && !_forWrite;



    /// <inheritdoc />
    public override bool CanWrite => !_disposed && _forWrite;



    /// <inheritdoc />
    public override bool CanSeek => false;



    /// <inheritdoc />
    public override long Length => throw new NotSupportedException();



    /// <inheritdoc />
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }



    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();



    /// <inheritdoc />
    public override void SetLength(long value) => throw new NotSupportedException();



    // XmlReader / XmlWriter / XmlSerializer drive the stream synchronously, so the synchronous
    // members are the adapter's contract — the banned-API "use the async member" rule does not
    // apply to a Stream implementation that exists to receive those calls.
#pragma warning disable RS0030
    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) => Open().Read(buffer, offset, count);



#pragma warning restore RS0030



    /// <inheritdoc />
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => Open().ReadAsync(buffer, offset, count, cancellationToken);



#pragma warning disable RS0030
    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => Open().Write(buffer, offset, count);
#pragma warning restore RS0030



    /// <inheritdoc />
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => Open().WriteAsync(buffer, offset, count, cancellationToken);



#pragma warning disable RS0030
    /// <inheritdoc />
    public override void Flush() => _inner?.Flush();
#pragma warning restore RS0030



    /// <inheritdoc />
    public override Task FlushAsync(CancellationToken cancellationToken)
        => _inner is null ? Task.CompletedTask : _inner.FlushAsync(cancellationToken);



    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposed = true;
            _inner?.Dispose();
        }

        base.Dispose(disposing);
    }
}
