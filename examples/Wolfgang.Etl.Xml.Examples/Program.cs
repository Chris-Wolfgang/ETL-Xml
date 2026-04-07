using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Wolfgang.Etl.TestKit;
using Wolfgang.Etl.Xml;
using Wolfgang.Etl.Xml.Examples;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});

await SingleStreamExtractPipelineAsync().ConfigureAwait(false);
Console.WriteLine();
await SingleStreamLoadPipelineAsync(loggerFactory).ConfigureAwait(false);
Console.WriteLine();
await SingleStreamLoadWithCustomRootAsync().ConfigureAwait(false);
Console.WriteLine();
await MultiStreamExtractPipelineAsync().ConfigureAwait(false);
Console.WriteLine();
await MultiStreamLoadPipelineAsync(loggerFactory).ConfigureAwait(false);



/// <summary>
/// Demonstrates extracting from XML through a full ETL pipeline.
/// XmlSingleStreamExtractor reads from an XML stream, then a
/// TestTransformer passes items through, and a TestLoader collects them.
/// </summary>
static async Task SingleStreamExtractPipelineAsync()
{
    Console.WriteLine("=== Single-Stream Extract Pipeline ===");
    Console.WriteLine();

    // Prepare sample XML data
    var xmlStream = CreateSampleXmlStream();

    // --- Extract → Transform → Load pipeline ---
    var extractor = new XmlSingleStreamExtractor<Person>(xmlStream);

    var transformer = new TestTransformer<Person>();
    var loader = new TestLoader<Person>(collectItems: true);

    await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync())).ConfigureAwait(false);

    // Show results
    Console.WriteLine($"Extracted {extractor.CurrentItemCount} items from XML.");
    Console.WriteLine($"Transformed {transformer.CurrentItemCount} items.");
    Console.WriteLine($"Loaded {loader.CurrentItemCount} items.");
    Console.WriteLine();

    foreach (var person in loader.GetCollectedItems()!)
    {
        Console.WriteLine($"  {person.FirstName} {person.LastName}, age {person.Age}");
    }
}



/// <summary>
/// Demonstrates loading to XML with a custom root element name.
/// By default the root element is <c>ArrayOf{TypeName}</c>; this shows
/// how to override that with a domain-meaningful name.
/// Also demonstrates <c>leaveOpen: false</c> so the stream is closed
/// automatically when loading completes.
/// </summary>
static async Task SingleStreamLoadWithCustomRootAsync()
{
    Console.WriteLine("=== Single-Stream Load with Custom Root Element ===");
    Console.WriteLine();

    var people = new List<Person>
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30, Email = "alice@example.com" },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25, Email = "bob@example.com" },
    };

    var extractor = new TestExtractor<Person>(people);
    var transformer = new TestTransformer<Person>();

    // leaveOpen: false — the MemoryStream is closed automatically after LoadAsync returns.
    var outputStream = new MemoryStream();
    var loader = new XmlSingleStreamLoader<Person>
    (
        outputStream,
        new XmlSingleStreamLoaderOptions
        {
            RootElementName = "People",
            LeaveOpen = false,
        }
    );

    await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync())).ConfigureAwait(false);

    Console.WriteLine($"Loaded {loader.CurrentItemCount} items using root element <People>.");
    Console.WriteLine();

    // MemoryStream.ToArray() returns the buffer regardless of disposal state.
    // For non-MemoryStream targets, read before disposing or use leaveOpen: true.
    var content = System.Text.Encoding.UTF8.GetString(outputStream.ToArray());
    Console.WriteLine(content);
}



/// <summary>
/// Demonstrates loading to XML through a full ETL pipeline.
/// TestExtractor provides in-memory data, TestTransformer passes it
/// through, and XmlSingleStreamLoader writes the XML output.
/// </summary>
static async Task SingleStreamLoadPipelineAsync(ILoggerFactory loggerFactory)
{
    Console.WriteLine("=== Single-Stream Load Pipeline ===");
    Console.WriteLine();

    // --- Extract → Transform → Load pipeline ---
    var people = new List<Person>
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30, Email = "alice@example.com" },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25, Email = "bob@example.com" },
        new() { FirstName = "Carol", LastName = "White", Age = 35, Email = "carol@example.com" },
    };

    var extractor = new TestExtractor<Person>(people);
    var transformer = new TestTransformer<Person>();

    var outputStream = new MemoryStream();
    var loader = new XmlSingleStreamLoader<Person>
    (
        outputStream,
        new XmlWriterSettings { Indent = true },
        loggerFactory.CreateLogger<XmlSingleStreamLoader<Person>>()
    );

    await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync())).ConfigureAwait(false);

    // Show resulting XML
    Console.WriteLine($"Extracted {extractor.CurrentItemCount} items from memory.");
    Console.WriteLine($"Transformed {transformer.CurrentItemCount} items.");
    Console.WriteLine($"Loaded {loader.CurrentItemCount} items to XML.");
    Console.WriteLine();

    outputStream.Position = 0;
    using var reader = new StreamReader(outputStream);
    Console.WriteLine(await reader.ReadToEndAsync().ConfigureAwait(false));
}



/// <summary>
/// Demonstrates extracting from multiple XML streams (one item per file)
/// through a full ETL pipeline using TestTransformer and TestLoader.
/// </summary>
static async Task MultiStreamExtractPipelineAsync()
{
    Console.WriteLine("=== Multi-Stream Extract Pipeline ===");
    Console.WriteLine();

    // Prepare individual XML streams (simulating one-item-per-file)
    var streams = CreateSampleMultiStreams();

    // --- Extract → Transform → Load pipeline ---
    var extractor = new XmlMultiStreamExtractor<Person>(streams);

    var transformer = new TestTransformer<Person>();
    var loader = new TestLoader<Person>(collectItems: true);

    await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync())).ConfigureAwait(false);

    Console.WriteLine($"Extracted {extractor.CurrentItemCount} items from {streams.Count} streams.");
    Console.WriteLine($"Transformed {transformer.CurrentItemCount} items.");
    Console.WriteLine($"Loaded {loader.CurrentItemCount} items.");
    Console.WriteLine();

    foreach (var person in loader.GetCollectedItems()!)
    {
        Console.WriteLine($"  {person.FirstName} {person.LastName}, age {person.Age}");
    }
}



/// <summary>
/// Demonstrates loading to multiple XML streams (one item per file)
/// through a full ETL pipeline using TestExtractor and TestTransformer.
/// </summary>
static async Task MultiStreamLoadPipelineAsync(ILoggerFactory loggerFactory)
{
    Console.WriteLine("=== Multi-Stream Load Pipeline ===");
    Console.WriteLine();

    // --- Extract → Transform → Load pipeline ---
    var people = new List<Person>
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30, Email = "alice@example.com" },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25, Email = "bob@example.com" },
    };

    var extractor = new TestExtractor<Person>(people);
    var transformer = new TestTransformer<Person>();

    var buffers = new Dictionary<string, MemoryStream>(StringComparer.Ordinal);
    var loader = new XmlMultiStreamLoader<Person>
    (
        person =>
        {
            var key = $"{person.FirstName}_{person.LastName}.xml";
            var ms = new MemoryStream();
            buffers[key] = ms;
            return ms;
        },
        new XmlWriterSettings { Indent = true },
        loggerFactory.CreateLogger<XmlMultiStreamLoader<Person>>()
    );

    await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync())).ConfigureAwait(false);

    Console.WriteLine($"Extracted {extractor.CurrentItemCount} items from memory.");
    Console.WriteLine($"Transformed {transformer.CurrentItemCount} items.");
    Console.WriteLine($"Loaded {loader.CurrentItemCount} items to {buffers.Count} XML streams.");
    Console.WriteLine();

    foreach (var (fileName, buffer) in buffers)
    {
        buffer.Position = 0;
        using var streamReader = new StreamReader(buffer);
        Console.WriteLine($"--- {fileName} ---");
        Console.WriteLine(await streamReader.ReadToEndAsync().ConfigureAwait(false));
        Console.WriteLine();
    }
}



/// <summary>
/// Creates a MemoryStream containing sample XML with three Person elements.
/// </summary>
static MemoryStream CreateSampleXmlStream()
{
    var serializer = new XmlSerializer(typeof(Person));
    var emptyNs = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") });

    var stream = new MemoryStream();
    var settings = new XmlWriterSettings { Indent = true, CloseOutput = false };
    using var writer = XmlWriter.Create(stream, settings);

    writer.WriteStartDocument();
    writer.WriteStartElement("ArrayOfPerson");

    foreach (var person in SamplePeople())
    {
        serializer.Serialize(writer, person, emptyNs);
    }

    writer.WriteEndElement();
    writer.WriteEndDocument();
    writer.Flush();

    stream.Position = 0;
    return stream;
}



/// <summary>
/// Creates a list of MemoryStreams, each containing a single Person as XML.
/// </summary>
static List<MemoryStream> CreateSampleMultiStreams()
{
    var serializer = new XmlSerializer(typeof(Person));
    var emptyNs = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") });
    var streams = new List<MemoryStream>();

    foreach (var person in SamplePeople())
    {
        var ms = new MemoryStream();
        serializer.Serialize(ms, person, emptyNs);
        ms.Position = 0;
        streams.Add(ms);
    }

    return streams;
}



static List<Person> SamplePeople() =>
    new()
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30, Email = "alice@example.com" },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25, Email = "bob@example.com" },
        new() { FirstName = "Carol", LastName = "White", Age = 35, Email = "carol@example.com" },
    };
