using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// Restores per-class coverage for paths the TestKit contract base stopped exercising after the
/// 0.14 bump — the typed-<c>ILogger&lt;T&gt;</c> constructors on the multi-stream extractor / both
/// loaders, the custom-<see cref="XmlWriterSettings"/> serialize branch, the invalid-NCName root
/// throw, and the deserialize-to-null skip.
/// </summary>
public sealed class XmlCoverageTests
{
    private static readonly PersonRecord[] Sample =
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
    };


    [Fact]
    public async Task MultiStreamExtractor_streams_logger_ctor_extracts()
    {
        var extractor = new XmlMultiStreamExtractor<PersonRecord>
        (
            SerializeEach(Sample),
            NullLogger<XmlMultiStreamExtractor<PersonRecord>>.Instance
        );

        Assert.Equal(1, await CountAsync(extractor).ConfigureAwait(false));
    }


    [Fact]
    public async Task MultiStreamExtractor_streams_readerSettings_logger_ctor_extracts()
    {
        var extractor = new XmlMultiStreamExtractor<PersonRecord>
        (
            SerializeEach(Sample),
            new XmlReaderSettings(),
            NullLogger<XmlMultiStreamExtractor<PersonRecord>>.Instance
        );

        Assert.Equal(1, await CountAsync(extractor).ConfigureAwait(false));
    }


    [Fact]
    public async Task MultiStreamExtractor_when_a_stream_deserializes_to_null_skips_it()
    {
        const string nilXml =
            "<PersonRecord xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:nil=\"true\" />";
        var streams = new[] { new MemoryStream(Encoding.UTF8.GetBytes(nilXml)) };
        var extractor = new XmlMultiStreamExtractor<PersonRecord>(streams);

        Assert.Equal(0, await CountAsync(extractor).ConfigureAwait(false));
    }


    [Fact]
    public async Task MultiStreamLoader_streamFactory_logger_ctor_loads()
    {
        var buffers = new List<MemoryStream>();
        var loader = new XmlMultiStreamLoader<PersonRecord>
        (
            _ => Capture(buffers),
            NullLogger<XmlMultiStreamLoader<PersonRecord>>.Instance
        );

        await loader.LoadAsync(ToAsync(Sample)).ConfigureAwait(false);

        Assert.Equal(1, loader.CurrentItemCount);
    }


    [Fact]
    public async Task MultiStreamLoader_streamFactory_writerSettings_logger_ctor_serializes_with_settings()
    {
        var buffers = new List<MemoryStream>();
        var loader = new XmlMultiStreamLoader<PersonRecord>
        (
            _ => Capture(buffers),
            new XmlWriterSettings { OmitXmlDeclaration = true },
            NullLogger<XmlMultiStreamLoader<PersonRecord>>.Instance
        );

        await loader.LoadAsync(ToAsync(Sample)).ConfigureAwait(false);

        Assert.Equal(1, loader.CurrentItemCount);
        var xml = Encoding.UTF8.GetString(Assert.Single(buffers).ToArray());
        // OmitXmlDeclaration = true suppresses the <?xml ?> prolog — proof the custom
        // writer settings were actually applied, not silently discarded (the default
        // serialize path emits the declaration).
        Assert.DoesNotContain("<?xml", xml, StringComparison.Ordinal);
    }


    [Fact]
    public async Task SingleStreamLoader_stream_logger_ctor_loads()
    {
        using var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance
        );

        await loader.LoadAsync(ToAsync(Sample)).ConfigureAwait(false);

        Assert.Equal(1, loader.CurrentItemCount);
    }


    [Fact]
    public void SingleStreamLoader_when_rootElementName_is_not_a_valid_NCName_throws_ArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>
        (
            () => new XmlSingleStreamLoader<PersonRecord>
            (
                new MemoryStream(),
                new XmlSingleStreamLoaderOptions { RootElementName = "not a valid name" }
            )
        );

        Assert.Equal("rootElementName", ex.ParamName);
    }


    private static MemoryStream Capture(List<MemoryStream> buffers)
    {
        var ms = new MemoryStream();
        buffers.Add(ms);
        return ms;
    }


    private static IEnumerable<Stream> SerializeEach(IEnumerable<PersonRecord> items)
    {
        var serializer = new XmlSerializer(typeof(PersonRecord));
        foreach (var item in items)
        {
            var ms = new MemoryStream();
            serializer.Serialize(ms, item);
            ms.Position = 0;
            yield return ms;
        }
    }


    private static async Task<int> CountAsync(XmlMultiStreamExtractor<PersonRecord> extractor)
    {
        var count = 0;
        await foreach (var _ in extractor.ExtractAsync().ConfigureAwait(false))
        {
            count++;
        }

        return count;
    }


    private static async IAsyncEnumerable<PersonRecord> ToAsync(IEnumerable<PersonRecord> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
