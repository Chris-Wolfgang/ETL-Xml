using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// A record type used only by <see cref="XmlMetricsTests"/> so its measurements are isolated —
/// the <c>Wolfgang.Etl.Xml</c> meter is process-global, so the tests filter by the
/// <c>etl.record_type</c> tag to ignore metrics emitted by other test classes running in parallel.
/// </summary>
public sealed class MetricProbe
{
    public string Name { get; set; } = string.Empty;

    public int Value { get; set; }
}


/// <summary>
/// Verifies the OpenTelemetry-compatible metrics (#12) — the extractors and loaders emit the
/// documented counters + duration histogram on the <c>Wolfgang.Etl.Xml</c> meter, tagged with
/// operation / component / record type.
/// </summary>
public sealed class XmlMetricsTests
{
    private const string ProbeType = nameof(MetricProbe);


    private static readonly MetricProbe[] Sample =
    {
        new() { Name = "Alice", Value = 30 },
        new() { Name = "Bob", Value = 25 },
    };


    private sealed record Measurement(string Instrument, double Value, Dictionary<string, object?> Tags);


    [Fact]
    public async Task SingleStreamExtractor_emits_items_extracted_counter_and_duration_with_tags()
    {
        using var source = await BuildXmlAsync(Sample).ConfigureAwait(false);

        var measurements = await CollectAsync(async () =>
        {
            var extractor = new XmlSingleStreamExtractor<MetricProbe>(source);
            await foreach (var _ in extractor.ExtractAsync().ConfigureAwait(false))
            {
            }
        }).ConfigureAwait(false);

        var extracted = measurements.Where(m => Is(m, "wolfgang.etl.xml.items.extracted")).ToList();
        Assert.Equal(2, extracted.Sum(m => m.Value));
        Assert.All(extracted, m =>
        {
            Assert.Equal("extract", m.Tags["etl.operation"]);
            Assert.Equal("XmlSingleStream", m.Tags["etl.component"]);
            Assert.Equal(ProbeType, m.Tags["etl.record_type"]);
        });
        Assert.Contains(measurements, m => Is(m, "wolfgang.etl.xml.operation.duration"));
    }


    [Fact]
    public async Task SingleStreamLoader_emits_items_loaded_counter_tagged_load()
    {
        var measurements = await CollectAsync(async () =>
        {
            using var ms = new MemoryStream();
            var loader = new XmlSingleStreamLoader<MetricProbe>
            (
                ms,
                new XmlSingleStreamLoaderOptions { LeaveOpen = true }
            );
            await loader.LoadAsync(Sample.ToAsyncEnumerable()).ConfigureAwait(false);
        }).ConfigureAwait(false);

        var loaded = measurements.Where(m => Is(m, "wolfgang.etl.xml.items.loaded")).ToList();
        Assert.Equal(2, loaded.Sum(m => m.Value));
        Assert.All(loaded, m => Assert.Equal("load", m.Tags["etl.operation"]));
        Assert.Contains(measurements, m => Is(m, "wolfgang.etl.xml.operation.duration"));
    }


    [Fact]
    public async Task Loader_emits_items_skipped_counter_for_the_skip_budget()
    {
        var measurements = await CollectAsync(async () =>
        {
            using var ms = new MemoryStream();
            var loader = new XmlSingleStreamLoader<MetricProbe>
            (
                ms,
                new XmlSingleStreamLoaderOptions { LeaveOpen = true }
            )
            {
                SkipItemCount = 1,
            };
            await loader.LoadAsync(Sample.ToAsyncEnumerable()).ConfigureAwait(false);
        }).ConfigureAwait(false);

        Assert.Equal(1, measurements.Where(m => Is(m, "wolfgang.etl.xml.items.skipped")).Sum(m => m.Value));
    }


    private static bool Is(Measurement measurement, string instrumentName) =>
        string.Equals(measurement.Instrument, instrumentName, StringComparison.Ordinal);


    private static async Task<List<Measurement>> CollectAsync(Func<Task> action)
    {
        var measurements = new List<Measurement>();

        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (string.Equals(instrument.Meter.Name, XmlMetrics.MeterName, StringComparison.Ordinal))
                {
                    l.EnableMeasurementEvents(instrument);
                }
            },
        };

        listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
            Add(measurements, instrument.Name, value, tags));
        listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
            Add(measurements, instrument.Name, value, tags));
        listener.Start();

        await action().ConfigureAwait(false);

        return measurements;
    }


    // Only records measurements tagged with this test's probe record type, ignoring metrics from
    // other test classes emitting to the same process-global meter in parallel.
    private static void Add(
        List<Measurement> sink,
        string name,
        double value,
        ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var tag in tags)
        {
            dict[tag.Key] = tag.Value;
        }

        if (!dict.TryGetValue("etl.record_type", out var recordType)
            || !string.Equals(recordType as string, ProbeType, StringComparison.Ordinal))
        {
            return;
        }

        lock (sink)
        {
            sink.Add(new Measurement(name, value, dict));
        }
    }


    private static async Task<MemoryStream> BuildXmlAsync(MetricProbe[] items)
    {
        var ms = new MemoryStream();
        var loader = new XmlSingleStreamLoader<MetricProbe>
        (
            ms,
            new XmlSingleStreamLoaderOptions { LeaveOpen = true }
        );
        await loader.LoadAsync(items.ToAsyncEnumerable()).ConfigureAwait(false);
        ms.Position = 0;
        return ms;
    }
}
