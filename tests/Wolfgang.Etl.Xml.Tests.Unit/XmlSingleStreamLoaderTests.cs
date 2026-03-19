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

public class XmlSingleStreamLoaderTests
    : LoaderBaseContractTests
    <
        XmlSingleStreamLoader<PersonRecord>,
        PersonRecord,
        XmlReport
    >
{
    private static readonly IReadOnlyList<PersonRecord> SourceItems = new List<PersonRecord>
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25 },
        new() { FirstName = "Carol", LastName = "White", Age = 35 },
        new() { FirstName = "Dave", LastName = "Brown", Age = 40 },
        new() { FirstName = "Eve", LastName = "Davis", Age = 28 },
    };



    protected override XmlSingleStreamLoader<PersonRecord> CreateSut(int itemCount)
    {
        var stream = new MemoryStream();
        return new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance
        );
    }



    protected override IReadOnlyList<PersonRecord> CreateSourceItems() => SourceItems;



    protected override XmlSingleStreamLoader<PersonRecord> CreateSutWithTimer
    (
        IProgressTimer timer
    )
    {
        var stream = new MemoryStream();
        return new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            new XmlWriterSettings(),
            NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance,
            timer
        );
    }



    [Fact]
    public async Task LoadAsync_writes_valid_xml_with_root_element()
    {
        var stream = new MemoryStream();
        var sut = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance
        );

        var items = new List<PersonRecord>
        {
            new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
            new() { FirstName = "Bob", LastName = "Jones", Age = 25 },
        };

        await sut.LoadAsync(items.ToAsyncEnumerable());

        stream.Position = 0;
        var content = Encoding.UTF8.GetString(stream.ToArray());

        Assert.Contains("ArrayOfPersonRecord", content);
        Assert.Contains("<FirstName>Alice</FirstName>", content);
        Assert.Contains("<FirstName>Bob</FirstName>", content);
    }



    [Fact]
    public async Task LoadAsync_when_empty_sequence_writes_empty_root_element()
    {
        var stream = new MemoryStream();
        var sut = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance
        );

        await sut.LoadAsync(AsyncEnumerable.Empty<PersonRecord>());

        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        Assert.Contains("ArrayOfPersonRecord", content);

        // Should be valid XML — parse it to verify
        var settings = new XmlReaderSettings { Async = true };
        using var xmlReader = XmlReader.Create(new StringReader(content), settings);
        while (await xmlReader.ReadAsync())
        {
            if (xmlReader.NodeType == XmlNodeType.Element)
            {
                Assert.Equal("ArrayOfPersonRecord", xmlReader.LocalName);
                return;
            }
        }

        Assert.Fail("No element found in output XML.");
    }



    [Fact]
    public async Task LoadAsync_round_trips_correctly()
    {
        var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance
        );

        var items = new List<PersonRecord>
        {
            new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        };

        await loader.LoadAsync(items.ToAsyncEnumerable());

        stream.Position = 0;
        var extractor = new XmlSingleStreamExtractor<PersonRecord>
        (
            stream,
            NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance
        );

        var results = new List<PersonRecord>();
        await foreach (var item in extractor.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Single(results);
        Assert.Equal("Alice", results[0].FirstName);
        Assert.Equal("Smith", results[0].LastName);
        Assert.Equal(30, results[0].Age);
    }



    [Fact]
    public async Task LoadAsync_when_custom_XmlWriterSettings_uses_settings()
    {
        var stream = new MemoryStream();
        var settings = new XmlWriterSettings
        {
            Indent = false,
            OmitXmlDeclaration = true,
        };

        var sut = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            settings,
            NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance
        );

        var items = new List<PersonRecord>
        {
            new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        };

        await sut.LoadAsync(items.ToAsyncEnumerable());

        stream.Position = 0;
        var content = Encoding.UTF8.GetString(stream.ToArray());

        Assert.DoesNotContain("<?xml", content);
    }



    [Fact]
    public async Task LoadAsync_when_XmlElement_attributes_writes_mapped_names()
    {
        var stream = new MemoryStream();
        var sut = new XmlSingleStreamLoader<XmlAttributePersonRecord>
        (
            stream,
            NullLogger<XmlSingleStreamLoader<XmlAttributePersonRecord>>.Instance
        );

        var items = new List<XmlAttributePersonRecord>
        {
            new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        };

        await sut.LoadAsync(items.ToAsyncEnumerable());

        stream.Position = 0;
        var content = Encoding.UTF8.GetString(stream.ToArray());

        Assert.Contains("<first_name>Alice</first_name>", content);
        Assert.Contains("<last_name>Smith</last_name>", content);
        Assert.DoesNotContain("<FirstName>", content);
        Assert.DoesNotContain("<LastName>", content);
    }



    [Fact]
    public void Constructor_when_stream_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlSingleStreamLoader<PersonRecord>
            (
                null!,
                NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance
            )
        );
    }



    [Fact]
    public void Constructor_when_logger_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlSingleStreamLoader<PersonRecord>
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
            () => new XmlSingleStreamLoader<PersonRecord>
            (
                new MemoryStream(),
                writerSettings: null!,
                NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance
            )
        );
    }



    [Fact]
    public void Internal_constructor_when_stream_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlSingleStreamLoader<PersonRecord>
            (
                null!,
                new XmlWriterSettings(),
                NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance,
                new ManualProgressTimer()
            )
        );
    }



    [Fact]
    public void Internal_constructor_when_logger_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlSingleStreamLoader<PersonRecord>
            (
                new MemoryStream(),
                new XmlWriterSettings(),
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
            () => new XmlSingleStreamLoader<PersonRecord>
            (
                new MemoryStream(),
                new XmlWriterSettings(),
                NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance,
                timer: null!
            )
        );
    }
}
