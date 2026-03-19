using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using Wolfgang.Etl.Xml;
using Wolfgang.Etl.Xml.Examples;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});

await SingleStreamExample(loggerFactory);
Console.WriteLine();
await MultiStreamExample(loggerFactory);
Console.WriteLine();
await ProgressReportingExample(loggerFactory);
Console.WriteLine();
await SkipAndMaxExample(loggerFactory);



static async Task SingleStreamExample(ILoggerFactory loggerFactory)
{
    Console.WriteLine("=== Single Stream Example ===");
    Console.WriteLine();

    // Create sample data
    var people = new List<Person>
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30, Email = "alice@example.com" },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25, Email = "bob@example.com" },
        new() { FirstName = "Carol", LastName = "White", Age = 35, Email = "carol@example.com" },
    };

    // --- Load to XML ---
    var stream = new MemoryStream();
    var loader = new XmlSingleStreamLoader<Person>
    (
        stream,
        loggerFactory.CreateLogger<XmlSingleStreamLoader<Person>>()
    );

    await loader.LoadAsync(people.ToAsyncEnumerable());
    Console.WriteLine($"Loaded {loader.CurrentItemCount} items to XML stream.");

    // Show the XML
    stream.Position = 0;
    using var reader = new StreamReader(stream);
    var xml = await reader.ReadToEndAsync();
    Console.WriteLine();
    Console.WriteLine("Generated XML:");
    Console.WriteLine(xml);

    // --- Extract from XML ---
    stream.Position = 0;
    var extractor = new XmlSingleStreamExtractor<Person>
    (
        stream,
        loggerFactory.CreateLogger<XmlSingleStreamExtractor<Person>>()
    );

    Console.WriteLine("Extracted items:");
    await foreach (var person in extractor.ExtractAsync())
    {
        Console.WriteLine($"  {person.FirstName} {person.LastName}, age {person.Age}");
    }
}



static async Task MultiStreamExample(ILoggerFactory loggerFactory)
{
    Console.WriteLine("=== Multi-Stream Example ===");
    Console.WriteLine();

    var people = new List<Person>
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30, Email = "alice@example.com" },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25, Email = "bob@example.com" },
    };

    // --- Load each person to its own stream ---
    var buffers = new Dictionary<string, MemoryStream>();
    var loader = new XmlMultiStreamLoader<Person>
    (
        person =>
        {
            var key = $"{person.FirstName}_{person.LastName}.xml";
            var ms = new MemoryStream();
            buffers[key] = ms;
            return ms;
        },
        loggerFactory.CreateLogger<XmlMultiStreamLoader<Person>>()
    );

    await loader.LoadAsync(people.ToAsyncEnumerable());
    Console.WriteLine($"Loaded {loader.CurrentItemCount} items to {buffers.Count} streams.");

    // --- Extract from the streams ---
    var streams = buffers.Values.Select(ms =>
    {
        var copy = new MemoryStream(ms.ToArray());
        return (Stream)copy;
    });

    var extractor = new XmlMultiStreamExtractor<Person>
    (
        streams,
        loggerFactory.CreateLogger<XmlMultiStreamExtractor<Person>>()
    );

    Console.WriteLine("Extracted items:");
    await foreach (var person in extractor.ExtractAsync())
    {
        Console.WriteLine($"  {person.FirstName} {person.LastName}, age {person.Age}");
    }
}



static async Task ProgressReportingExample(ILoggerFactory loggerFactory)
{
    Console.WriteLine("=== Progress Reporting Example ===");
    Console.WriteLine();

    var people = Enumerable.Range(1, 20).Select(i => new Person
    {
        FirstName = $"Person{i}",
        LastName = $"Last{i}",
        Age = 20 + i,
    }).ToList();

    var stream = new MemoryStream();
    var loader = new XmlSingleStreamLoader<Person>
    (
        stream,
        loggerFactory.CreateLogger<XmlSingleStreamLoader<Person>>()
    );
    loader.ReportingInterval = 100; // Report every 100ms

    var progress = new Progress<XmlReport>(report =>
        Console.WriteLine($"  Progress: {report.CurrentItemCount} items loaded, {report.CurrentSkippedItemCount} skipped")
    );

    await loader.LoadAsync(people.ToAsyncEnumerable(), progress);
    Console.WriteLine($"Completed. Loaded {loader.CurrentItemCount} items.");
}



static async Task SkipAndMaxExample(ILoggerFactory loggerFactory)
{
    Console.WriteLine("=== Skip and Maximum Item Count Example ===");
    Console.WriteLine();

    // Create a large XML stream
    var people = Enumerable.Range(1, 100).Select(i => new Person
    {
        FirstName = $"Person{i}",
        LastName = $"Last{i}",
        Age = 20 + (i % 50),
    }).ToList();

    var stream = new MemoryStream();
    var loader = new XmlSingleStreamLoader<Person>
    (
        stream,
        new XmlWriterSettings { Indent = false },
        loggerFactory.CreateLogger<XmlSingleStreamLoader<Person>>()
    );

    await loader.LoadAsync(people.ToAsyncEnumerable());
    Console.WriteLine($"Loaded {loader.CurrentItemCount} items to stream.");

    // Now extract with skip and max
    stream.Position = 0;
    var extractor = new XmlSingleStreamExtractor<Person>
    (
        stream,
        loggerFactory.CreateLogger<XmlSingleStreamExtractor<Person>>()
    );
    extractor.SkipItemCount = 10;     // Skip first 10
    extractor.MaximumItemCount = 5;   // Then take 5

    Console.WriteLine("Extracting with SkipItemCount=10, MaximumItemCount=5:");
    await foreach (var person in extractor.ExtractAsync(CancellationToken.None))
    {
        Console.WriteLine($"  {person.FirstName} {person.LastName}");
    }

    Console.WriteLine($"Extracted: {extractor.CurrentItemCount}, Skipped: {extractor.CurrentSkippedItemCount}");
}
