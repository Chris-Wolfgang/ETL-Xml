using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

public sealed class EtlPipelineXmlExtensionsTests
{
    private static readonly XmlSerializer PersonSerializer = new(typeof(PersonRecord));

    private static readonly PersonRecord[] People =
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25 },
        new() { FirstName = "Carol", LastName = "White", Age = 35 },
    };



    [Fact]
    public async Task SingleStream_stream_round_trip_via_pipeline()
    {
        // Arrange: a single-root XML source stream and an in-memory destination.
        var source = await CreateSingleStreamXmlAsync(People);
        var destination = new MemoryStream();

        // Act: XmlSingleStreamExtractor -> XmlSingleStreamLoader through the fluent EtlPipeline.
        await EtlPipeline
            .Create()
            .XmlSingleStreamExtractor<PersonRecord>(source)
            .XmlSingleStreamLoader<PersonRecord>(destination)
            .RunAsync();

        // Assert: reading the destination back yields the same records.
        var readBack = await ReadSingleStreamXmlAsync(destination);

        Assert.Equal(People, readBack);
    }



    [Fact]
    public async Task SingleStream_file_round_trip_via_pipeline_disposes_streams()
    {
        var inPath = Path.GetTempFileName();
        var outPath = Path.GetTempFileName();
        try
        {
            using (var seed = await CreateSingleStreamXmlAsync(People))
            using (var inFile = File.Create(inPath))
            {
                await seed.CopyToAsync(inFile);
            }

            await EtlPipeline
                .Create()
                .XmlSingleStreamExtractor<PersonRecord>(inPath)
                .XmlSingleStreamLoader<PersonRecord>(outPath)
                .RunAsync();

            // If the factory-owned streams were not closed, these opens would throw IOException.
            List<PersonRecord> readBack;
            using (var outStream = File.OpenRead(outPath))
            {
                readBack = await ReadSingleStreamXmlAsync(outStream);
            }

            Assert.Equal(People, readBack);
        }
        finally
        {
            File.Delete(inPath);
            File.Delete(outPath);
        }
    }



    [Fact]
    public async Task MultiStream_source_to_single_stream_sink_via_pipeline()
    {
        // One single-document XML stream per record feeding a single-root XML sink.
        var sources = People.Select(CreateSingleRecordXml).Cast<Stream>().ToList();
        var destination = new MemoryStream();

        await EtlPipeline
            .Create()
            .XmlMultiStreamExtractor<PersonRecord>(sources)
            .XmlSingleStreamLoader<PersonRecord>(destination)
            .RunAsync();

        var readBack = await ReadSingleStreamXmlAsync(destination);

        Assert.Equal(People, readBack);
    }



    [Fact]
    public async Task SingleStream_source_to_multi_stream_sink_via_pipeline()
    {
        // A single-root XML source fanned out to one document per record.
        var source = await CreateSingleStreamXmlAsync(People);
        var captured = new List<byte[]>();

        await EtlPipeline
            .Create()
            .XmlSingleStreamExtractor<PersonRecord>(source)
            .XmlMultiStreamLoader<PersonRecord>(_ => new CapturingMemoryStream(captured))
            .RunAsync();

        // One captured document per record, each deserializing back to the original.
        Assert.Equal(People.Length, captured.Count);

        var readBack = captured
            .Select(bytes => (PersonRecord)PersonSerializer.Deserialize(new MemoryStream(bytes))!)
            .ToList();

        Assert.Equal(People, readBack);
    }



    [Fact]
    public async Task Through_operator_applies_between_xml_source_and_sink()
    {
        var source = await CreateSingleStreamXmlAsync(People);
        var destination = new MemoryStream();

        // Keep only people aged 30+ via a stream-to-stream Through stage.
        await EtlPipeline
            .Create()
            .XmlSingleStreamExtractor<PersonRecord>(source)
            .Through<PersonRecord>(items => Filter(items, p => p.Age >= 30))
            .XmlSingleStreamLoader<PersonRecord>(destination)
            .RunAsync();

        var readBack = await ReadSingleStreamXmlAsync(destination);

        Assert.Equal
        (
            new[] { "Alice", "Carol" },
            readBack.Select(p => p.FirstName).ToArray()
        );
    }



    [Fact]
    public void XmlSingleStreamExtractor_null_pipeline_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => ((EtlPipeline)null!).XmlSingleStreamExtractor<PersonRecord>(new MemoryStream())
        );
    }



    [Fact]
    public void XmlSingleStreamExtractor_null_path_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => EtlPipeline.Create().XmlSingleStreamExtractor<PersonRecord>((string)null!)
        );
    }



    [Fact]
    public void XmlMultiStreamExtractor_null_streams_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => EtlPipeline.Create().XmlMultiStreamExtractor<PersonRecord>((IEnumerable<Stream>)null!)
        );
    }



    [Fact]
    public void XmlSingleStreamLoader_null_path_throws()
    {
        var pipeline = EtlPipeline.Create().XmlSingleStreamExtractor<PersonRecord>(new MemoryStream());

        Assert.Throws<ArgumentNullException>
        (
            () => pipeline.XmlSingleStreamLoader<PersonRecord>((string)null!)
        );
    }



    [Fact]
    public void XmlMultiStreamLoader_null_factory_throws()
    {
        var pipeline = EtlPipeline.Create().XmlSingleStreamExtractor<PersonRecord>(new MemoryStream());

        Assert.Throws<ArgumentNullException>
        (
            () => pipeline.XmlMultiStreamLoader<PersonRecord>((Func<PersonRecord, Stream>)null!)
        );
    }



    private static async Task<MemoryStream> CreateSingleStreamXmlAsync(IEnumerable<PersonRecord> people)
    {
        var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>(stream);
        await loader.LoadAsync(ToAsync(people));
        stream.Position = 0;
        return stream;
    }



    private static MemoryStream CreateSingleRecordXml(PersonRecord person)
    {
        var stream = new MemoryStream();
        PersonSerializer.Serialize(stream, person);
        stream.Position = 0;
        return stream;
    }



    private static async Task<List<PersonRecord>> ReadSingleStreamXmlAsync(Stream stream)
    {
        stream.Position = 0;
        var results = new List<PersonRecord>();
        await foreach (var p in new XmlSingleStreamExtractor<PersonRecord>(stream).ExtractAsync())
        {
            results.Add(p);
        }

        return results;
    }



    private static async IAsyncEnumerable<T> ToAsync<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask;
    }



    private static async IAsyncEnumerable<T> Filter<T>(IAsyncEnumerable<T> items, Func<T, bool> predicate)
    {
        await foreach (var item in items)
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }
}
