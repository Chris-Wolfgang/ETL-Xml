using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

public class XmlSingleStreamExtractorTests
    : ExtractorBaseContractTests
    <
        XmlSingleStreamExtractor<PersonRecord>,
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



    private static MemoryStream CreateXmlStream(int itemCount)
    {
        var stream = new MemoryStream();
        var writerSettings = new XmlWriterSettings { Indent = true, CloseOutput = false };
        using var writer = XmlWriter.Create(stream, writerSettings);
        var emptyNs = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") });

        writer.WriteStartDocument();
        writer.WriteStartElement("ArrayOfPersonRecord");

        foreach (var item in ExpectedItems.Take(itemCount))
        {
            Serializer.Serialize(writer, item, emptyNs);
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();

        stream.Position = 0;
        return stream;
    }



    protected override XmlSingleStreamExtractor<PersonRecord> CreateSut(int itemCount) =>
        new
        (
            CreateXmlStream(itemCount),
            NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance
        );



    protected override IReadOnlyList<PersonRecord> CreateExpectedItems() => ExpectedItems;



    protected override XmlSingleStreamExtractor<PersonRecord> CreateSutWithTimer
    (
        IProgressTimer timer
    ) =>
        new
        (
            CreateXmlStream(ExpectedItems.Count),
            new XmlReaderSettings(),
            NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance,
            timer
        );



    [Fact]
    public async Task ExtractAsync_when_null_element_deserialized_skips_it()
    {
        var xml = "<?xml version=\"1.0\"?><ArrayOfPersonRecord><PersonRecord><FirstName>Alice</FirstName><LastName>Smith</LastName><Age>30</Age></PersonRecord></ArrayOfPersonRecord>";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        var sut = new XmlSingleStreamExtractor<PersonRecord>
        (
            stream,
            NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance
        );

        var results = new List<PersonRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Single(results);
        Assert.Equal("Alice", results[0].FirstName);
    }



    [Fact]
    public async Task ExtractAsync_when_custom_XmlReaderSettings_uses_settings()
    {
        var stream = CreateXmlStream(2);

        var settings = new XmlReaderSettings
        {
            IgnoreWhitespace = true,
        };

        var sut = new XmlSingleStreamExtractor<PersonRecord>
        (
            stream,
            settings,
            NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance
        );

        var results = new List<PersonRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Equal(2, results.Count);
        Assert.Equal("Alice", results[0].FirstName);
        Assert.Equal("Bob", results[1].FirstName);
    }



    [Fact]
    public async Task ExtractAsync_when_custom_XmlReaderSettings_does_not_mutate_caller_settings()
    {
        var stream = CreateXmlStream(1);

        var settings = new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            CloseInput = true,
            Async = false,
        };

        var sut = new XmlSingleStreamExtractor<PersonRecord>
        (
            stream,
            settings,
            NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance
        );

        await foreach (var _ in sut.ExtractAsync())
        {
        }

        Assert.True(settings.CloseInput);
        Assert.False(settings.Async);
    }



    [Fact]
    public async Task ExtractAsync_when_XmlElement_attributes_maps_correctly()
    {
        var xml = "<?xml version=\"1.0\"?><ArrayOfperson><person><first_name>Alice</first_name><last_name>Smith</last_name><age>30</age></person></ArrayOfperson>";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        var sut = new XmlSingleStreamExtractor<XmlAttributePersonRecord>
        (
            stream,
            NullLogger<XmlSingleStreamExtractor<XmlAttributePersonRecord>>.Instance
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
    public void Constructor_when_stream_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlSingleStreamExtractor<PersonRecord>
            (
                null!,
                NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance
            )
        );
    }



    [Fact]
    public void Constructor_when_logger_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlSingleStreamExtractor<PersonRecord>
            (
                new MemoryStream(),
                logger: null!
            )
        );
    }



    [Fact]
    public void Constructor_with_settings_when_settings_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlSingleStreamExtractor<PersonRecord>
            (
                new MemoryStream(),
                readerSettings: null!,
                NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance
            )
        );
    }



    [Fact]
    public void Internal_constructor_when_stream_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlSingleStreamExtractor<PersonRecord>
            (
                null!,
                new XmlReaderSettings(),
                NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance,
                new ManualProgressTimer()
            )
        );
    }



    [Fact]
    public void Internal_constructor_when_logger_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlSingleStreamExtractor<PersonRecord>
            (
                new MemoryStream(),
                new XmlReaderSettings(),
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
            () => new XmlSingleStreamExtractor<PersonRecord>
            (
                new MemoryStream(),
                new XmlReaderSettings(),
                NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance,
                timer: null!
            )
        );
    }
}
