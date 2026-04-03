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

public class XmlMultiStreamLoaderTests
    : LoaderBaseContractTests
    <
        XmlMultiStreamLoader<PersonRecord>,
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



    protected override XmlMultiStreamLoader<PersonRecord> CreateSut(int itemCount) =>
        new(_ => new MemoryStream());



    protected override IReadOnlyList<PersonRecord> CreateSourceItems() => SourceItems;



    protected override XmlMultiStreamLoader<PersonRecord> CreateSutWithTimer
    (
        IProgressTimer timer
    ) =>
        new
        (
            _ => new MemoryStream(),
            new XmlWriterSettings(),
            NullLogger<XmlMultiStreamLoader<PersonRecord>>.Instance,
            timer
        );



    [Fact]
    public async Task LoadAsync_writes_one_xml_document_per_stream()
    {
        var buffers = new List<byte[]>();
        var sut = new XmlMultiStreamLoader<PersonRecord>
        (
            _ =>
            {
                var stream = new CapturingMemoryStream(buffers);
                return stream;
            }
        );

        var items = new List<PersonRecord>
        {
            new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
            new() { FirstName = "Bob", LastName = "Jones", Age = 25 },
        };

        await sut.LoadAsync(items.ToAsyncEnumerable());

        Assert.Equal(2, buffers.Count);

        var serializer = new XmlSerializer(typeof(PersonRecord));

        var person1 = (PersonRecord?)serializer.Deserialize(new MemoryStream(buffers[0]));
        Assert.NotNull(person1);
        Assert.Equal("Alice", person1.FirstName);

        var person2 = (PersonRecord?)serializer.Deserialize(new MemoryStream(buffers[1]));
        Assert.NotNull(person2);
        Assert.Equal("Bob", person2.FirstName);
    }



    [Fact]
    public Task LoadAsync_when_stream_factory_returns_null_throws_InvalidOperationException()
    {
        var sut = new XmlMultiStreamLoader<PersonRecord>(_ => null!);

        var items = new List<PersonRecord>
        {
            new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        };

        return Assert.ThrowsAsync<InvalidOperationException>
        (
            () => sut.LoadAsync(items.ToAsyncEnumerable())
        );
    }



    [Fact]
    public async Task LoadAsync_disposes_streams_after_writing()
    {
        var streams = new List<MemoryStream>();
        var sut = new XmlMultiStreamLoader<PersonRecord>
        (
            _ =>
            {
                var stream = new MemoryStream();
                streams.Add(stream);
                return stream;
            }
        );

        var items = new List<PersonRecord>
        {
            new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        };

        await sut.LoadAsync(items.ToAsyncEnumerable());

        Assert.Single(streams);
        Assert.Throws<ObjectDisposedException>(() => streams[0].ReadByte());
    }



    [Fact]
    public async Task LoadAsync_when_empty_sequence_creates_no_streams()
    {
        var streamCount = 0;
        var sut = new XmlMultiStreamLoader<PersonRecord>
        (
            _ =>
            {
                streamCount++;
                return new MemoryStream();
            }
        );

        await sut.LoadAsync(AsyncEnumerable.Empty<PersonRecord>());

        Assert.Equal(0, streamCount);
    }



    [Fact]
    public async Task LoadAsync_when_XmlElement_attributes_writes_mapped_names()
    {
        var buffers = new List<byte[]>();
        var sut = new XmlMultiStreamLoader<XmlAttributePersonRecord>
        (
            _ => new CapturingMemoryStream(buffers)
        );

        var items = new List<XmlAttributePersonRecord>
        {
            new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        };

        await sut.LoadAsync(items.ToAsyncEnumerable());

        Assert.Single(buffers);
        var content = Encoding.UTF8.GetString(buffers[0]);

        Assert.Contains("<first_name>Alice</first_name>", content, StringComparison.Ordinal);
        Assert.Contains("<last_name>Smith</last_name>", content, StringComparison.Ordinal);
        Assert.DoesNotContain("<FirstName>", content, StringComparison.Ordinal);
    }



    [Fact]
    public async Task LoadAsync_round_trips_correctly()
    {
        var buffers = new List<byte[]>();
        var loader = new XmlMultiStreamLoader<PersonRecord>
        (
            _ => new CapturingMemoryStream(buffers)
        );

        var items = new List<PersonRecord>
        {
            new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        };

        await loader.LoadAsync(items.ToAsyncEnumerable());

        Assert.Single(buffers);

        var extractor = new XmlMultiStreamExtractor<PersonRecord>
        (
            new[] { (Stream)new MemoryStream(buffers[0]) }
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
    public void Constructor_when_streamFactory_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlMultiStreamLoader<PersonRecord>(null!)
        );
    }



    [Fact]
    public void Constructor_with_settings_when_settings_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlMultiStreamLoader<PersonRecord>
            (
                _ => new MemoryStream(),
                writerSettings: null!,
                NullLogger<XmlMultiStreamLoader<PersonRecord>>.Instance
            )
        );
    }



    [Fact]
    public void Internal_constructor_when_streamFactory_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new XmlMultiStreamLoader<PersonRecord>
            (
                null!,
                new XmlWriterSettings(),
                NullLogger<XmlMultiStreamLoader<PersonRecord>>.Instance,
                new ManualProgressTimer()
            )
        );
    }



    [Fact]
    public async Task Internal_constructor_when_logger_is_null_uses_NullLogger()
    {
        var sut = new XmlMultiStreamLoader<PersonRecord>
        (
            _ => new MemoryStream(),
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
            () => new XmlMultiStreamLoader<PersonRecord>
            (
                _ => new MemoryStream(),
                new XmlWriterSettings(),
                NullLogger<XmlMultiStreamLoader<PersonRecord>>.Instance,
                timer: null!
            )
        );
    }
}
