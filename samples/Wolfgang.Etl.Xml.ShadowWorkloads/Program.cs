using BenchmarkDotNet.Running;

namespace Wolfgang.Etl.Xml.ShadowWorkloads;

/// <summary>
/// Entry point for the ETL-Xml shadow-workload benchmarks (issue #121).
/// </summary>
/// <remarks>
/// These are realistic consumer workloads — production-shaped record counts and
/// concurrent-caller patterns round-tripped through the XML extractor/loader over
/// in-memory streams — rather than the micro-benchmarks in the <c>benchmarks/</c>
/// project. The nightly <c>shadow.yaml</c> workflow replays them against a baseline
/// release and the current build, comparing latency and allocations to catch
/// regressions before release.
///
/// Run all workloads:      dotnet run -c Release --
/// Filter a single one:    dotnet run -c Release -- --filter '*RoundTrip*'
///
/// No external services required — everything runs over <see cref="System.IO.MemoryStream"/>.
/// </remarks>
internal static class Program
{
    private static void Main(string[] args) =>
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
