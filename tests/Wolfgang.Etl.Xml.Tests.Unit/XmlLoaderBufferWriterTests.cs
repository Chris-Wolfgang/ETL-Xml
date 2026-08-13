using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// A minimal growable <see cref="IBufferWriter{T}"/> of bytes for the tests — used instead of
/// <c>System.Buffers.ArrayBufferWriter&lt;byte&gt;</c>, which does not exist on the .NET Framework
/// test TFMs (net462–net481). <c>IBufferWriter&lt;byte&gt;</c> itself is available everywhere via
/// <c>System.Memory</c>.
/// </summary>
internal sealed class TestBufferWriter : IBufferWriter<byte>
{
    private byte[] _buffer = new byte[256];


    public int WrittenCount { get; private set; }


    public ReadOnlySpan<byte> WrittenSpan => _buffer.AsSpan(0, WrittenCount);


    public void Advance(int count) => WrittenCount += count;


    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsMemory(WrittenCount);
    }


    public Span<byte> GetSpan(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsSpan(WrittenCount);
    }


    private void EnsureCapacity(int sizeHint)
    {
        var needed = WrittenCount + Math.Max(1, sizeHint);
        if (needed > _buffer.Length)
        {
            Array.Resize(ref _buffer, Math.Max(_buffer.Length * 2, needed));
        }
    }
}


/// <summary>
/// Verifies the <see cref="IBufferWriter{T}"/>-of-bytes loader overloads (#8): serialized bytes flow
/// into the caller's buffer writer and produce exactly the same XML as the stream overloads, and the
/// <c>BufferWriterStream</c> adapter honours its write-only contract.
/// </summary>
public sealed class XmlLoaderBufferWriterTests
{
    private static readonly PersonRecord[] Sample =
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25 },
    };


    [Fact]
    public async Task SingleStreamLoader_buffer_writer_produces_same_xml_as_stream()
    {
        var bufferWriter = new TestBufferWriter();
        var loader = new XmlSingleStreamLoader<PersonRecord>(bufferWriter);
        await loader.LoadAsync(Sample.ToAsyncEnumerable());
        var viaBuffer = Encoding.UTF8.GetString(bufferWriter.WrittenSpan.ToArray());

        using var ms = new MemoryStream();
        var streamLoader = new XmlSingleStreamLoader<PersonRecord>
        (
            ms,
            new XmlSingleStreamLoaderOptions { LeaveOpen = true }
        );
        await streamLoader.LoadAsync(Sample.ToAsyncEnumerable());
        var viaStream = Encoding.UTF8.GetString(ms.ToArray());

        Assert.Equal(viaStream, viaBuffer);
        Assert.Equal(2, loader.CurrentItemCount);
    }


    [Fact]
    public async Task MultiStreamLoader_buffer_writer_factory_writes_each_record()
    {
        var buffers = new List<TestBufferWriter>();
        var loader = new XmlMultiStreamLoader<PersonRecord>
        (
            _ =>
            {
                var bufferWriter = new TestBufferWriter();
                buffers.Add(bufferWriter);
                return bufferWriter;
            }
        );

        await loader.LoadAsync(Sample.ToAsyncEnumerable());

        Assert.Equal(2, buffers.Count);
        Assert.All(buffers, b => Assert.True(b.WrittenCount > 0));
    }


    [Fact]
    public void SingleStreamLoader_when_bufferWriter_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => new XmlSingleStreamLoader<PersonRecord>((IBufferWriter<byte>)null!)
        );

        Assert.Equal("bufferWriter", ex.ParamName);
    }


    [Fact]
    public async Task MultiStreamLoader_when_factory_returns_null_buffer_writer_throws_InvalidOperationException()
    {
        var loader = new XmlMultiStreamLoader<PersonRecord>(_ => (IBufferWriter<byte>)null!);

        // A null buffer writer routes through the loader's existing null-stream guard.
        await Assert.ThrowsAsync<InvalidOperationException>
        (
            () => loader.LoadAsync(Sample.ToAsyncEnumerable())
        );
    }


    [Fact]
    public void MultiStreamLoader_when_bufferWriterFactory_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => new XmlMultiStreamLoader<PersonRecord>((Func<PersonRecord, IBufferWriter<byte>>)null!)
        );

        Assert.Equal("bufferWriterFactory", ex.ParamName);
    }


    [Fact]
    public void BufferWriterStream_supports_writing_only()
    {
        var stream = new BufferWriterStream(new TestBufferWriter());

        Assert.True(stream.CanWrite);
        Assert.False(stream.CanRead);
        Assert.False(stream.CanSeek);
        Assert.Throws<NotSupportedException>(() => stream.Length);
        Assert.Throws<NotSupportedException>(() => stream.Position);
        Assert.Throws<NotSupportedException>(() => stream.Position = 0);
        Assert.Throws<NotSupportedException>(() => stream.Read(new byte[1], 0, 1));
        Assert.Throws<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
        Assert.Throws<NotSupportedException>(() => stream.SetLength(0));
    }


    [Fact]
    public async Task BufferWriterStream_WriteAsync_appends_bytes_and_honours_cancellation()
    {
        var bufferWriter = new TestBufferWriter();
        var stream = new BufferWriterStream(bufferWriter);

        await stream.WriteAsync(new byte[] { 1, 2, 3 }, 0, 3, CancellationToken.None);
        stream.Flush();
        Assert.Equal(3, bufferWriter.WrittenCount);

        // An empty write is a no-op.
        await stream.WriteAsync(Array.Empty<byte>(), 0, 0, CancellationToken.None);
        Assert.Equal(3, bufferWriter.WrittenCount);

        // A null buffer surfaces ArgumentNullException.
        await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => stream.WriteAsync(null!, 0, 0, CancellationToken.None)
        );

        // A pre-cancelled token is honoured.
        await Assert.ThrowsAsync<TaskCanceledException>
        (
            () => stream.WriteAsync(new byte[] { 4 }, 0, 1, new CancellationToken(canceled: true))
        );
    }
}
