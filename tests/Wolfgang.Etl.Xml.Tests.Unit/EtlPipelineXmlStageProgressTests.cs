using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

public sealed class EtlPipelineXmlStageProgressTests
{
    private static readonly XmlSerializer PersonSerializer = new(typeof(PersonRecord));

    private static readonly PersonRecord[] People =
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25 },
        new() { FirstName = "Carol", LastName = "White", Age = 35 },
    };



    [Fact]
    public async Task RunAsync_when_single_stream_source_and_sink_take_progress_reports_each_stage()
    {
        var source = await CreateSingleStreamXmlAsync();
        var destination = new MemoryStream();
        var extractReports = new List<XmlReport>();
        var loadReports = new List<XmlReport>();

        await EtlPipeline
            .Create()
            .XmlSingleStreamExtractor<PersonRecord>(source, new SynchronousProgress<XmlReport>(extractReports.Add))
            .XmlSingleStreamLoader(destination, new SynchronousProgress<XmlReport>(loadReports.Add))
            .RunAsync();

        Assert.Equal(People.Length, extractReports[extractReports.Count - 1].CurrentItemCount);
        Assert.Equal(People.Length, loadReports[loadReports.Count - 1].CurrentItemCount);
    }



    [Fact]
    public async Task RunAsync_when_file_paths_take_progress_reports_each_stage_and_round_trips()
    {
        var inPath = Path.GetTempFileName();
        var outPath = Path.GetTempFileName();
        try
        {
            using (var seed = await CreateSingleStreamXmlAsync())
            using (var inFile = File.Create(inPath))
            {
                await seed.CopyToAsync(inFile);
            }

            var extractReports = new List<XmlReport>();
            var loadReports = new List<XmlReport>();

            await EtlPipeline
                .Create()
                .XmlSingleStreamExtractor<PersonRecord>(inPath, new SynchronousProgress<XmlReport>(extractReports.Add))
                .XmlSingleStreamLoader(outPath, new SynchronousProgress<XmlReport>(loadReports.Add))
                .RunAsync();

            Assert.Equal(People.Length, extractReports[extractReports.Count - 1].CurrentItemCount);
            Assert.Equal(People.Length, loadReports[loadReports.Count - 1].CurrentItemCount);

            // If the factory-owned streams were not closed, this open would throw IOException.
            using (var outStream = File.OpenRead(outPath))
            {
                Assert.True(outStream.Length > 0);
            }
        }
        finally
        {
            File.Delete(inPath);
            File.Delete(outPath);
        }
    }



    [Fact]
    public async Task RunAsync_when_multi_stream_source_and_sink_take_progress_reports_each_stage()
    {
        var sources = People.Select(CreateRecordStream).ToList();
        var written = new List<MemoryStream>();
        var extractReports = new List<XmlReport>();
        var loadReports = new List<XmlReport>();

        await EtlPipeline
            .Create()
            .XmlMultiStreamExtractor<PersonRecord>(sources, new SynchronousProgress<XmlReport>(extractReports.Add))
            .XmlMultiStreamLoader
            (
                _ =>
                {
                    var stream = new MemoryStream();
                    written.Add(stream);
                    return stream;
                },
                new SynchronousProgress<XmlReport>(loadReports.Add)
            )
            .RunAsync();

        Assert.Equal(People.Length, extractReports[extractReports.Count - 1].CurrentItemCount);
        Assert.Equal(People.Length, loadReports[loadReports.Count - 1].CurrentItemCount);
        Assert.Equal(People.Length, written.Count);
    }



    [Fact]
    public void Source_progress_overloads_when_an_argument_is_null_throw_ArgumentNullException()
    {
        var progress = new SynchronousProgress<XmlReport>(_ => { });
        var pipeline = EtlPipeline.Create();
        string path = null!;
        Stream stream = null!;
        IEnumerable<Stream> streams = null!;
        IProgress<XmlReport> noProgress = null!;

        Assert.Throws<ArgumentNullException>(() => ((EtlPipeline)null!).XmlSingleStreamExtractor<PersonRecord>("a.xml", progress));
        Assert.Throws<ArgumentNullException>(() => pipeline.XmlSingleStreamExtractor<PersonRecord>(path, progress));
        Assert.Throws<ArgumentNullException>(() => pipeline.XmlSingleStreamExtractor<PersonRecord>("a.xml", noProgress));

        Assert.Throws<ArgumentNullException>(() => ((EtlPipeline)null!).XmlSingleStreamExtractor<PersonRecord>(new MemoryStream(), progress));
        Assert.Throws<ArgumentNullException>(() => pipeline.XmlSingleStreamExtractor<PersonRecord>(stream, progress));
        Assert.Throws<ArgumentNullException>(() => pipeline.XmlSingleStreamExtractor<PersonRecord>(new MemoryStream(), noProgress));

        Assert.Throws<ArgumentNullException>(() => ((EtlPipeline)null!).XmlMultiStreamExtractor<PersonRecord>(new List<Stream>(), progress));
        Assert.Throws<ArgumentNullException>(() => pipeline.XmlMultiStreamExtractor<PersonRecord>(streams, progress));
        Assert.Throws<ArgumentNullException>(() => pipeline.XmlMultiStreamExtractor<PersonRecord>(new List<Stream>(), noProgress));
    }



    [Fact]
    public void Sink_progress_overloads_when_an_argument_is_null_throw_ArgumentNullException()
    {
        var progress = new SynchronousProgress<XmlReport>(_ => { });
        var pipeline = EtlPipeline.Create().From(People.ToAsyncEnumerable());
        string path = null!;
        Stream stream = null!;
        Func<PersonRecord, Stream> factory = null!;
        IProgress<XmlReport> noProgress = null!;

        Assert.Throws<ArgumentNullException>(() => ((IEtlPipeline<PersonRecord>)null!).XmlSingleStreamLoader("a.xml", progress));
        Assert.Throws<ArgumentNullException>(() => pipeline.XmlSingleStreamLoader(path, progress));
        Assert.Throws<ArgumentNullException>(() => pipeline.XmlSingleStreamLoader("a.xml", noProgress));

        Assert.Throws<ArgumentNullException>(() => ((IEtlPipeline<PersonRecord>)null!).XmlSingleStreamLoader(new MemoryStream(), progress));
        Assert.Throws<ArgumentNullException>(() => pipeline.XmlSingleStreamLoader(stream, progress));
        Assert.Throws<ArgumentNullException>(() => pipeline.XmlSingleStreamLoader(new MemoryStream(), noProgress));

        Assert.Throws<ArgumentNullException>(() => ((IEtlPipeline<PersonRecord>)null!).XmlMultiStreamLoader(_ => new MemoryStream(), progress));
        Assert.Throws<ArgumentNullException>(() => pipeline.XmlMultiStreamLoader(factory, progress));
        Assert.Throws<ArgumentNullException>(() => pipeline.XmlMultiStreamLoader(_ => new MemoryStream(), noProgress));
    }



    private static async Task<MemoryStream> CreateSingleStreamXmlAsync()
    {
        var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>(stream);
        await loader.LoadAsync(People.ToAsyncEnumerable());
        stream.Position = 0;
        return stream;
    }



    private static Stream CreateRecordStream(PersonRecord record)
    {
        var stream = new MemoryStream();
        PersonSerializer.Serialize(stream, record);
        stream.Position = 0;
        return stream;
    }
}
