using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Wolfgang.Etl.Xml;

namespace Wolfgang.Etl.Xml.AotSmoke;

// A public class with a parameterless ctor — what a consumer would round-trip.
public sealed class Person
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public int Age { get; set; }
}


// Native-AOT smoke (#135). The library serializes via System.Xml.Serialization.XmlSerializer,
// which uses runtime reflection / Reflection.Emit — so it is marked [RequiresUnreferencedCode]
// and is NOT expected to work under Native AOT. This smoke asserts the BOUNDARY rather than
// claiming safety:
//   phase 1 — the trim-safe options/report POCO surface must run clean under AOT;
//   phase 2 — the extract/load reflection path is expected to fail under AOT; the expected
//             failure is a success, an unexpected success prints a loud NOTE (the marker may be
//             removable), and any other exception fails the job.
internal static class Program
{
    private static async Task<int> Main()
    {
        // --- Phase 1: the AOT-safe surface (pure POCOs, no reflection) ---
        try
        {
            var extractorOptions = new XmlSingleStreamExtractorOptions { LeaveOpen = true };
            var loaderOptions = new XmlSingleStreamLoaderOptions { RootElementName = "People", LeaveOpen = false };
            var report = new XmlReport(currentItemCount: 0, currentSkippedItemCount: 0);

            Console.WriteLine(
                $"AOT-safe surface OK (extractor.LeaveOpen={extractorOptions.LeaveOpen}, " +
                $"loader.Root={loaderOptions.RootElementName}, report.Skipped={report.CurrentSkippedItemCount}).");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"AOT smoke FAILED: the trim-safe POCO surface threw {ex.GetType().Name}: {ex.Message}");
            return 1;
        }

        // --- Phase 2: the reflection-based extract/load path ---
        try
        {
            var count = await RoundTripAsync().ConfigureAwait(false);
            Console.WriteLine(
                $"NOTE: extract/load round trip SUCCEEDED under Native AOT ({count} records) — the " +
                "[RequiresUnreferencedCode] marker may now be removable; review whether the library " +
                "gained AOT support (e.g. source-generated serialization) and update this smoke.");
            return 0;
        }
        catch (Exception ex) when (IsExpectedAotFailure(ex))
        {
            var reported = Unwrap(ex);
            Console.WriteLine(
                "OK: extract/load is unavailable under Native AOT exactly as documented " +
                $"([RequiresUnreferencedCode]) — {reported.GetType().Name}: {reported.Message}");
            return 0;
        }
    }


    // XmlSerializer's runtime code generation is disabled under Native AOT. Depending on where it
    // trips (constructing the serializer in a static initializer, or the first Serialize/Deserialize
    // call) the surfaced exception varies, so accept the family of "no dynamic code / not supported"
    // failures — including a TypeInitializationException wrapping one.
    private static bool IsExpectedAotFailure(Exception ex)
    {
        var inner = Unwrap(ex);
        return inner is NotSupportedException
            or PlatformNotSupportedException
            or InvalidOperationException
            or MissingMethodException;
    }


    private static Exception Unwrap(Exception ex) =>
        ex is TypeInitializationException && ex.InnerException is not null ? ex.InnerException : ex;


    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "Smoke test: intentionally exercises the [RequiresUnreferencedCode] extract/load path to verify AOT runtime behavior.")]
    private static async Task<int> RoundTripAsync()
    {
        var people = new[]
        {
            new Person { FirstName = "Alice", LastName = "Smith", Age = 30 },
            new Person { FirstName = "Bob", LastName = "Jones", Age = 25 },
        };

        using var buffer = new MemoryStream();
        var loader = new XmlSingleStreamLoader<Person>(buffer, new XmlSingleStreamLoaderOptions { LeaveOpen = true });
        await loader.LoadAsync(ToAsync(people)).ConfigureAwait(false);

        buffer.Position = 0;
        var extractor = new XmlSingleStreamExtractor<Person>(buffer);

        var count = 0;
        await foreach (var _ in extractor.ExtractAsync().ConfigureAwait(false))
        {
            count++;
        }

        return count;
    }


    private static async IAsyncEnumerable<Person> ToAsync(IEnumerable<Person> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
