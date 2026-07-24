using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// Covers the XML extractors' adoption of the Abstractions #84 per-item error mechanism. A record
/// whose value cannot be deserialized (here a non-numeric <c>Age</c>) is routed through the base
/// <c>OnItemError</c>/<c>HandleItemError</c> policy via the <see cref="ErrorHandling"/> knob, so a
/// genuine failure is counted in <c>CurrentErrorItemCount</c>, surfaced as pipeline
/// <see cref="EtlPipelineProgress.ErrorItemCount"/>, and (for <see cref="ErrorHandling.CaptureAndContinue"/>)
/// collected in <c>Errors</c>. The single-stream case exercises reader repositioning: the bad element
/// is skipped and the next sibling is still read.
/// </summary>
public class XmlItemErrorHandlingTests
{
    // Root with three child records: good, bad (Age="abc"), good.
    private const string GoodBadGood =
        "<?xml version=\"1.0\"?><People>" +
        "<PersonRecord><FirstName>Carol</FirstName><LastName>Clark</LastName><Age>35</Age></PersonRecord>" +
        "<PersonRecord><FirstName>Eve</FirstName><LastName>Evans</LastName><Age>abc</Age></PersonRecord>" +
        "<PersonRecord><FirstName>Dan</FirstName><LastName>Davis</LastName><Age>40</Age></PersonRecord>" +
        "</People>";

    private static Stream SingleStream() => new MemoryStream(Encoding.UTF8.GetBytes(GoodBadGood));

    private static Stream Doc(string first, string last, string age) =>
        new MemoryStream(Encoding.UTF8.GetBytes(
            $"<?xml version=\"1.0\"?><PersonRecord><FirstName>{first}</FirstName><LastName>{last}</LastName><Age>{age}</Age></PersonRecord>"));


    // ---- Single-stream ----

    [Fact]
    public async Task SingleStream_CaptureAndContinue_skips_the_bad_element_and_captures_it()
    {
        var extractor = new XmlSingleStreamExtractor<PersonRecord>(SingleStream())
        {
            ErrorHandling = ErrorHandling.CaptureAndContinue,
        };

        var yielded = await Drain(extractor.ExtractAsync(CancellationToken.None));

        Assert.Equal(new[] { "Carol", "Dan" }, yielded.Select(p => p.FirstName));
        Assert.Equal(1, extractor.CurrentErrorItemCount);

        var error = Assert.Single(extractor.Errors);
        Assert.Equal(2, error.ItemNumber);                 // Eve is the 2nd element
        Assert.Contains("abc", error.RawContent);            // raw XML captured
    }


    [Fact]
    public async Task SingleStream_Throw_aborts_on_the_bad_element()
    {
        var extractor = new XmlSingleStreamExtractor<PersonRecord>(SingleStream());
        // default ErrorHandling == Throw -> OnItemError returns Abort -> rethrow

        await Assert.ThrowsAnyAsync<InvalidOperationException>(
            () => Drain(extractor.ExtractAsync(CancellationToken.None)));
    }


    [Fact]
    public async Task SingleStream_SkipAndLog_skips_without_capturing()
    {
        var extractor = new XmlSingleStreamExtractor<PersonRecord>(SingleStream())
        {
            ErrorHandling = ErrorHandling.SkipAndLog,
        };

        var yielded = await Drain(extractor.ExtractAsync(CancellationToken.None));

        Assert.Equal(2, yielded.Count);
        Assert.Equal(1, extractor.CurrentErrorItemCount);    // counted, never silent
        Assert.Empty(extractor.Errors);                      // but not collected
    }


    [Fact]
    public async Task Pipeline_ErrorItemCount_surfaces_single_stream_skips()
    {
        var reports = new List<EtlPipelineProgress>();
        var progress = new SyncProgress(reports.Add);
        var extractor = new XmlSingleStreamExtractor<PersonRecord>(SingleStream())
        {
            ErrorHandling = ErrorHandling.CaptureAndContinue,
        };
        var loader = new CountingLoader();

        await EtlPipeline
            .Create()
            .From(extractor)
            .To(loader)
            .RunAsync(progress);

        var final = reports[^1];
        Assert.Equal(2, final.ExtractedItemCount);
        Assert.Equal(2, final.LoadedItemCount);
        Assert.Equal(1, final.ErrorItemCount);
    }


    // ---- Multi-stream ----

    [Fact]
    public async Task MultiStream_CaptureAndContinue_skips_the_bad_stream_and_continues()
    {
        var streams = new[] { Doc("Carol", "Clark", "35"), Doc("Eve", "Evans", "abc"), Doc("Dan", "Davis", "40") };
        var extractor = new XmlMultiStreamExtractor<PersonRecord>(streams)
        {
            ErrorHandling = ErrorHandling.CaptureAndContinue,
        };

        var yielded = await Drain(extractor.ExtractAsync(CancellationToken.None));

        Assert.Equal(new[] { "Carol", "Dan" }, yielded.Select(p => p.FirstName));
        Assert.Equal(1, extractor.CurrentErrorItemCount);
        Assert.Equal(2, Assert.Single(extractor.Errors).ItemNumber);   // 2nd stream
    }


    [Fact]
    public async Task MultiStream_Throw_aborts_on_the_bad_stream()
    {
        var streams = new[] { Doc("Carol", "Clark", "35"), Doc("Eve", "Evans", "abc"), Doc("Dan", "Davis", "40") };
        var extractor = new XmlMultiStreamExtractor<PersonRecord>(streams);

        await Assert.ThrowsAnyAsync<InvalidOperationException>(
            () => Drain(extractor.ExtractAsync(CancellationToken.None)));
    }


    // ---- helpers / doubles ----

    private static async Task<List<PersonRecord>> Drain(IAsyncEnumerable<PersonRecord> source)
    {
        var result = new List<PersonRecord>();
        await foreach (var item in source.ConfigureAwait(false))
        {
            result.Add(item);
        }

        return result;
    }


    private sealed class SyncProgress : IProgress<EtlPipelineProgress>
    {
        private readonly Action<EtlPipelineProgress> _report;

        public SyncProgress(Action<EtlPipelineProgress> report) => _report = report;

        public void Report(EtlPipelineProgress value) => _report(value);
    }


    private sealed class CountingLoader : LoaderBase<PersonRecord, CountingLoader.Report>
    {
        public List<PersonRecord> Loaded { get; } = new();

        protected override async Task LoadWorkerAsync(IAsyncEnumerable<PersonRecord> items, CancellationToken token)
        {
            await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
            {
                Loaded.Add(item);
                IncrementCurrentItemCount();
            }
        }

        protected override Report CreateProgressReport() => new(CurrentItemCount);

        public sealed record Report(int Count);
    }
}
