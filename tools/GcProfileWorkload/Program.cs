// Sustained-load workload for GC / allocation profiling (#134).
//
// Runs an XML load + extract loop over an in-memory stream for a wall-clock
// duration configured via env / CLI. Designed to be run under
// `dotnet-counters collect` or `dotnet-trace` from a scheduled workflow so we
// can characterise gen0/1/2 promotion rates, LOH pressure, finalizer queue
// depth, and thread-pool starvation under a real ETL pattern.
//
// Not intended as a benchmark — the scale / iteration counts are arbitrary.
// Meaningful metrics come from the ETW / EventPipe trace captured by the outer
// workflow, not from the wall time this process reports.

using System.Diagnostics;
using Wolfgang.Etl.Xml;

const int rowsPerBatch = 5_000;
var durationSeconds = ParseDuration(args);

Console.WriteLine($"[gc-workload] Version : {typeof(XmlSingleStreamExtractorOptions).Assembly.GetName().Version}");
Console.WriteLine($"[gc-workload] Runtime : {Environment.Version}");
Console.WriteLine($"[gc-workload] ServerGC : {System.Runtime.GCSettings.IsServerGC}");
Console.WriteLine($"[gc-workload] Duration : {durationSeconds}s");
Console.WriteLine($"[gc-workload] Batch    : {rowsPerBatch} rows / cycle");
Console.WriteLine($"[gc-workload] PID      : {Environment.ProcessId}");

var stopwatch = Stopwatch.StartNew();
long cycles = 0, extracted = 0, loaded = 0;

// A workflow's timeout cancels the token so the process ends cleanly and still
// prints the summary.
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(durationSeconds));

try
{
    while (!cts.IsCancellationRequested)
    {
        // Load hot path: serialize a fresh batch to an in-memory XML document.
        using var buffer = new MemoryStream();
        var loader = new XmlSingleStreamLoader<Record>(
            buffer,
            new XmlSingleStreamLoaderOptions { LeaveOpen = true });
        await loader.LoadAsync(GenerateBatch(rowsPerBatch), cts.Token);
        loaded += rowsPerBatch;

        // Extract hot path: read every record back out of the same document.
        buffer.Position = 0;
        var extractor = new XmlSingleStreamExtractor<Record>(
            buffer,
            new XmlSingleStreamExtractorOptions { LeaveOpen = true });
        await foreach (var _ in extractor.ExtractAsync(cts.Token))
        {
            extracted++;
        }

        cycles++;
        if (cycles % 10 == 0)
        {
            var alloc = GC.GetTotalAllocatedBytes(precise: true);
            Console.WriteLine(
                $"[gc-workload] t={stopwatch.Elapsed.TotalSeconds,7:F1}s  " +
                $"cycles={cycles,5}  extracted={extracted,10}  loaded={loaded,10}  " +
                $"gen0={GC.CollectionCount(0),4}  gen1={GC.CollectionCount(1),3}  gen2={GC.CollectionCount(2),3}  " +
                $"total-alloc={alloc / 1024 / 1024,7} MB");
        }
    }
}
catch (OperationCanceledException)
{
    // Expected when the duration elapses mid-cycle.
}

Console.WriteLine();
Console.WriteLine($"[gc-workload] Completed {cycles} cycles in {stopwatch.Elapsed.TotalSeconds:F1}s.");
Console.WriteLine($"[gc-workload] Final GC counts: gen0={GC.CollectionCount(0)} gen1={GC.CollectionCount(1)} gen2={GC.CollectionCount(2)}");
Console.WriteLine($"[gc-workload] Peak working set: {Environment.WorkingSet / 1024 / 1024} MB");

static int ParseDuration(string[] args)
{
    if (args.Length > 0 && int.TryParse(args[0], out var s) && s > 0)
    {
        return s;
    }

    if (int.TryParse(Environment.GetEnvironmentVariable("GC_WORKLOAD_SECONDS"), out var envSeconds) && envSeconds > 0)
    {
        return envSeconds;
    }

    return 600; // 10 minutes default.
}

static async IAsyncEnumerable<Record> GenerateBatch(int count)
{
    for (var i = 0; i < count; i++)
    {
        yield return new Record { Id = i, Name = $"record-{i}", Price = i * 0.05m };
        if ((i & 0xFF) == 0)
        {
            await Task.Yield();
        }
    }
}

// XmlSerializer requires a public type with a public parameterless constructor.
public sealed class Record
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }
}
