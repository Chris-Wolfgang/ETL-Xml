using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Wolfgang.Etl.Xml;

namespace Wolfgang.Etl.Xml.AotSmoke;

// A public class with a parameterless ctor — what a consumer would round-trip. Its getters are never
// called from code, so only the library's [DynamicallyAccessedMembers] annotation keeps them.
public sealed class Person
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public int Age { get; set; }
}


// Native-AOT smoke (#135, #293). The library serializes via System.Xml.Serialization.XmlSerializer,
// which requires unreferenced and dynamic code, so every public entry point carries
// [RequiresUnreferencedCode] and [RequiresDynamicCode]. Under Native AOT XmlSerializer falls back to its
// reflection-based mode; the record type's public members survive trimming because the library annotates
// TRecord with [DynamicallyAccessedMembers]. This smoke asserts that contract:
//   phase 1 — the trim-safe options/report POCO surface must run clean under AOT;
//   phase 2 — an extract/load round trip must run AND return the same values. Before #293 the round trip
//             "succeeded" with every property trimmed away (empty <Person /> elements), so counting records
//             is not enough: the values are compared.
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

        // --- Phase 2: the XmlSerializer extract/load path ---
        var expected = new[]
        {
            new Person { FirstName = "Alice", LastName = "Smith", Age = 30 },
            new Person { FirstName = "Bob", LastName = "Jones", Age = 25 },
        };

        List<Person> actual;
        try
        {
            actual = await RoundTripAsync(expected).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"AOT smoke FAILED: the extract/load round trip threw {ex.GetType().Name}: {ex.Message}");
            return 1;
        }

        if (actual.Count != expected.Length)
        {
            Console.Error.WriteLine($"AOT smoke FAILED: expected {expected.Length} records, extracted {actual.Count}.");
            return 1;
        }

        for (var i = 0; i < expected.Length; i++)
        {
            if (!string.Equals(actual[i].FirstName, expected[i].FirstName, StringComparison.Ordinal)
                || !string.Equals(actual[i].LastName, expected[i].LastName, StringComparison.Ordinal)
                || actual[i].Age != expected[i].Age)
            {
                Console.Error.WriteLine(
                    $"AOT smoke FAILED: record {i} came back as '{actual[i].FirstName}|{actual[i].LastName}|{actual[i].Age}', " +
                    $"expected '{expected[i].FirstName}|{expected[i].LastName}|{expected[i].Age}' (members trimmed?).");
                return 1;
            }
        }

        Console.WriteLine($"OK: extract/load round trip under Native AOT returned all {actual.Count} records with their values.");
        return 0;
    }


    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "Smoke test: exercises the [RequiresUnreferencedCode] extract/load path to verify AOT runtime behavior. Person has only primitive members, which the library's [DynamicallyAccessedMembers] keeps.")]
    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050",
        Justification = "Smoke test: exercises the [RequiresDynamicCode] extract/load path to verify XmlSerializer's reflection fallback under Native AOT.")]
    private static async Task<List<Person>> RoundTripAsync(IEnumerable<Person> people)
    {
        using var buffer = new MemoryStream();
        var loader = new XmlSingleStreamLoader<Person>(buffer, new XmlSingleStreamLoaderOptions { LeaveOpen = true });
        await loader.LoadAsync(ToAsync(people)).ConfigureAwait(false);

        buffer.Position = 0;
        var extractor = new XmlSingleStreamExtractor<Person>(buffer);

        var result = new List<Person>();
        await foreach (var person in extractor.ExtractAsync().ConfigureAwait(false))
        {
            result.Add(person);
        }

        return result;
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
