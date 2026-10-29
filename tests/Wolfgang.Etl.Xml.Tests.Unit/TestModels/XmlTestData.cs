using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Xml.Tests.Unit.TestModels;

/// <summary>
/// Fixtures and helpers shared by the unit-test classes: the five-person fixture, a single-stream
/// XML document built from records, one serialized stream per record, and draining an extractor.
/// </summary>
internal static class XmlTestData
{
    /// <summary>
    /// The five-person fixture. Each access returns new instances, so a test that mutates a record
    /// cannot affect another test class. Use <c>Take(n)</c> for a shorter list.
    /// </summary>
    internal static IReadOnlyList<PersonRecord> People => new List<PersonRecord>
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25 },
        new() { FirstName = "Carol", LastName = "White", Age = 35 },
        new() { FirstName = "Dave", LastName = "Brown", Age = 40 },
        new() { FirstName = "Eve", LastName = "Davis", Age = 28 },
    };



    /// <summary>
    /// Writes <paramref name="items"/> as one XML document with <see cref="XmlSingleStreamLoader{TRecord}"/>
    /// and returns the stream rewound to the start.
    /// </summary>
    internal static async Task<MemoryStream> BuildXmlAsync<T>(IEnumerable<T> items)
        where T : notnull, new()
    {
        var ms = new MemoryStream();
        var loader = new XmlSingleStreamLoader<T>
        (
            ms,
            new XmlSingleStreamLoaderOptions { LeaveOpen = true }
        );
        await loader.LoadAsync(items.ToAsyncEnumerable()).ConfigureAwait(false);
        ms.Position = 0;
        return ms;
    }



    /// <summary>
    /// Serializes each record into its own rewound <see cref="MemoryStream"/>, lazily, the input shape
    /// <see cref="XmlMultiStreamExtractor{TRecord}"/> reads.
    /// </summary>
    internal static IEnumerable<Stream> SerializeEach<T>(IEnumerable<T> items)
    {
        var serializer = new XmlSerializer(typeof(T));
        foreach (var item in items)
        {
            var ms = new MemoryStream();
            serializer.Serialize(ms, item);
            ms.Position = 0;
            yield return ms;
        }
    }



    /// <summary>
    /// Enumerates the extractor to the end, discarding the items.
    /// </summary>
    internal static async Task DrainAsync<T>(ExtractorBase<T, XmlReport> extractor)
        where T : notnull
    {
        await foreach (var _ in extractor.ExtractAsync().ConfigureAwait(false))
        {
        }
    }
}
