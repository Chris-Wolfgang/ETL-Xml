using System;
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
/// Regression tests for two defects found by the mutation-testing pass (#127):
/// the <c>(stream, logger)</c> constructors silently ignoring the documented
/// <c>LeaveOpen = true</c> default, and a null-deserializing element causing the
/// element that follows it to be silently dropped.
/// </summary>
public sealed class XmlSingleStreamRegressionTests
{
    private static readonly PersonRecord[] One =
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
    };


    // ── LeaveOpen default must not depend on which constructor was used ───────

    [Fact]
    public async Task Loader_options_and_logger_ctor_honours_default_LeaveOpen_true()
    {
        var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            options: null,
            NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance
        );

        await loader.LoadAsync(One.ToAsyncEnumerable()).ConfigureAwait(false);

        Assert.True(stream.CanWrite);
    }


    [Fact]
    public async Task Loader_stream_logger_ctor_honours_default_LeaveOpen_true()
    {
        var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance
        );

        await loader.LoadAsync(One.ToAsyncEnumerable()).ConfigureAwait(false);

        // Regression: this constructor never assigned _leaveOpen, so it defaulted to
        // false and closed the caller's stream — contradicting the documented default
        // and diverging from every other constructor.
        Assert.True(stream.CanWrite);
    }


    [Fact]
    public async Task Extractor_stream_logger_ctor_honours_default_LeaveOpen_true()
    {
        using var source = await BuildXmlAsync().ConfigureAwait(false);
        var extractor = new XmlSingleStreamExtractor<PersonRecord>
        (
            source,
            NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance
        );

        await foreach (var _ in extractor.ExtractAsync().ConfigureAwait(false))
        {
        }

        Assert.True(source.CanRead);
    }


    [Fact]
    public async Task Extractor_options_and_logger_ctor_honours_explicit_LeaveOpen_false()
    {
        var source = await BuildXmlAsync().ConfigureAwait(false);
        var extractor = new XmlSingleStreamExtractor<PersonRecord>
        (
            source,
            new XmlSingleStreamExtractorOptions { LeaveOpen = false },
            NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance
        );

        await foreach (var _ in extractor.ExtractAsync().ConfigureAwait(false))
        {
        }

        Assert.False(source.CanRead);
    }


    // ── A null element must not consume the sibling that follows it ───────────

    [Fact]
    public async Task ExtractAsync_when_element_deserializes_to_null_does_not_drop_the_next_sibling()
    {
        // Regression: `needsRead` was only cleared after the null check, so a null
        // element left the loop reading one extra node — silently swallowing the
        // record that followed. Here Bob was lost and only Carol came through.
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
        await foreach (var item in extractor.ExtractAsync().ConfigureAwait(false))
        {
            results.Add(item);
        }

        Assert.Equal(2, results.Count);
        Assert.Equal("Bob", results[0].FirstName);
        Assert.Equal("Carol", results[1].FirstName);
    }


    [Fact]
    public async Task ExtractAsync_when_only_element_deserializes_to_null_yields_nothing()
    {
        const string xml =
            "<?xml version=\"1.0\"?>" +
            "<ArrayOfPersonRecord xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<PersonRecord xsi:nil=\"true\" />" +
            "</ArrayOfPersonRecord>";
        using var source = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        var extractor = new XmlSingleStreamExtractor<PersonRecord>(source);

        var results = new List<PersonRecord>();
        await foreach (var item in extractor.ExtractAsync().ConfigureAwait(false))
        {
            results.Add(item);
        }

        Assert.Empty(results);
    }


    [Fact]
    public async Task ExtractAsync_when_consecutive_null_elements_keeps_the_following_records()
    {
        const string xml =
            "<?xml version=\"1.0\"?>" +
            "<ArrayOfPersonRecord xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<PersonRecord xsi:nil=\"true\" />" +
            "<PersonRecord xsi:nil=\"true\" />" +
            "<PersonRecord><FirstName>Dave</FirstName><LastName>Brown</LastName><Age>40</Age></PersonRecord>" +
            "</ArrayOfPersonRecord>";
        using var source = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        var extractor = new XmlSingleStreamExtractor<PersonRecord>(source);

        var results = new List<PersonRecord>();
        await foreach (var item in extractor.ExtractAsync().ConfigureAwait(false))
        {
            results.Add(item);
        }

        Assert.Equal("Dave", Assert.Single(results).FirstName);
    }


    // ── The canonical ctor closes the options + logger gap ────────────────────

    [Fact]
    public async Task Loader_canonical_ctor_accepts_options_and_logger_together()
    {
        var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            new XmlSingleStreamLoaderOptions { RootElementName = "People", LeaveOpen = true },
            NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance
        );

        await loader.LoadAsync(One.ToAsyncEnumerable()).ConfigureAwait(false);

        var content = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Contains("<People>", content, StringComparison.Ordinal);
        Assert.True(stream.CanWrite);
    }


    [Fact]
    public void Loader_canonical_ctor_when_stream_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlSingleStreamLoader<PersonRecord>
            (
                (Stream)null!,
                options: null,
                NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance
            )
        );
    }


    [Fact]
    public void Extractor_canonical_ctor_when_stream_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlSingleStreamExtractor<PersonRecord>
            (
                null!,
                options: null,
                NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance
            )
        );
    }


    [Fact]
    public async Task Loader_canonical_ctor_allows_null_logger()
    {
        var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>(stream, options: null, logger: null);

        await loader.LoadAsync(One.ToAsyncEnumerable()).ConfigureAwait(false);

        Assert.Equal(1, loader.CurrentItemCount);
    }


    private static async Task<MemoryStream> BuildXmlAsync()
    {
        var ms = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            ms,
            new XmlSingleStreamLoaderOptions { LeaveOpen = true }
        );
        await loader.LoadAsync(One.ToAsyncEnumerable()).ConfigureAwait(false);
        ms.Position = 0;
        return ms;
    }
}
