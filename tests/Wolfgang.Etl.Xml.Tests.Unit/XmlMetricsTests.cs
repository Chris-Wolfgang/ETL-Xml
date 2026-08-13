using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.ErrorPolicies;
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
/// A record used only by the metrics error-path test — its <see cref="Value"/> getter throws so the
/// multi-stream loader's error policy fires. A dedicated type keeps its <c>etl.record_type</c> tag
/// unique to this test class.
/// </summary>
public sealed class ExplodingMetricProbe
{
    public bool Explode { get; set; }

    public string Value
    {
        get => Explode ? throw new InvalidOperationException("boom") : "ok";
        // XmlSerializer requires a public setter to round-trip the property; the
        // value is never read back, so it is deliberately discarded.
        set => _ = value;
    }
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
        using var source = await BuildXmlAsync(Sample);

        var measurements = await CollectAsync(async () =>
        {
            var extractor = new XmlSingleStreamExtractor<MetricProbe>(source);
            await foreach (var _ in extractor.ExtractAsync())
            {
            }
        });

        var extracted = measurements.Where(m => Is(m, "wolfgang.etl.xml.items.extracted")).ToList();
        Assert.Equal(2, extracted.Sum(m => m.Value));
        Assert.All(extracted, m => AssertTags(m, "extract", "XmlSingleStream"));
        Assert.Contains(measurements, m => Is(m, "wolfgang.etl.xml.operation.duration")
            && Tag(m, "etl.operation", "extract")
            && Tag(m, "etl.component", "XmlSingleStream")
            && Tag(m, "etl.record_type", ProbeType));
    }


    [Fact]
    public async Task SingleStreamLoader_emits_items_loaded_counter_and_duration_with_tags()
    {
        var measurements = await CollectAsync(async () =>
        {
            using var ms = new MemoryStream();
            var loader = new XmlSingleStreamLoader<MetricProbe>
            (
                ms,
                new XmlSingleStreamLoaderOptions { LeaveOpen = true }
            );
            await loader.LoadAsync(Sample.ToAsyncEnumerable());
        });

        var loaded = measurements.Where(m => Is(m, "wolfgang.etl.xml.items.loaded")).ToList();
        Assert.Equal(2, loaded.Sum(m => m.Value));
        Assert.All(loaded, m => AssertTags(m, "load", "XmlSingleStream"));
        Assert.Contains(measurements, m => Is(m, "wolfgang.etl.xml.operation.duration")
            && Tag(m, "etl.operation", "load")
            && Tag(m, "etl.component", "XmlSingleStream"));
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
            await loader.LoadAsync(Sample.ToAsyncEnumerable());
        });

        Assert.Equal(1, measurements.Where(m => Is(m, "wolfgang.etl.xml.items.skipped")).Sum(m => m.Value));
    }


    [Fact]
    public async Task SingleStreamExtractor_emits_items_skipped_counter_tagged_extract()
    {
        using var source = await BuildXmlAsync(Sample);

        var measurements = await CollectAsync(async () =>
        {
            var extractor = new XmlSingleStreamExtractor<MetricProbe>(source)
            {
                SkipItemCount = 1,
            };
            await foreach (var _ in extractor.ExtractAsync())
            {
            }
        });

        var skipped = measurements.Where(m => Is(m, "wolfgang.etl.xml.items.skipped")).ToList();
        Assert.Equal(1, skipped.Sum(m => m.Value));
        Assert.All(skipped, m => AssertTags(m, "extract", "XmlSingleStream"));
    }


    [Fact]
    public async Task MultiStreamExtractor_emits_items_extracted_counter_and_duration_tagged_multistream()
    {
        var streams = Sample.Select(SerializeProbe).ToArray();

        var measurements = await CollectAsync(async () =>
        {
            var extractor = new XmlMultiStreamExtractor<MetricProbe>(streams);
            await foreach (var _ in extractor.ExtractAsync())
            {
            }
        });

        var extracted = measurements.Where(m => Is(m, "wolfgang.etl.xml.items.extracted")).ToList();
        Assert.Equal(2, extracted.Sum(m => m.Value));
        Assert.All(extracted, m => AssertTags(m, "extract", "XmlMultiStream"));
        Assert.Contains(measurements, m => Is(m, "wolfgang.etl.xml.operation.duration")
            && Tag(m, "etl.operation", "extract")
            && Tag(m, "etl.component", "XmlMultiStream")
            && Tag(m, "etl.record_type", ProbeType));
    }


    [Fact]
    public async Task MultiStreamLoader_emits_items_loaded_counter_and_duration_tagged_multistream()
    {
        var measurements = await CollectAsync(async () =>
        {
            var loader = new XmlMultiStreamLoader<MetricProbe>(_ => new MemoryStream());
            await loader.LoadAsync(Sample.ToAsyncEnumerable());
        });

        var loaded = measurements.Where(m => Is(m, "wolfgang.etl.xml.items.loaded")).ToList();
        Assert.Equal(2, loaded.Sum(m => m.Value));
        Assert.All(loaded, m => AssertTags(m, "load", "XmlMultiStream"));
        Assert.Contains(measurements, m => Is(m, "wolfgang.etl.xml.operation.duration")
            && Tag(m, "etl.operation", "load")
            && Tag(m, "etl.component", "XmlMultiStream"));
    }


    [Fact]
    public async Task MultiStreamLoader_error_policy_emits_items_errored_counter_tagged_multistream()
    {
        var measurements = await CollectAsync(
            async () =>
            {
                var loader = new XmlMultiStreamLoader<ExplodingMetricProbe>(_ => new MemoryStream())
                {
                    ErrorPolicy = ItemErrorPolicy.Skip,
                };
                await loader.LoadAsync(new[] { new ExplodingMetricProbe { Explode = true } }.ToAsyncEnumerable())
                    ;
            },
            recordType: nameof(ExplodingMetricProbe));

        var errored = measurements.Where(m => Is(m, "wolfgang.etl.xml.items.errored")).ToList();
        Assert.Equal(1, errored.Sum(m => m.Value));
        Assert.All(errored, m => Assert.Equal("XmlMultiStream", m.Tags["etl.component"]));
    }


    private static bool Is(Measurement measurement, string instrumentName) =>
        string.Equals(measurement.Instrument, instrumentName, StringComparison.Ordinal);


    private static bool Tag(Measurement measurement, string key, string value) =>
        measurement.Tags.TryGetValue(key, out var actual) && string.Equals(actual as string, value, StringComparison.Ordinal);


    private static void AssertTags(Measurement measurement, string operation, string component)
    {
        Assert.Equal(operation, measurement.Tags["etl.operation"]);
        Assert.Equal(component, measurement.Tags["etl.component"]);
    }


    private static async Task<List<Measurement>> CollectAsync(Func<Task> action, string recordType = ProbeType)
    {
        var measurements = new List<Measurement>();

        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (string.Equals(instrument.Meter.Name, XmlMetrics.MeterName, StringComparison.Ordinal))
            {
                l.EnableMeasurementEvents(instrument);
            }
        };

        listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
            Add(measurements, recordType, instrument.Name, value, tags));
        listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
            Add(measurements, recordType, instrument.Name, value, tags));
        listener.Start();

        await action().ConfigureAwait(false);

        return measurements;
    }


    // Only records measurements tagged with this test's record type, ignoring metrics from other
    // test classes emitting to the same process-global meter in parallel.
    private static void Add(
        List<Measurement> sink,
        string recordType,
        string name,
        double value,
        ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var tag in tags)
        {
            dict[tag.Key] = tag.Value;
        }

        if (!dict.TryGetValue("etl.record_type", out var actual)
            || !string.Equals(actual as string, recordType, StringComparison.Ordinal))
        {
            return;
        }

        lock (sink)
        {
            sink.Add(new Measurement(name, value, dict));
        }
    }


    private static MemoryStream SerializeProbe(MetricProbe item)
    {
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(MetricProbe));
        var ms = new MemoryStream();
        serializer.Serialize(ms, item);
        ms.Position = 0;
        return ms;
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
