using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// The <see cref="Meter"/> and instruments the XML extractors and loaders emit to (#12).
/// Subscribe with a <c>System.Diagnostics.Metrics.MeterListener</c> or OpenTelemetry using the
/// meter name <see cref="MeterName"/> (<c>Wolfgang.Etl.Xml</c>). Every instrument is a no-op —
/// zero measurable overhead — when no listener is registered, so instrumentation is always on and
/// requires no configuration from the caller.
/// </summary>
/// <remarks>
/// All measurements are tagged with <c>etl.operation</c> (<c>extract</c> / <c>load</c>),
/// <c>etl.component</c> (<c>XmlSingleStream</c> / <c>XmlMultiStream</c>), and <c>etl.record_type</c>
/// (the record type name).
/// </remarks>
internal static class XmlMetrics
{
    /// <summary>The meter name consumers subscribe to.</summary>
    internal const string MeterName = "Wolfgang.Etl.Xml";


    private static readonly Meter Meter = new(MeterName);


    /// <summary>Counts items successfully extracted.</summary>
    internal static readonly Counter<long> ItemsExtracted =
        Meter.CreateCounter<long>("wolfgang.etl.xml.items.extracted");


    /// <summary>Counts items successfully loaded.</summary>
    internal static readonly Counter<long> ItemsLoaded =
        Meter.CreateCounter<long>("wolfgang.etl.xml.items.loaded");


    /// <summary>Counts items skipped by the skip budget.</summary>
    internal static readonly Counter<long> ItemsSkipped =
        Meter.CreateCounter<long>("wolfgang.etl.xml.items.skipped");


    /// <summary>Counts items that failed and were skipped / dead-lettered by the error policy.</summary>
    internal static readonly Counter<long> ItemsErrored =
        Meter.CreateCounter<long>("wolfgang.etl.xml.items.errored");


    /// <summary>Records the duration, in milliseconds, of a completed extract / load operation.</summary>
    internal static readonly Histogram<double> OperationDuration =
        Meter.CreateHistogram<double>("wolfgang.etl.xml.operation.duration", "ms");


    // Each Record* helper guards on the instrument's Enabled flag so that, with no listener
    // registered, the per-item hot path is a single boolean check — no tag marshalling.

    internal static void RecordExtracted(KeyValuePair<string, object?>[] tags) => Bump(ItemsExtracted, tags);


    internal static void RecordLoaded(KeyValuePair<string, object?>[] tags) => Bump(ItemsLoaded, tags);


    internal static void RecordSkipped(KeyValuePair<string, object?>[] tags) => Bump(ItemsSkipped, tags);


    internal static void RecordErrored(KeyValuePair<string, object?>[] tags) => Bump(ItemsErrored, tags);


    private static void Bump(Counter<long> counter, KeyValuePair<string, object?>[] tags)
    {
        if (counter.Enabled)
        {
            counter.Add(1, tags);
        }
    }


    /// <summary>
    /// Times an operation and records <see cref="OperationDuration"/> on dispose. Use with
    /// <c>using</c> at the top of a worker so the duration is captured when the operation completes
    /// (including for async iterators, whose <c>using</c> runs on enumeration completion / disposal).
    /// </summary>
    internal static OperationScope StartOperation(KeyValuePair<string, object?>[] tags) => new(tags);


    internal sealed class OperationScope : IDisposable
    {
        private readonly long _startTimestamp;
        private readonly KeyValuePair<string, object?>[] _tags;


        internal OperationScope(KeyValuePair<string, object?>[] tags)
        {
            _tags = tags;
            _startTimestamp = Stopwatch.GetTimestamp();
        }


        public void Dispose()
        {
            var elapsedMs = (Stopwatch.GetTimestamp() - _startTimestamp) * 1000.0 / Stopwatch.Frequency;
            OperationDuration.Record(elapsedMs, _tags);
        }
    }
}
