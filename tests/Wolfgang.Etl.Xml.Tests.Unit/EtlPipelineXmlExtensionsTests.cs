using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
    public async Task RunAsync_when_single_stream_source_and_sink_round_trips_the_records()
    {
        // Arrange: a single-root XML source stream and an in-memory destination.
        var source = await CreateSingleStreamXmlAsync(People);
        var destination = new MemoryStream();

        // Act: XmlSingleStreamExtractor -> XmlSingleStreamLoader through the fluent EtlPipeline.
        await EtlPipeline
            .Create()
            .XmlSingleStreamExtractor<PersonRecord>(source)
            .XmlSingleStreamLoader(destination)
            .RunAsync();

        // Assert: reading the destination back yields the same records.
        var readBack = await ReadSingleStreamXmlAsync(destination);

        Assert.Equal(People, readBack);
    }



    [Fact]
    public async Task RunAsync_when_single_stream_file_paths_round_trips_and_disposes_the_files()
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
                .XmlSingleStreamLoader(outPath)
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
    public async Task XmlSingleStreamLoader_path_when_IsDryRun_leaves_an_existing_file_unchanged()
    {
        var outPath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(outPath, "<precious/>");
            var source = await CreateSingleStreamXmlAsync(People);

            await EtlPipeline
                .Create()
                .XmlSingleStreamExtractor<PersonRecord>(source)
                .XmlSingleStreamLoader(outPath, new XmlSingleStreamLoaderOptions { IsDryRun = true })
                .RunAsync();

            Assert.Equal
            (
                "<precious/>",
                File.ReadAllText(outPath)
            );
        }
        finally
        {
            File.Delete(outPath);
        }
    }



    [Fact]
    public async Task XmlSingleStreamLoader_path_when_IsDryRun_does_not_create_a_missing_file()
    {
        var outPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");
        var source = await CreateSingleStreamXmlAsync(People);

        await EtlPipeline
            .Create()
            .XmlSingleStreamExtractor<PersonRecord>(source)
            .XmlSingleStreamLoader(outPath, new XmlSingleStreamLoaderOptions { IsDryRun = true })
            .RunAsync();

        Assert.False(File.Exists(outPath));
    }



    [Fact]
    public void XmlSingleStreamLoader_path_when_composed_but_never_run_leaves_an_existing_file_unchanged()
    {
        var outPath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(outPath, "<precious/>");

            _ = EtlPipeline
                .Create()
                .From(new List<PersonRecord>().ToAsyncEnumerable())
                .XmlSingleStreamLoader(outPath);

            Assert.Equal("<precious/>", File.ReadAllText(outPath));
        }
        finally
        {
            File.Delete(outPath);
        }
    }



    [Fact]
    public void XmlSingleStreamExtractor_path_when_composed_but_never_run_does_not_lock_the_file()
    {
        var inPath = Path.GetTempFileName();
        try
        {
            _ = EtlPipeline.Create().XmlSingleStreamExtractor<PersonRecord>(inPath);

            using (File.Open(inPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
            }
        }
        finally
        {
            File.Delete(inPath);
        }
    }



    [Fact]
    public async Task XmlSingleStreamExtractor_path_when_run_with_a_pre_cancelled_token_does_not_lock_the_file()
    {
        var inPath = Path.GetTempFileName();
        try
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>
            (
                () => EtlPipeline
                    .Create()
                    .XmlSingleStreamExtractor<PersonRecord>(inPath)
                    .XmlSingleStreamLoader(new MemoryStream())
                    .RunAsync(null, cts.Token)
            );

            using (File.Open(inPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
            }
        }
        finally
        {
            File.Delete(inPath);
        }
    }



    [Fact]
    public async Task XmlSingleStreamExtractor_path_when_file_is_missing_fails_at_run_time()
    {
        var inPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");

        var pipeline = EtlPipeline
            .Create()
            .XmlSingleStreamExtractor<PersonRecord>(inPath)
            .XmlSingleStreamLoader(new MemoryStream());

        await Assert.ThrowsAsync<FileNotFoundException>(() => pipeline.RunAsync());
    }



    [Fact]
    public void DeferredFileStream_exposes_forward_only_capabilities_and_rejects_seeking()
    {
        var path = Path.GetTempFileName();
        try
        {
            using var reader = DeferredFileStream.ForRead(path);
            using var writer = DeferredFileStream.ForWrite(path);

            Assert.True(reader.CanRead);
            Assert.False(reader.CanWrite);
            Assert.True(writer.CanWrite);
            Assert.False(writer.CanRead);
            Assert.False(reader.CanSeek);
            Assert.Throws<NotSupportedException>(() => reader.Length);
            Assert.Throws<NotSupportedException>(() => reader.Position);
            Assert.Throws<NotSupportedException>(() => reader.Position = 0);
            Assert.Throws<NotSupportedException>(() => reader.Seek(0, SeekOrigin.Begin));
            Assert.Throws<NotSupportedException>(() => reader.SetLength(0));
        }
        finally
        {
            File.Delete(path);
        }
    }



    [Fact]
    public async Task DeferredFileStream_reads_and_writes_synchronously_and_asynchronously()
    {
        var path = Path.GetTempFileName();
        try
        {
            var data = new byte[] { 1, 2, 3, 4 };

            var writer = DeferredFileStream.ForWrite(path);
            writer.Write(data, 0, 2);
            await writer.WriteAsync(data, 2, 2, CancellationToken.None);
            writer.Flush();
            await writer.FlushAsync(CancellationToken.None);
            writer.Dispose();
            Assert.False(writer.CanWrite);
            Assert.Throws<ObjectDisposedException>(() => writer.Write(data, 0, 1));

            var reader = DeferredFileStream.ForRead(path);
            var buffer = new byte[4];
            Assert.Equal(2, reader.Read(buffer, 0, 2));
            Assert.Equal(2, await reader.ReadAsync(buffer, 2, 2, CancellationToken.None));
            Assert.Equal(data, buffer);
            reader.Dispose();
        }
        finally
        {
            File.Delete(path);
        }
    }



    [Fact]
    public async Task DeferredFileStream_flush_before_first_use_does_not_open_the_file()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");

        using var writer = DeferredFileStream.ForWrite(path);
        writer.Flush();
        await writer.FlushAsync(CancellationToken.None);

        Assert.False(File.Exists(path));
    }



    [Fact]
    public void XmlSingleStreamLoader_path_when_options_are_invalid_throws_and_releases_the_file()
    {
        var outPath = Path.GetTempFileName();
        try
        {
            var options = new XmlSingleStreamLoaderOptions { RootElementName = "" };

            Assert.Throws<ArgumentException>
            (
                () => EtlPipeline
                    .Create()
                    .From(new List<PersonRecord>().ToAsyncEnumerable())
                    .XmlSingleStreamLoader(outPath, options)
            );

            // An undisposed FileStream would hold the handle and make this exclusive open throw IOException.
            using (File.Open(outPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
            }
        }
        finally
        {
            File.Delete(outPath);
        }
    }



    [Fact]
    public async Task RunAsync_when_multi_stream_source_feeds_single_stream_sink_round_trips_the_records()
    {
        // One single-document XML stream per record feeding a single-root XML sink.
        var sources = People.Select(CreateSingleRecordXml).Cast<Stream>().ToList();
        var destination = new MemoryStream();

        await EtlPipeline
            .Create()
            .XmlMultiStreamExtractor<PersonRecord>(sources)
            .XmlSingleStreamLoader(destination)
            .RunAsync();

        var readBack = await ReadSingleStreamXmlAsync(destination);

        Assert.Equal(People, readBack);
    }



    [Fact]
    public async Task RunAsync_when_single_stream_source_feeds_multi_stream_sink_writes_one_document_per_record()
    {
        // A single-root XML source fanned out to one document per record.
        var source = await CreateSingleStreamXmlAsync(People);
        var captured = new List<byte[]>();

        await EtlPipeline
            .Create()
            .XmlSingleStreamExtractor<PersonRecord>(source)
            .XmlMultiStreamLoader(_ => new CapturingMemoryStream(captured))
            .RunAsync();

        // One captured document per record, each deserializing back to the original.
        Assert.Equal(People.Length, captured.Count);

        var readBack = captured
            .Select(bytes => (PersonRecord)PersonSerializer.Deserialize(new MemoryStream(bytes))!)
            .ToList();

        Assert.Equal(People, readBack);
    }



    [Fact]
    public async Task RunAsync_when_Through_filters_between_xml_source_and_sink_loads_only_the_kept_records()
    {
        var source = await CreateSingleStreamXmlAsync(People);
        var destination = new MemoryStream();

        // Keep only people aged 30+ via a stream-to-stream Through stage.
        await EtlPipeline
            .Create()
            .XmlSingleStreamExtractor<PersonRecord>(source)
            .Through<PersonRecord>(items => Filter(items, p => p.Age >= 30))
            .XmlSingleStreamLoader(destination)
            .RunAsync();

        var readBack = await ReadSingleStreamXmlAsync(destination);

        Assert.Equal
        (
            new[] { "Alice", "Carol" },
            readBack.Select(p => p.FirstName).ToArray()
        );
    }



    [Fact]
    public void XmlSingleStreamExtractor_when_pipeline_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => ((EtlPipeline)null!).XmlSingleStreamExtractor<PersonRecord>(new MemoryStream())
        );
    }



    [Fact]
    public void XmlSingleStreamExtractor_when_path_is_null_throws_ArgumentNullException()
    {
        string path = null!;

        Assert.Throws<ArgumentNullException>
        (
            () => EtlPipeline.Create().XmlSingleStreamExtractor<PersonRecord>(path)
        );
    }



    [Fact]
    public void XmlMultiStreamExtractor_when_streams_is_null_throws_ArgumentNullException()
    {
        IEnumerable<Stream> streams = null!;

        Assert.Throws<ArgumentNullException>
        (
            () => EtlPipeline.Create().XmlMultiStreamExtractor<PersonRecord>(streams)
        );
    }



    [Fact]
    public void XmlSingleStreamLoader_when_path_is_null_throws_ArgumentNullException()
    {
        var pipeline = EtlPipeline.Create().XmlSingleStreamExtractor<PersonRecord>(new MemoryStream());

        Assert.Throws<ArgumentNullException>
        (
            () => pipeline.XmlSingleStreamLoader((string)null!)
        );
    }



    [Fact]
    public void XmlMultiStreamLoader_when_streamFactory_is_null_throws_ArgumentNullException()
    {
        var pipeline = EtlPipeline.Create().XmlSingleStreamExtractor<PersonRecord>(new MemoryStream());
        Func<PersonRecord, Stream> streamFactory = null!;

        Assert.Throws<ArgumentNullException>
        (
            () => pipeline.XmlMultiStreamLoader(streamFactory)
        );
    }



    [Fact]
    public void XmlSingleStreamExtractor_path_overload_when_pipeline_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => ((EtlPipeline)null!).XmlSingleStreamExtractor<PersonRecord>("people.xml")
        );
    }



    [Fact]
    public void XmlSingleStreamExtractor_when_stream_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => EtlPipeline.Create().XmlSingleStreamExtractor<PersonRecord>((Stream)null!)
        );
    }



    [Fact]
    public void XmlMultiStreamExtractor_when_pipeline_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => ((EtlPipeline)null!).XmlMultiStreamExtractor<PersonRecord>(new List<Stream>())
        );
    }



    [Fact]
    public void XmlSingleStreamLoader_path_overload_when_pipeline_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => ((IEtlPipeline<PersonRecord>)null!).XmlSingleStreamLoader("people.xml")
        );
    }



    [Fact]
    public void XmlSingleStreamLoader_stream_overload_when_pipeline_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => ((IEtlPipeline<PersonRecord>)null!).XmlSingleStreamLoader(new MemoryStream())
        );
    }



    [Fact]
    public void XmlSingleStreamLoader_when_stream_is_null_throws_ArgumentNullException()
    {
        var pipeline = EtlPipeline.Create().XmlSingleStreamExtractor<PersonRecord>(new MemoryStream());

        Assert.Throws<ArgumentNullException>
        (
            () => pipeline.XmlSingleStreamLoader((Stream)null!)
        );
    }



    [Fact]
    public void XmlMultiStreamLoader_when_pipeline_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => ((IEtlPipeline<PersonRecord>)null!).XmlMultiStreamLoader(_ => new MemoryStream())
        );
    }



    private static async Task<MemoryStream> CreateSingleStreamXmlAsync(IEnumerable<PersonRecord> people)
    {
        var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>(stream);
        await loader.LoadAsync(people.ToAsyncEnumerable());
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
