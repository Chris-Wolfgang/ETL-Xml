using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// Covers the constructor convergence work and the two defects it exposed:
/// <list type="number">
/// <item>
/// The <c>(stream, logger)</c> overloads initialized their fields by hand and omitted
/// <c>_leaveOpen</c>, so they silently defaulted it to <see langword="false"/> and closed the
/// caller's stream — the opposite of the documented <c>LeaveOpen = true</c>. Every overload now
/// delegates to a single private initializer, so the default cannot drift per overload again.
/// </item>
/// <item>
/// A child element that deserialized to <see langword="null"/> left the reader already advanced,
/// but the loop still issued another read, silently discarding the <em>following</em> sibling.
/// </item>
/// </list>
/// </summary>
public sealed class XmlConstructorConvergenceTests
{
    private static readonly PersonRecord[] TwoPeople =
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25 },
    };


    // ── Defect 1: LeaveOpen default must not depend on which ctor was used ────

    [Fact]
    public async Task Loader_stream_logger_ctor_honours_documented_LeaveOpen_default()
    {
        var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance
        );

        await loader.LoadAsync(TwoPeople.ToAsyncEnumerable());

        // Documented default is LeaveOpen = true, so the caller's stream stays usable.
        Assert.True(stream.CanWrite);
    }


    [Fact]
    public async Task Extractor_stream_logger_ctor_honours_documented_LeaveOpen_default()
    {
        using var source = await BuildXmlAsync();
        var extractor = new XmlSingleStreamExtractor<PersonRecord>
        (
            source,
            NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance
        );

        await DrainAsync(extractor);

        Assert.True(source.CanRead);
    }


    [Fact]
    public async Task Loader_all_ctors_agree_on_the_LeaveOpen_default()
    {
        var viaOptions = new MemoryStream();
        var viaLogger = new MemoryStream();
        var viaCanonical = new MemoryStream();

        await new XmlSingleStreamLoader<PersonRecord>(viaOptions)
            .LoadAsync(TwoPeople.ToAsyncEnumerable());
        await new XmlSingleStreamLoader<PersonRecord>(viaLogger, NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance)
            .LoadAsync(TwoPeople.ToAsyncEnumerable());
        await new XmlSingleStreamLoader<PersonRecord>(viaCanonical, options: null, logger: null)
            .LoadAsync(TwoPeople.ToAsyncEnumerable());

        Assert.True(viaOptions.CanWrite);
        Assert.True(viaLogger.CanWrite);
        Assert.True(viaCanonical.CanWrite);
    }


    // ── The gap the convergence closes: options AND logger, no settings ───────

    [Fact]
    public async Task Loader_canonical_ctor_accepts_options_and_logger_together()
    {
        var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            new XmlSingleStreamLoaderOptions { RootElementName = "People" },
            NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance
        );

        await loader.LoadAsync(TwoPeople.ToAsyncEnumerable());

        var content = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Contains("<People>", content, StringComparison.Ordinal);
    }


    [Fact]
    public async Task Extractor_canonical_ctor_accepts_options_and_logger_together()
    {
        var source = await BuildXmlAsync();
        var extractor = new XmlSingleStreamExtractor<PersonRecord>
        (
            source,
            new XmlSingleStreamExtractorOptions { LeaveOpen = false },
            NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance
        );

        await DrainAsync(extractor);

        // Proof the options were applied rather than silently dropped.
        Assert.False(source.CanRead);
    }


    [Fact]
    public async Task Loader_bufferWriter_canonical_ctor_accepts_options_and_logger_together()
    {
        var writer = new TestBufferWriter();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            writer,
            new XmlSingleStreamLoaderOptions { RootElementName = "People" },
            NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance
        );

        await loader.LoadAsync(TwoPeople.ToAsyncEnumerable());

        var content = Encoding.UTF8.GetString(writer.ToArray());
        Assert.Contains("<People>", content, StringComparison.Ordinal);
    }


    // ── Defect 2: a null-deserializing element must not swallow its sibling ───

    [Fact]
    public async Task Extractor_when_element_deserializes_to_null_still_returns_following_siblings()
    {
        // The xsi:nil element deserializes to null. Deserialize has already advanced the reader
        // past it, so the loop must not read again — doing so used to consume "Bob"'s start tag
        // and drop that record entirely, returning only Carol.
        const string xml =
            "<?xml version=\"1.0\"?>" +
            "<ArrayOfPersonRecord xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<PersonRecord xsi:nil=\"true\" />" +
            "<PersonRecord><FirstName>Bob</FirstName><LastName>Jones</LastName><Age>25</Age></PersonRecord>" +
            "<PersonRecord><FirstName>Carol</FirstName><LastName>White</LastName><Age>35</Age></PersonRecord>" +
            "</ArrayOfPersonRecord>";
        using var source = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        var extractor = new XmlSingleStreamExtractor<PersonRecord>(source);

        var results = new List<PersonRecord>();
        await foreach (var item in extractor.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Equal(2, results.Count);
        Assert.Equal("Bob", results[0].FirstName);
        Assert.Equal("Carol", results[1].FirstName);
    }


    [Fact]
    public async Task Extractor_when_only_element_deserializes_to_null_returns_nothing()
    {
        const string xml =
            "<?xml version=\"1.0\"?>" +
            "<ArrayOfPersonRecord xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<PersonRecord xsi:nil=\"true\" />" +
            "</ArrayOfPersonRecord>";
        using var source = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        var extractor = new XmlSingleStreamExtractor<PersonRecord>(source);

        var results = new List<PersonRecord>();
        await foreach (var item in extractor.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Empty(results);
    }


    [Fact]
    public async Task Extractor_counts_exclude_the_null_element_but_include_its_siblings()
    {
        const string xml =
            "<?xml version=\"1.0\"?>" +
            "<ArrayOfPersonRecord xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<PersonRecord xsi:nil=\"true\" />" +
            "<PersonRecord><FirstName>Bob</FirstName><LastName>Jones</LastName><Age>25</Age></PersonRecord>" +
            "</ArrayOfPersonRecord>";
        using var source = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        var extractor = new XmlSingleStreamExtractor<PersonRecord>(source);

        await DrainAsync(extractor);

        Assert.Equal(1, extractor.CurrentItemCount);
    }


    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task DrainAsync(XmlSingleStreamExtractor<PersonRecord> extractor)
    {
        await foreach (var _ in extractor.ExtractAsync().ConfigureAwait(false))
        {
        }
    }


    private static async Task<MemoryStream> BuildXmlAsync()
    {
        var ms = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            ms,
            new XmlSingleStreamLoaderOptions { LeaveOpen = true }
        );
        await loader.LoadAsync(TwoPeople.ToAsyncEnumerable()).ConfigureAwait(false);
        ms.Position = 0;
        return ms;
    }


    // ArrayBufferWriter<T> does not exist on the netfx / netstandard2.0 test TFMs, so the
    // tests use a portable buffer writer of their own.
    private sealed class TestBufferWriter : IBufferWriter<byte>
    {
        private byte[] _buffer = new byte[256];
        private int _written;


        public byte[] ToArray() => _buffer.AsSpan(0, _written).ToArray();


        public void Advance(int count) => _written += count;


        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            EnsureCapacity(sizeHint);
            return _buffer.AsMemory(_written);
        }


        public Span<byte> GetSpan(int sizeHint = 0)
        {
            EnsureCapacity(sizeHint);
            return _buffer.AsSpan(_written);
        }


        private void EnsureCapacity(int sizeHint)
        {
            if (sizeHint < 1)
            {
                sizeHint = 1;
            }

            if (_written + sizeHint <= _buffer.Length)
            {
                return;
            }

            var grown = new byte[Math.Max(_buffer.Length * 2, _written + sizeHint)];
            Array.Copy(_buffer, grown, _written);
            _buffer = grown;
        }
    }
}
