# Introduction

Wolfgang.Etl.Xml provides extractors and loaders for reading and writing XML files, built on [Wolfgang.Etl.Abstractions](https://github.com/Chris-Wolfgang/ETL-Abstractions).

## Overview

This library implements the Extract-Transform-Load (ETL) pattern for XML data. It provides four components that plug into any Wolfgang.Etl pipeline:

- **XmlSingleStreamExtractor&lt;T&gt;** — Reads multiple items from a single XML document with a root element wrapper (e.g. `<ArrayOfPerson><Person/>...</ArrayOfPerson>`).
- **XmlSingleStreamLoader&lt;T&gt;** — Writes multiple items into a single XML document with a root element wrapper.
- **XmlMultiStreamExtractor&lt;T&gt;** — Reads items from multiple XML streams, one document per stream.
- **XmlMultiStreamLoader&lt;T&gt;** — Writes items to multiple XML streams via a factory function, one document per stream.

## Key Features

- **Streaming deserialization** — Uses `XmlReader` for memory-efficient forward-only parsing of large XML files.
- **Progress reporting** — Built-in `IProgress<XmlReport>` support with configurable reporting intervals.
- **Skip and maximum** — `SkipItemCount` and `MaximumItemCount` properties for paging through large XML sources.
- **Custom XML settings** — Accepts `XmlReaderSettings` and `XmlWriterSettings` for full control over XML behavior.
- **Structured logging** — High-performance `LoggerMessage`-based logging with categorized event IDs.
- **Multi-TFM support** — Targets .NET Framework 4.6.2+, .NET Standard 2.0, .NET 8.0, and .NET 10.0.

## Getting Help

If you need help with Wolfgang.Etl.Xml, please:

- Check the [Getting Started](getting-started.md) guide
- Review the [API Reference](../api/index.md)
- Visit the [GitHub repository](https://github.com/Chris-Wolfgang/ETL-Xml)
- Open an issue on [GitHub Issues](https://github.com/Chris-Wolfgang/ETL-Xml/issues)
