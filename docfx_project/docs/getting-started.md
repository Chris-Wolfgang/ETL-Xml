# Getting Started

This guide will help you quickly get up and running with Wolfgang.Etl.Xml.

## Prerequisites

- .NET 8.0 SDK or later (also supports .NET Framework 4.6.2+ and .NET Standard 2.0)
- (Optional) A logging provider (e.g. `Microsoft.Extensions.Logging`)

## Installation

### Via .NET CLI

```bash
dotnet add package Wolfgang.Etl.Xml
```

### Via Package Manager Console

```powershell
Install-Package Wolfgang.Etl.Xml
```

## Quick Start

### Extracting from XML

Read items from an XML stream and pipe them through a transformer into a loader:

```csharp
using Wolfgang.Etl.TestKit;
using Wolfgang.Etl.Xml;

// XmlSingleStreamExtractor reads items from a single XML document
// ILogger is optional — omit for no logging, or pass settings + logger for full control
var extractor = new XmlSingleStreamExtractor<Person>(xmlStream);

// Use TestKit placeholders for the rest of the pipeline
var transformer = new TestTransformer<Person>();
var loader = new TestLoader<Person>(collectItems: true);

await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync()));

var items = loader.GetCollectedItems();
```

### Loading to XML

Pipe items from any source through a transformer and write them to an XML stream:

```csharp
using Wolfgang.Etl.TestKit;
using Wolfgang.Etl.Xml;

// Use TestKit as the data source
var extractor = new TestExtractor<Person>(people);
var transformer = new TestTransformer<Person>();

// XmlSingleStreamLoader writes items to a single XML document
// Use the 3-param constructor to customize settings and enable logging
var loader = new XmlSingleStreamLoader<Person>
(
    outputStream,
    new XmlWriterSettings { Indent = true },
    logger  // optional — use 1-param constructor to omit
);

await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync()));
```

### Multi-stream (one item per file)

For scenarios where each record lives in its own XML file:

```csharp
// Extract from multiple XML files
var extractor = new XmlMultiStreamExtractor<Person>(xmlStreams);

// Load to individual XML files via a stream factory
var loader = new XmlMultiStreamLoader<Person>
(
    person => File.Create($"{person.Id}.xml")
);
```

## Next Steps

- Explore the [API Reference](../api/index.md) for detailed documentation
- Read the [Introduction](introduction.md) to learn more about Wolfgang.Etl.Xml
- Check out [example projects](https://github.com/Chris-Wolfgang/ETL-Xml/tree/main/examples) in the GitHub repository

## Additional Resources

- [GitHub Repository](https://github.com/Chris-Wolfgang/ETL-Xml)
- [Contributing Guidelines](https://github.com/Chris-Wolfgang/ETL-Xml/blob/main/CONTRIBUTING.md)
- [Report an Issue](https://github.com/Chris-Wolfgang/ETL-Xml/issues)
