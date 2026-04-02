using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Wolfgang.Etl.Abstractions;
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

    private static readonly IReadOnlyList<PersonRecord> ExpectedItems = new List<PersonRecord>
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25 },
        new() { FirstName = "Carol", LastName = "White", Age = 35 },
        new() { FirstName = "Dave", LastName = "Brown", Age = 40 },
        new() { FirstName = "Eve", LastName = "Davis", Age = 28 },
    };



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



    protected override XmlMultiStreamExtractor<PersonRecord> CreateSut(int itemCount) =>
        new(CreateXmlStreams(itemCount));



    protected override IReadOnlyList<PersonRecord> CreateExpectedItems() => ExpectedItems;



    protected override XmlMultiStreamExtractor<PersonRecord> CreateSutWithTimer
    (
        IProgressTimer timer
    ) =>
        new
        (
            CreateXmlStreams(ExpectedItems.Count),
            NullLogger<XmlMultiStreamExtractor<PersonRecord>>.Instance,
            timer
        );



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
    public void Internal_constructor_when_streams_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlMultiStreamExtractor<PersonRecord>
            (
                null!,
                NullLogger<XmlMultiStreamExtractor<PersonRecord>>.Instance,
                new ManualProgressTimer()
            )
        );
    }



    [Fact]
    public void Internal_constructor_when_logger_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlMultiStreamExtractor<PersonRecord>
            (
                Array.Empty<Stream>(),
                logger: null!,
                new ManualProgressTimer()
            )
        );
    }



    [Fact]
    public void Internal_constructor_when_timer_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlMultiStreamExtractor<PersonRecord>
            (
                Array.Empty<Stream>(),
                NullLogger<XmlMultiStreamExtractor<PersonRecord>>.Instance,
                timer: null!
            )
        );
    }
}
