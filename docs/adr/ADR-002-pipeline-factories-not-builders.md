# ADR-002 EtlPipeline XML factories are plain extension methods, not builder interfaces

- **Status**: Accepted
- **Date**: 2026-07-28

---

## Context

Issue #66 added class-named factories that plug XML sources and sinks into the fluent `EtlPipeline` chain — `EtlPipeline.Create().XmlSingleStreamExtractor<T>("in.xml").XmlSingleStreamLoader<T>("out.xml").RunAsync()`. Two shapes existed across the sibling ETL packages for expressing per-factory configuration (reader/writer settings, `leaveOpen`, root element name):

1. A fluent builder interface (`IXmlExtractorBuilder<T>` with `.WithSettings(...)`, `.Encoding(...)`, materialized at first operator) — the shape ETL-FixedWidth and ETL-Csv adopted.
2. Plain extension-method factories that take an options object parameter — the shape ETL-Json shipped.

---

## Decision

We will use plain extension-method factories with an optional `XmlSingleStream…Options` parameter, matching the ETL-Json convention. No builder interfaces are introduced.

---

## Considered Options

### Option A: Builder interfaces (`IXml…Builder<T>`)

- Pro: Discoverable inline configuration via chained `.WithX()` calls.
- Pro: Consistent with ETL-FixedWidth / ETL-Csv.
- Con: Introduces new public interfaces + internal builder types + deferred-materialization semantics (setters-after-materialize must throw) — a large public surface for what XML needs.
- Con: XML configuration is already modelled by `XmlReaderSettings` / `XmlWriterSettings` and the existing `…Options` records; a builder would wrap types that already exist.

### Option B: Plain factories + options parameter (ETL-Json shape)

- Pro: Tiny public surface — two static extension classes, no new interfaces or builders.
- Pro: Reuses the extractor/loader constructors' existing `Options` records verbatim.
- Pro: Reads identically to the ETL-Json siblings, so a user moving between formats sees the same pattern.
- Con: Diverges from ETL-FixedWidth / ETL-Csv, so the family is not uniform on this point.

---

## Consequences

**Easier:**

- Minimal public API to maintain, document, and run through PublicAPI analyzers.
- Configuration flows through the same `Options` types the non-pipeline constructors already use.

**Harder:**

- The ETL family is split on the factory-configuration idiom (builders in FixedWidth/Csv, plain factories in Json/Xml); a future consolidation would have to reconcile the two.
