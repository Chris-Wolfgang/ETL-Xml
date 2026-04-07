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
        return new XmlSingleStreamLoader<PersonRecord>(stream);
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
        var sut = new XmlSingleStreamLoader<PersonRecord>(stream);

        var items = new List<PersonRecord>
        {
            new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
            new() { FirstName = "Bob", LastName = "Jones", Age = 25 },
        };

        await sut.LoadAsync(items.ToAsyncEnumerable());

        stream.Position = 0;
        var content = Encoding.UTF8.GetString(stream.ToArray());

        Assert.Contains("ArrayOfPersonRecord", content, StringComparison.Ordinal);
        Assert.Contains("<FirstName>Alice</FirstName>", content, StringComparison.Ordinal);
        Assert.Contains("<FirstName>Bob</FirstName>", content, StringComparison.Ordinal);
    }



    [Fact]
    public async Task LoadAsync_when_empty_sequence_writes_empty_root_element()
    {
        var stream = new MemoryStream();
        var sut = new XmlSingleStreamLoader<PersonRecord>(stream);

        await sut.LoadAsync(AsyncEnumerable.Empty<PersonRecord>());

        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        Assert.Contains("ArrayOfPersonRecord", content, StringComparison.Ordinal);

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
        var loader = new XmlSingleStreamLoader<PersonRecord>(stream);

        var items = new List<PersonRecord>
        {
            new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        };

        await loader.LoadAsync(items.ToAsyncEnumerable());

        stream.Position = 0;
        var extractor = new XmlSingleStreamExtractor<PersonRecord>(stream);

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

        Assert.DoesNotContain("<?xml", content, StringComparison.Ordinal);
    }



    [Fact]
    public async Task LoadAsync_when_custom_XmlWriterSettings_does_not_mutate_caller_settings()
    {
        var stream = new MemoryStream();
        var settings = new XmlWriterSettings
        {
            Indent = false,
            CloseOutput = true,
            Async = false,
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

        Assert.True(settings.CloseOutput);
        Assert.False(settings.Async);
    }



    [Fact]
    public async Task LoadAsync_when_XmlElement_attributes_writes_mapped_names()
    {
        var stream = new MemoryStream();
        var sut = new XmlSingleStreamLoader<XmlAttributePersonRecord>(stream);

        var items = new List<XmlAttributePersonRecord>
        {
            new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        };

        await sut.LoadAsync(items.ToAsyncEnumerable());

        stream.Position = 0;
        var content = Encoding.UTF8.GetString(stream.ToArray());

        Assert.Contains("<first_name>Alice</first_name>", content, StringComparison.Ordinal);
        Assert.Contains("<last_name>Smith</last_name>", content, StringComparison.Ordinal);
        Assert.DoesNotContain("<FirstName>", content, StringComparison.Ordinal);
        Assert.DoesNotContain("<LastName>", content, StringComparison.Ordinal);
    }



    [Fact]
    public async Task LoadAsync_when_rootElementName_is_specified_uses_custom_root_element()
    {
        var stream = new MemoryStream();
        var sut = new XmlSingleStreamLoader<PersonRecord>(stream, new XmlSingleStreamLoaderOptions { RootElementName = "People" });

        var items = new List<PersonRecord>
        {
            new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        };

        await sut.LoadAsync(items.ToAsyncEnumerable());

        stream.Position = 0;
        var content = Encoding.UTF8.GetString(stream.ToArray());

        Assert.Contains("<People>", content, StringComparison.Ordinal);
        Assert.DoesNotContain("ArrayOfPersonRecord", content, StringComparison.Ordinal);
    }



    [Fact]
    public async Task LoadAsync_when_rootElementName_is_null_uses_default_root_element()
    {
        var stream = new MemoryStream();
        var sut = new XmlSingleStreamLoader<PersonRecord>(stream, new XmlSingleStreamLoaderOptions { RootElementName = null });

        var items = new List<PersonRecord>
        {
            new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        };

        await sut.LoadAsync(items.ToAsyncEnumerable());

        stream.Position = 0;
        var content = Encoding.UTF8.GetString(stream.ToArray());

        Assert.Contains("ArrayOfPersonRecord", content, StringComparison.Ordinal);
    }



    [Fact]
    public void Constructor_when_rootElementName_is_empty_throws_ArgumentException()
    {
        Assert.Throws<ArgumentException>
        (
            () => new XmlSingleStreamLoader<PersonRecord>(new MemoryStream(), new XmlSingleStreamLoaderOptions { RootElementName = "" })
        );
    }



    [Fact]
    public void Constructor_when_rootElementName_is_whitespace_throws_ArgumentException()
    {
        Assert.Throws<ArgumentException>
        (
            () => new XmlSingleStreamLoader<PersonRecord>(new MemoryStream(), new XmlSingleStreamLoaderOptions { RootElementName = "   " })
        );
    }



    [Fact]
    public async Task LoadAsync_when_leaveOpen_is_true_leaves_stream_open_after_loading()
    {
        var stream = new MemoryStream();
        var sut = new XmlSingleStreamLoader<PersonRecord>(stream, new XmlSingleStreamLoaderOptions { LeaveOpen = true });

        await sut.LoadAsync(AsyncEnumerable.Empty<PersonRecord>());

        Assert.True(stream.CanWrite);
    }



    [Fact]
    public async Task LoadAsync_when_leaveOpen_is_false_closes_stream_after_loading()
    {
        var stream = new MemoryStream();
        var sut = new XmlSingleStreamLoader<PersonRecord>(stream, new XmlSingleStreamLoaderOptions { LeaveOpen = false });

        await sut.LoadAsync(AsyncEnumerable.Empty<PersonRecord>());

        Assert.False(stream.CanWrite);
    }



    [Fact]
    public async Task LoadAsync_with_settings_when_rootElementName_is_specified_uses_custom_root_element()
    {
        var stream = new MemoryStream();
        var sut = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            new XmlWriterSettings { Indent = false },
            NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance,
            new XmlSingleStreamLoaderOptions { RootElementName = "People" }
        );

        var items = new List<PersonRecord>
        {
            new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        };

        await sut.LoadAsync(items.ToAsyncEnumerable());

        stream.Position = 0;
        var content = Encoding.UTF8.GetString(stream.ToArray());

        Assert.Contains("<People>", content, StringComparison.Ordinal);
        Assert.DoesNotContain("ArrayOfPersonRecord", content, StringComparison.Ordinal);
    }



    [Fact]
    public void Constructor_when_stream_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlSingleStreamLoader<PersonRecord>(null!)
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
    public async Task Internal_constructor_when_logger_is_null_uses_NullLogger()
    {
        var sut = new XmlSingleStreamLoader<PersonRecord>
        (
            new MemoryStream(),
            new XmlWriterSettings(),
            logger: null,
            new ManualProgressTimer()
        );

        await sut.LoadAsync(AsyncEnumerable.Empty<PersonRecord>());

        Assert.NotNull(sut);
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
