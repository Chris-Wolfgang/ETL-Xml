using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Wolfgang.Etl.Xml.ShadowWorkloads;

/// <summary>
/// Realistic consumer workloads for ETL-Xml. Unlike the micro-benchmarks in
/// <c>benchmarks/</c>, these exercise production-shaped record counts and a
/// bursty concurrent-caller pattern, with <see cref="MemoryDiagnoserAttribute"/> tracking
/// per-op allocations so a nightly <c>shadow.yaml</c> run can compare the current
/// build against a baseline release for latency and allocation regressions.
/// </summary>
[MemoryDiagnoser]
public class XmlShadowWorkloads
{
    // Small = per-request payload; large = a batch/export shape.
    [Params(1_000, 100_000)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Set by BenchmarkDotNet via reflection for each [Params] value.")]
    public int RecordCount { get; set; }

    private byte[] _sourceXml = Array.Empty<byte>();

    private WidgetRecord[] _records = Array.Empty<WidgetRecord>();


    [GlobalSetup]
    public async Task Setup()
    {
        _records = Enumerable.Range(0, RecordCount)
            .Select(i => new WidgetRecord
            {
                Id = i,
                Name = $"widget-{i}",
                Price = (decimal)((i * 0.01) % 100.0),
            })
            .ToArray();

        _sourceXml = await LoadToBytesAsync(_records).ConfigureAwait(false);
    }


    /// <summary>One extractor over the whole document — the export/read shape.</summary>
    [Benchmark]
    public async Task<int> Extract()
    {
        using var stream = new MemoryStream(_sourceXml);
        var extractor = new XmlSingleStreamExtractor<WidgetRecord>(stream);

        var count = 0;
        await foreach (var _ in extractor.ExtractAsync().ConfigureAwait(false))
        {
            count++;
        }

        return count;
    }


    /// <summary>Serialize the batch out — the write/persist shape.</summary>
    [Benchmark]
    public async Task<long> Load()
    {
        var bytes = await LoadToBytesAsync(_records).ConfigureAwait(false);
        return bytes.LongLength;
    }


    /// <summary>Full round trip — the common ETL passthrough shape.</summary>
    [Benchmark]
    public async Task<int> RoundTrip()
    {
        var bytes = await LoadToBytesAsync(_records).ConfigureAwait(false);

        using var stream = new MemoryStream(bytes);
        var extractor = new XmlSingleStreamExtractor<WidgetRecord>(stream);

        var count = 0;
        await foreach (var _ in extractor.ExtractAsync().ConfigureAwait(false))
        {
            count++;
        }

        return count;
    }


    /// <summary>Bursty concurrent callers — 16 independent extractions at once.</summary>
    [Benchmark]
    public async Task<int> ConcurrentExtractors()
    {
        var tasks = Enumerable.Range(0, 16).Select(async _ =>
        {
            using var stream = new MemoryStream(_sourceXml);
            var extractor = new XmlSingleStreamExtractor<WidgetRecord>(stream);

            var count = 0;
            await foreach (var __ in extractor.ExtractAsync().ConfigureAwait(false))
            {
                count++;
            }

            return count;
        });

        var counts = await Task.WhenAll(tasks).ConfigureAwait(false);
        return counts.Sum();
    }


    private static async Task<byte[]> LoadToBytesAsync(IReadOnlyList<WidgetRecord> records)
    {
        using var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<WidgetRecord>
        (
            stream,
            new XmlSingleStreamLoaderOptions { LeaveOpen = true }
        );

        await loader.LoadAsync(ToAsync(records)).ConfigureAwait(false);

        return stream.ToArray();
    }


    private static async IAsyncEnumerable<WidgetRecord> ToAsync(IEnumerable<WidgetRecord> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
