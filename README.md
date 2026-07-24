# Wolfgang.Etl.Xml

Extractors and loaders for working with XML files using the Wolfgang.Etl design pattern

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Multi--Targeted-purple.svg)](https://dotnet.microsoft.com/)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-181717?logo=github)](https://github.com/Chris-Wolfgang/ETL-Xml)

---

## 📦 Installation

```bash
dotnet add package Wolfgang.Etl.Xml
```

---

## 📄 License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## 📚 Documentation

- **GitHub Repository:** [https://github.com/Chris-Wolfgang/ETL-Xml](https://github.com/Chris-Wolfgang/ETL-Xml)
- **API Documentation:** https://Chris-Wolfgang.github.io/ETL-Xml/
- **Formatting Guide:** [README-FORMATTING.md](docs/README-FORMATTING.md)
- **Contributing Guide:** [CONTRIBUTING.md](CONTRIBUTING.md)

---

## 🚀 Quick Start

Extract items from an XML stream through a full ETL pipeline:

```csharp
using Wolfgang.Etl.TestKit;
using Wolfgang.Etl.Xml;

// Extract from XML → Transform → Load into memory
var extractor = new XmlSingleStreamExtractor<Person>(xmlStream);

var transformer = new TestTransformer<Person>();
var loader = new TestLoader<Person>(collectItems: true);

await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync()));

var items = loader.GetCollectedItems();
```

Load items to an XML stream from an in-memory source:

```csharp
// Extract from memory → Transform → Load to XML
var extractor = new TestExtractor<Person>(people);
var transformer = new TestTransformer<Person>();

var loader = new XmlSingleStreamLoader<Person>(outputStream);

await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync()));
```

### Fluent pipeline (`EtlPipeline`)

For end-to-end wiring, the XML source and sink factories plug straight into the
`EtlPipeline` chain — no explicit extractor, transformer, or loader variables. The
factories read the same as the CSV/JSON siblings:

```csharp
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Xml;

// Read a single-root XML file → write a single-root XML file.
await EtlPipeline
    .Create()
    .XmlSingleStreamExtractor<Person>("people.xml")
    .XmlSingleStreamLoader<Person>("people-copy.xml")
    .RunAsync();
```

Insert a `Through` stage to transform or filter records mid-stream:

```csharp
await EtlPipeline
    .Create()
    .XmlSingleStreamExtractor<Person>("people.xml")
    .Through<Person>(people => people.Where(p => p.Age >= 18))
    .XmlSingleStreamLoader<Person>("adults.xml")
    .RunAsync(progress, cancellationToken);
```

Mix and match the single- and multi-stream shapes — e.g. fan a single XML document
out to one file per record:

```csharp
await EtlPipeline
    .Create()
    .XmlSingleStreamExtractor<Person>(sourceStream)
    .XmlMultiStreamLoader<Person>(person => File.Create($"{person.LastName}.xml"))
    .RunAsync();
```

…or fan the mirror direction — merge many single-document XML files back into one
root document:

```csharp
await EtlPipeline
    .Create()
    .XmlMultiStreamExtractor<Person>(Directory.EnumerateFiles("inbox", "*.xml").Select(File.OpenRead))
    .XmlSingleStreamLoader<Person>("people.xml")
    .RunAsync();
```

**Stream ownership:** path-based factories own the file stream they open and close
it when the run finishes — on success **and** failure. Stream-based factories leave
the caller's stream alone (honouring `XmlSingleStream…Options.LeaveOpen`), so the
caller controls its lifetime.

### Compressed streams (`.xml.gz`)

Every extractor and loader works against a plain `Stream`, so compression is
transparent — wrap the underlying stream in a `GZipStream` (or any
`System.IO.Compression` codec):

```csharp
using System.IO.Compression;

// Write gzip-compressed XML. LeaveOpen = false lets the loader dispose the
// GZipStream when the load completes, flushing the gzip footer.
using (var file = File.Create("people.xml.gz"))
using (var gzip = new GZipStream(file, CompressionMode.Compress))
{
    var loader = new XmlSingleStreamLoader<Person>(gzip);
    await loader.LoadAsync(people);
}

// Read it back — decompress on the way in.
using (var file = File.OpenRead("people.xml.gz"))
using (var gunzip = new GZipStream(file, CompressionMode.Decompress))
{
    var extractor = new XmlSingleStreamExtractor<Person>(gunzip);
    await foreach (var person in extractor.ExtractAsync())
    {
        // ...
    }
}
```

See the runnable `CompressedStreamRoundTripAsync` example in
[`examples/Wolfgang.Etl.Xml.Examples`](examples/Wolfgang.Etl.Xml.Examples/Program.cs).

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| Single-stream XML | Read/write multiple items from/to a single XML document with a root element wrapper |
| Multi-stream XML | Read/write one item per XML stream (one file per record) |
| Streaming deserialization | Uses `XmlReader` for memory-efficient forward-only parsing |
| Progress reporting | Built-in `IProgress<XmlReport>` support with configurable reporting intervals |
| Skip and maximum | `SkipItemCount` and `MaximumItemCount` for paging through large XML sources |
| Custom XML settings | Accept `XmlReaderSettings` and `XmlWriterSettings` for full control over XML behavior |
| Compressed streams | Works over any `Stream`, so gzip/deflate/Brotli is transparent — wrap in `GZipStream` for `.xml.gz` |
| Structured logging | High-performance `LoggerMessage`-based logging with categorized event IDs |
| Multi-TFM | Targets .NET Framework 4.6.2+, .NET Standard 2.0, .NET 8.0, and .NET 10.0 |

### Extractors

- **`XmlSingleStreamExtractor<T>`** — Extracts items from a single XML stream containing a root element with child elements (e.g. `<ArrayOfPerson><Person/>...</ArrayOfPerson>`).
- **`XmlMultiStreamExtractor<T>`** — Extracts items from multiple XML streams, one document per stream.

### Loaders

- **`XmlSingleStreamLoader<T>`** — Loads items into a single XML stream wrapped in a root element.
- **`XmlMultiStreamLoader<T>`** — Loads items into multiple XML streams via a factory function, one document per stream.
- **`XmlReport`** — Progress report returned via `IProgress<XmlReport>`. Properties: `CurrentItemCount`, `CurrentSkippedItemCount`.

### `EtlPipeline` factories

Class-named factories over the fluent `EtlPipeline` chain, so XML sources and sinks compose without hand-wiring an extractor/loader:

- **`XmlSingleStreamExtractor<T>(path)` / `(stream, options?)`** — seeds a pipeline from a single-root XML source. The path overload owns and closes the file stream.
- **`XmlMultiStreamExtractor<T>(streams)`** — seeds a pipeline from a sequence of single-document XML streams (one record each).
- **`XmlSingleStreamLoader<T>(path, options?)` / `(stream, options?)`** — terminates a pipeline into a single-root XML document. The path overload owns and closes the file stream.
- **`XmlMultiStreamLoader<T>(streamFactory)`** — terminates a pipeline, writing one XML document per record to a per-record stream.

### Constructor overloads

Each extractor and loader provides two public constructors (the first parameter varies by type):
- **`XmlSingleStreamExtractor<T>` / `XmlSingleStreamLoader<T>`** — `(stream)` or `(stream, settings, logger)`
- **`XmlMultiStreamExtractor<T>`** — `(streams)` or `(streams, settings, logger)` where `streams` is `IEnumerable<Stream>`
- **`XmlMultiStreamLoader<T>`** — `(streamFactory)` or `(streamFactory, settings, logger)` where `streamFactory` is `Func<T, Stream>`

### Progress reporting

```csharp
var extractor = new XmlSingleStreamExtractor<Person>(xmlStream);
extractor.ReportingInterval = 100; // Report every 100ms

var progress = new Progress<XmlReport>(report =>
    Console.WriteLine($"Progress: {report.CurrentItemCount} items, {report.CurrentSkippedItemCount} skipped")
);

var transformer = new TestTransformer<Person>();
var loader = new TestLoader<Person>(collectItems: true);

await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync(progress)));
```

### Skip and maximum item count

```csharp
var extractor = new XmlSingleStreamExtractor<Person>(xmlStream);
extractor.SkipItemCount = 10;     // Skip first 10 items
extractor.MaximumItemCount = 5;   // Then take 5 items

var transformer = new TestTransformer<Person>();
var loader = new TestLoader<Person>(collectItems: true);

await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync()));
// extractor.CurrentItemCount == 5, extractor.CurrentSkippedItemCount == 10
```

---

## 🎯 Supported Frameworks

This library targets:

- **.NET Framework:** 4.6.2

See the [NuGet package page](https://www.nuget.org/packages/Wolfgang.Etl.Xml/) for the authoritative per-TFM compatibility matrix.

## 🔍 Code Quality & Static Analysis

This project enforces **strict code quality standards** through **7 specialized analyzers** and custom async-first rules:

### Analyzers in Use

1. **Microsoft.CodeAnalysis.NetAnalyzers** - Built-in .NET analyzers for correctness and performance
2. **Roslynator.Analyzers** - Advanced refactoring and code quality rules
3. **AsyncFixer** - Async/await best practices and anti-pattern detection
4. **Microsoft.VisualStudio.Threading.Analyzers** - Thread safety and async patterns
5. **Microsoft.CodeAnalysis.BannedApiAnalyzers** - Prevents usage of banned synchronous APIs
6. **Meziantou.Analyzer** - Comprehensive code quality rules
7. **SonarAnalyzer.CSharp** - Industry-standard code analysis

### Async-First Enforcement

This library uses **`BannedSymbols.txt`** to prohibit synchronous APIs and enforce async-first patterns:

**Blocked APIs Include:**
- `Task.Wait()`, `Task.Result` - Use `await` instead
- `Thread.Sleep()` - Use `await Task.Delay()` instead
- Synchronous file I/O (`File.ReadAllText`) - Use async versions
- Synchronous stream operations - Use `ReadAsync()`, `WriteAsync()`
- `Parallel.For/ForEach` - Use `Task.WhenAll()` or `Parallel.ForEachAsync()`
- Obsolete APIs (`WebClient`, `BinaryFormatter`)

---

## Building from Source

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) (older SDKs work for restoring the older TFMs but the build/test matrix targets the full range up to .NET 10.0)
- Optional: [PowerShell Core](https://github.com/PowerShell/PowerShell) for formatting scripts

### Build Steps

```bash
# Clone the repository
git clone https://github.com/Chris-Wolfgang/ETL-Xml.git
cd ETL-Xml

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release

# Run code formatting (PowerShell Core)
pwsh ./format.ps1
```

### Code Formatting

This project uses `.editorconfig` and `dotnet format`:

```bash
# Format code
dotnet format

# Verify formatting
dotnet format --verify-no-changes
```

See [README-FORMATTING.md](docs/README-FORMATTING.md) for detailed formatting guidelines.

### Building Documentation

This project uses [DocFX](https://dotnet.github.io/docfx/) to generate API documentation:

```bash
# Install DocFX (one-time setup)
dotnet tool install -g docfx

# Generate API metadata and build documentation
cd docfx_project
docfx metadata  # Extract API metadata from source code
docfx build     # Build HTML documentation

# Documentation is generated in the docs/ folder at the repository root
```

The documentation is automatically built and deployed to GitHub Pages when changes are pushed to the `main` branch.

**Local Preview:**
```bash
# Serve documentation locally (with live reload)
cd docfx_project
docfx build --serve

# Open http://localhost:8080 in your browser
```

**Documentation Structure:**
- `docfx_project/` - DocFX configuration and source files
- `docs/` - Generated HTML documentation (published to GitHub Pages)
- `docfx_project/index.md` - Main landing page content
- `docfx_project/docs/` - Additional documentation articles
- `docfx_project/api/` - Auto-generated API reference YAML files

---

## 🤝 Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for:
- Code quality standards
- Build and test instructions
- Pull request guidelines
- Analyzer configuration details

---

## Acknowledgments

- [Wolfgang.Etl.Abstractions](https://github.com/Chris-Wolfgang/ETL-Abstractions) — the base class framework this library builds on
- [Wolfgang.Etl.TestKit](https://github.com/Chris-Wolfgang/ETL-Test-Kit) — test doubles and contract test base classes for pipeline development
