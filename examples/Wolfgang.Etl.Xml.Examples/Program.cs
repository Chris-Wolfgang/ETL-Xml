using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Wolfgang.Etl.Abstractions;
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
Console.WriteLine();
await FluentPipelineAsync().ConfigureAwait(false);
Console.WriteLine();
await FluentMultiStreamFanOutAsync().ConfigureAwait(false);
Console.WriteLine();
await FluentMultiStreamFanInAsync().ConfigureAwait(false);
Console.WriteLine();
await CompressedStreamRoundTripAsync().ConfigureAwait(false);
Console.WriteLine();
await XsdValidationAsync(loggerFactory).ConfigureAwait(false);
Console.WriteLine();
await ErrorPolicyDeadLetterAsync().ConfigureAwait(false);



// Demonstrates per-item error handling / dead-lettering (#11) on the multi-stream extractor.
// Each stream is an independent record, so assigning an XmlMultiStreamExtractor<TRecord>
// an ErrorPolicy lets a stream that fails to deserialize be captured (dead-lettered) and
// skipped instead of aborting the whole run. Ready-made policies come from
// Wolfgang.Etl.ErrorPolicies.ItemErrorPolicy.
static async Task ErrorPolicyDeadLetterAsync()
{
    Console.WriteLine("=== Per-item error handling / dead-lettering ===");
    Console.WriteLine();

    static Stream PersonStream(Person person)
    {
        var serializer = new XmlSerializer(typeof(Person));
        var emptyNs = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") });
        var ms = new MemoryStream();
        serializer.Serialize(ms, person, emptyNs);
        ms.Position = 0;
        return ms;
    }

    // Three streams: a valid person, a stream that does not deserialize to Person, then a valid person.
    var streams = new List<Stream>
    {
        PersonStream(new Person { FirstName = "Alice", LastName = "Smith", Age = 30 }),
        new MemoryStream(System.Text.Encoding.UTF8.GetBytes("<NotAPerson><garbage/></NotAPerson>")),
        PersonStream(new Person { FirstName = "Bob", LastName = "Jones", Age = 25 }),
    };

    var deadLetters = new List<Wolfgang.Etl.Abstractions.ItemErrorContext>();
    var extractor = new XmlMultiStreamExtractor<Person>(streams)
    {
        ErrorPolicy = Wolfgang.Etl.ErrorPolicies.ItemErrorPolicy.SkipAndDeadLetter(deadLetters),
    };

    await foreach (var person in extractor.ExtractAsync().ConfigureAwait(false))
    {
        Console.WriteLine($"  extracted: {person.FirstName} {person.LastName}");
    }

    Console.WriteLine($"Dead-lettered {deadLetters.Count} failed record(s) (skipped), extracted {extractor.CurrentItemCount}:");
    foreach (var failure in deadLetters)
    {
        Console.WriteLine($"  item #{failure.ItemNumber}: {failure.Exception.GetType().Name} — {failure.Exception.Message.Split('\n')[0]}");
    }
}



// Demonstrates validating the source XML against an XSD during extraction. Because
// XmlSingleStreamExtractor<TRecord> accepts a custom XmlReaderSettings
// (and clones it before use), setting XmlReaderSettings.ValidationType to
// ValidationType.Schema with a loaded XmlSchemaSet validates each
// element as it is read — no extra pass over the document. A schema violation surfaces from
// ExtractAsync as an InvalidOperationException whose inner exception is the
// XmlSchemaValidationException with the offending line and reason.
static async Task XsdValidationAsync(ILoggerFactory loggerFactory)
{
    Console.WriteLine("=== XSD validation during extraction ===");
    Console.WriteLine();

    const string schemaXsd = """
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
          <xs:element name="ArrayOfPerson">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="Person" minOccurs="0" maxOccurs="unbounded">
                  <xs:complexType>
                    <xs:sequence>
                      <xs:element name="FirstName" type="xs:string" minOccurs="0" />
                      <xs:element name="LastName" type="xs:string" minOccurs="0" />
                      <xs:element name="Age" type="xs:int" />
                      <xs:element name="Email" type="xs:string" minOccurs="0" />
                    </xs:sequence>
                  </xs:complexType>
                </xs:element>
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """;

    var schemas = new XmlSchemaSet();
    using (var schemaReader = XmlReader.Create(new StringReader(schemaXsd)))
    {
        schemas.Add(targetNamespace: null, schemaReader);
    }

    XmlReaderSettings ValidatingSettings() => new()
    {
        ValidationType = ValidationType.Schema,
        Schemas = schemas,
    };

    var logger = loggerFactory.CreateLogger<XmlSingleStreamExtractor<Person>>();

    // Valid source — validates cleanly and yields the records.
    using var validStream = CreateSampleXmlStream();
    var validExtractor = new XmlSingleStreamExtractor<Person>(validStream, ValidatingSettings(), logger);
    var validated = 0;
    await foreach (var _ in validExtractor.ExtractAsync().ConfigureAwait(false))
    {
        validated++;
    }

    Console.WriteLine($"Valid document: extracted and schema-validated {validated} people.");

    // Invalid source — Age is not an integer, so the schema-validating reader rejects it.
    var invalidXml =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
        + "<ArrayOfPerson><Person><FirstName>Dave</FirstName><Age>middle-aged</Age></Person></ArrayOfPerson>";
    using var invalidStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidXml));
    var invalidExtractor = new XmlSingleStreamExtractor<Person>(invalidStream, ValidatingSettings(), logger);

    try
    {
        await foreach (var _ in invalidExtractor.ExtractAsync().ConfigureAwait(false))
        {
            // Drain the sequence — the schema violation surfaces as an exception
            // partway through, so no record is expected to arrive here.
        }
    }
    catch (InvalidOperationException ex) when (ex.InnerException is XmlSchemaValidationException schemaError)
    {
        Console.WriteLine($"Invalid document rejected during extraction: {schemaError.Message}");
    }
}



// Demonstrates reading and writing compressed XML. Because every
// extractor and loader works against a plain Stream, gzip (or any
// other System.IO.Compression codec) is transparent — you simply
// wrap the underlying stream in a GZipStream. Here sample records
// are serialized straight into a gzip stream (compress) and then read back out of
// one (decompress), a full .xml.gz round trip.
// Ownership note: the loader is given LeaveOpen = false so that when the
// load completes it disposes the GZipStream, which flushes the gzip
// footer into the backing buffer. The backing MemoryStream is kept
// alive via leaveOpen: true on the GZipStream so it can be
// rewound and read back. A file-based equivalent would swap the
// MemoryStream for File.Create("people.xml.gz") /
// File.OpenRead("people.xml.gz").
static async Task CompressedStreamRoundTripAsync()
{
    Console.WriteLine("=== Compressed streams (gzip .xml.gz round trip) ===");
    Console.WriteLine();

    var people = SamplePeople();

    // --- Compress: serialize records straight into a gzip stream ---
    var compressed = new MemoryStream();
    using (var gzip = new GZipStream(compressed, CompressionMode.Compress, leaveOpen: true))
    {
        var extractor = new TestExtractor<Person>(people);
        var transformer = new TestTransformer<Person>();

        // LeaveOpen: false — completing the load disposes the GZipStream, which
        // flushes the gzip footer. leaveOpen: true above keeps `compressed` usable.
        var loader = new XmlSingleStreamLoader<Person>
        (
            gzip,
            new XmlSingleStreamLoaderOptions { LeaveOpen = false }
        );

        await loader
            .LoadAsync(transformer.TransformAsync(extractor.ExtractAsync()))
            .ConfigureAwait(false);
    }

    Console.WriteLine($"Wrote {people.Count} records as gzip-compressed XML: {compressed.Length} bytes.");
    Console.WriteLine();

    // --- Decompress: read the records back out of the gzip stream ---
    compressed.Position = 0;
    using var gunzip = new GZipStream(compressed, CompressionMode.Decompress);

    var reader = new XmlSingleStreamExtractor<Person>(gunzip);
    var collector = new TestLoader<Person>(collectItems: true);

    await collector.LoadAsync(reader.ExtractAsync()).ConfigureAwait(false);

    Console.WriteLine($"Read {reader.CurrentItemCount} records back from the gzip stream:");
    Console.WriteLine();

    foreach (var person in collector.GetCollectedItems()!)
    {
        Console.WriteLine($"  {person.FirstName} {person.LastName}, age {person.Age}");
    }
}



// Demonstrates the fluent EtlPipeline chain using the XML source
// and sink factories. A single-root XML source is filtered by a Through stage
// and written to a single-root XML destination — no explicit extractor,
// transformer, or loader variables. This reads the same as the CSV/JSON siblings.
static async Task FluentPipelineAsync()
{
    Console.WriteLine("=== Fluent EtlPipeline (XML → filter → XML) ===");
    Console.WriteLine();

    var source = CreateSampleXmlStream();
    var destination = new MemoryStream();

    // Extract from XML → keep people aged 30+ → load to XML, all in one chain.
    await EtlPipeline
        .Create()
        .XmlSingleStreamExtractor<Person>(source)
        .Through<Person>(people => WhereAsync(people, p => p.Age >= 30))
        .XmlSingleStreamLoader(destination)
        .RunAsync()
        .ConfigureAwait(false);

    Console.WriteLine("Kept people aged 30 or older:");
    Console.WriteLine();

    destination.Position = 0;
    using var reader = new StreamReader(destination);
    Console.WriteLine(await reader.ReadToEndAsync().ConfigureAwait(false));
}



// Demonstrates the fluent EtlPipeline chain fanning a single XML
// source out to many destinations — one XML document per record — via
// XmlMultiStreamLoader. The loader disposes each factory-supplied stream
// after writing its record, so a real pipeline would return
// FileStreams here (e.g. File.Create($"{p.LastName}.xml")).
static async Task FluentMultiStreamFanOutAsync()
{
    Console.WriteLine("=== Fluent EtlPipeline (XML → fan out to one file per record) ===");
    Console.WriteLine();

    var source = CreateSampleXmlStream();
    var buffers = new Dictionary<string, MemoryStream>(StringComparer.Ordinal);

    // One XML source → one destination stream per record, all in one chain.
    await EtlPipeline
        .Create()
        .XmlSingleStreamExtractor<Person>(source)
        .XmlMultiStreamLoader(person =>
        {
            var ms = new MemoryStream();
            buffers[$"{person.FirstName}_{person.LastName}.xml"] = ms;
            return ms;
        })
        .RunAsync()
        .ConfigureAwait(false);

    Console.WriteLine($"Wrote {buffers.Count} XML documents, one per record:");
    Console.WriteLine();

    foreach (var (fileName, buffer) in buffers)
    {
        Console.WriteLine($"--- {fileName} ---");
        Console.WriteLine(System.Text.Encoding.UTF8.GetString(buffer.ToArray()));
        Console.WriteLine();
    }
}



// Demonstrates the fluent EtlPipeline chain fanning many
// single-document XML sources in to one XML destination via
// XmlMultiStreamExtractor — the mirror of the fan-out shape. The
// extractor disposes each source stream after reading its record.
static async Task FluentMultiStreamFanInAsync()
{
    Console.WriteLine("=== Fluent EtlPipeline (fan in many files → one XML) ===");
    Console.WriteLine();

    var streams = CreateSampleMultiStreams();
    var destination = new MemoryStream();

    // Many single-document XML streams → one single-root XML document.
    await EtlPipeline
        .Create()
        .XmlMultiStreamExtractor<Person>(streams)
        .XmlSingleStreamLoader(destination)
        .RunAsync()
        .ConfigureAwait(false);

    Console.WriteLine($"Merged {streams.Count} single-document streams into one XML document:");
    Console.WriteLine();

    destination.Position = 0;
    using var reader = new StreamReader(destination);
    Console.WriteLine(await reader.ReadToEndAsync().ConfigureAwait(false));
}



// Filters an async sequence in place — a minimal Through stage for the fluent
// pipeline example that avoids taking a dependency on System.Linq.Async.
static async IAsyncEnumerable<T> WhereAsync<T>(IAsyncEnumerable<T> items, Func<T, bool> predicate)
{
    await foreach (var item in items.ConfigureAwait(false))
    {
        if (predicate(item))
        {
            yield return item;
        }
    }
}



// Demonstrates extracting from XML through a full ETL pipeline.
// XmlSingleStreamExtractor reads from an XML stream, then a
// TestTransformer passes items through, and a TestLoader collects them.
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



// Demonstrates loading to XML with a custom root element name.
// By default the root element is ArrayOf{TypeName}; this shows
// how to override that with a domain-meaningful name.
// Also demonstrates leaveOpen: false so the stream is closed
// automatically when loading completes.
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



// Demonstrates loading to XML through a full ETL pipeline.
// TestExtractor provides in-memory data, TestTransformer passes it
// through, and XmlSingleStreamLoader writes the XML output.
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



// Demonstrates extracting from multiple XML streams (one item per file)
// through a full ETL pipeline using TestTransformer and TestLoader.
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



// Demonstrates loading to multiple XML streams (one item per file)
// through a full ETL pipeline using TestExtractor and TestTransformer.
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
        // The multi-stream loader closes each factory-supplied stream after writing its record.
        // MemoryStream.ToArray() still returns the written buffer after disposal, so read it that way.
        Console.WriteLine($"--- {fileName} ---");
        Console.WriteLine(System.Text.Encoding.UTF8.GetString(buffer.ToArray()));
        Console.WriteLine();
    }
}



// Creates a MemoryStream containing sample XML with three Person elements.
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



// Creates a list of MemoryStreams, each containing a single Person as XML.
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
