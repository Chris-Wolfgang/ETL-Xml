using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

public class XmlMultiStreamExtractorTests
    : ExtractorBaseContractTests
    <
        XmlMultiStreamExtractor<PersonRecord>,
        PersonRecord,
        XmlReport
    >
{
    private static readonly XmlSerializer Serializer = new(typeof(PersonRecord));

    private static readonly IReadOnlyList<PersonRecord> ExpectedItems = XmlTestData.People;



    private static IEnumerable<Stream> CreateXmlStreams(int itemCount)
    {
        return ExpectedItems.Take(itemCount).Select(item =>
        {
            var stream = new MemoryStream();
            Serializer.Serialize(stream, item);
            stream.Position = 0;
            return (Stream)stream;
        });
    }



    protected override XmlMultiStreamExtractor<PersonRecord> CreateSut(int itemCount, int maximumItemCount, int skipItemCount, int reportingInterval) =>
        new
        (
            CreateXmlStreams(itemCount),
            new XmlMultiStreamExtractorOptions
            {
                MaximumItemCount = maximumItemCount,
                SkipItemCount = skipItemCount,
                ReportingInterval = reportingInterval,
            }
        );



    protected override IReadOnlyList<PersonRecord> CreateExpectedItems() => ExpectedItems;



    [Fact]
    public async Task ExtractAsync_disposes_streams_after_reading()
    {
        var streams = ExpectedItems.Take(2).Select(item =>
        {
            var stream = new MemoryStream();
            Serializer.Serialize(stream, item);
            stream.Position = 0;
            return stream;
        }).ToList();

        var sut = new XmlMultiStreamExtractor<PersonRecord>(streams);

        var results = new List<PersonRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Equal(2, results.Count);

        foreach (var stream in streams)
        {
            Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
        }
    }



    [Fact]
    public async Task ExtractAsync_when_MaximumItemCount_stops_early_leaves_unreached_streams_open()
    {
        var streams = ExpectedItems.Take(3).Select(item =>
        {
            var stream = new MemoryStream();
            Serializer.Serialize(stream, item);
            stream.Position = 0;
            return stream;
        }).ToList();

        var sut = new XmlMultiStreamExtractor<PersonRecord>(streams, new XmlMultiStreamExtractorOptions { MaximumItemCount = 1 });

        var results = new List<PersonRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Single(results);

        // The stream that was read is disposed; the ones the enumeration never reached stay
        // caller-owned and open, as the class remarks document.
        Assert.Throws<ObjectDisposedException>(() => streams[0].ReadByte());
        Assert.True(streams[1].CanRead);
        Assert.True(streams[2].CanRead);
    }



    [Fact]
    public async Task ExtractAsync_when_XmlElement_attributes_maps_correctly()
    {
        var serializer = new XmlSerializer(typeof(XmlAttributePersonRecord));
        var stream = new MemoryStream();
        serializer.Serialize(stream, new XmlAttributePersonRecord { FirstName = "Alice", LastName = "Smith", Age = 30 });
        stream.Position = 0;

        var sut = new XmlMultiStreamExtractor<XmlAttributePersonRecord>
        (
            new[] { (Stream)stream }
        );

        var results = new List<XmlAttributePersonRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Single(results);
        Assert.Equal("Alice", results[0].FirstName);
        Assert.Equal("Smith", results[0].LastName);
        Assert.Equal(30, results[0].Age);
    }



    [Fact]
    public void Constructor_when_streams_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlMultiStreamExtractor<PersonRecord>(null!)
        );
    }



    [Fact]
    public async Task Constructor_Streams_Logger_when_logger_is_null_uses_NullLogger()
    {
        // logger is now an optional trailing parameter: null means "no logging"
        // (NullLogger.Instance) rather than an argument error.
        var sut = new XmlMultiStreamExtractor<PersonRecord>
        (
            CreateXmlStreams(1),
            logger: null
        );

        var extracted = await sut.ExtractAsync().ToListAsync();

        Assert.Single(extracted);
    }



    [Fact]
    public void Constructor_with_settings_when_streams_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlMultiStreamExtractor<PersonRecord>
            (
                null!,
                new XmlReaderSettings(),
                NullLogger<XmlMultiStreamExtractor<PersonRecord>>.Instance
            )
        );
    }



    [Fact]
    public void Constructor_with_settings_when_readerSettings_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlMultiStreamExtractor<PersonRecord>
            (
                Array.Empty<Stream>(),
                readerSettings: null!,
                NullLogger<XmlMultiStreamExtractor<PersonRecord>>.Instance
            )
        );
    }



    [Fact]
    public async Task Constructor_with_settings_when_logger_is_null_uses_NullLogger()
    {
        // logger is now an optional trailing parameter: null means "no logging"
        // (NullLogger.Instance) rather than an argument error.
        var sut = new XmlMultiStreamExtractor<PersonRecord>
        (
            CreateXmlStreams(1),
            new XmlReaderSettings(),
            logger: null
        );

        var extracted = await sut.ExtractAsync().ToListAsync();

        Assert.Single(extracted);
    }



    [Fact]
    public void Internal_constructor_when_streams_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlMultiStreamExtractor<PersonRecord>
            (
                null!,
                new XmlReaderSettings(),
                new ManualProgressTimer(),
                NullLogger<XmlMultiStreamExtractor<PersonRecord>>.Instance
            )
        );
    }



    [Fact]
    public void Internal_constructor_when_readerSettings_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlMultiStreamExtractor<PersonRecord>
            (
                Array.Empty<Stream>(),
                readerSettings: null!,
                new ManualProgressTimer(),
                NullLogger<XmlMultiStreamExtractor<PersonRecord>>.Instance
            )
        );
    }



    [Fact]
    public async Task Internal_constructor_when_logger_is_null_uses_NullLogger()
    {
        var sut = new XmlMultiStreamExtractor<PersonRecord>
        (
            CreateXmlStreams(1),
            new XmlReaderSettings(),
            new ManualProgressTimer(),
            logger: null
        );

        var extracted = 0;
        await foreach (var _ in sut.ExtractAsync())
        {
            extracted++;
        }

        // A full run with the NullLogger fallback extracts the item and counts it.
        Assert.Equal(1, extracted);
        Assert.Equal(1, sut.CurrentItemCount);
    }



    [Fact]
    public void Internal_constructor_when_timer_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlMultiStreamExtractor<PersonRecord>
            (
                Array.Empty<Stream>(),
                new XmlReaderSettings(),
                timer: null!,
                NullLogger<XmlMultiStreamExtractor<PersonRecord>>.Instance
            )
        );
    }
}
