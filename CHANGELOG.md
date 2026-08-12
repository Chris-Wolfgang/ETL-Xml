# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.7.0] - 2026-08-12

### Fixed

- **Data loss: a record following an element that deserialized to `null` was silently dropped.**
  `XmlSingleStreamExtractor<T>` recorded "the reader is already advanced" only after a successful
  deserialize, so a `null` result (for example an `xsi:nil` element) fell through to another
  `ReadAsync()` and consumed the *next* sibling's start tag. Given `<nil/>`, `Bob`, `Carol` the
  extractor returned only `Carol`. It now returns `Bob` and `Carol`. Found by mutation testing (#127).
- **`LeaveOpen` was ignored by the `(stream, logger)` constructors, closing the caller's stream.**
  `XmlSingleStreamExtractor<T>` and `XmlSingleStreamLoader<T>` initialized their fields
  independently in each overload, and the two-parameter logger overloads never assigned the
  backing field — so it defaulted to `false` and the stream was closed on completion, the opposite
  of the documented `LeaveOpen = true`. Every constructor now delegates to a single private
  initializer, so the default cannot diverge per overload again.

### Added

- **Canonical constructors that accept options and a logger together.** Previously a logger could
  only be supplied by giving up the options record or by also passing `XmlReaderSettings` /
  `XmlWriterSettings`. New overloads take `(source, options, logger)` with `logger` optional:
  - `XmlSingleStreamExtractor<T>(Stream, XmlSingleStreamExtractorOptions?, ILogger<...>?)`
  - `XmlSingleStreamLoader<T>(Stream, XmlSingleStreamLoaderOptions?, ILogger<...>?)`
  - `XmlSingleStreamLoader<T>(IBufferWriter<byte>, XmlSingleStreamLoaderOptions?, ILogger<...>?)`

  Purely additive — existing constructors are unchanged and still bind exactly as before. They
  become redundant and are scheduled to be marked `[Obsolete]` in 0.8.0 and removed in 0.9.0
  (see the deprecation tracking issue); nothing is deprecated in this release, so upgrading
  cannot break a build that treats warnings as errors.

## [0.6.0] - 2026-08-10

### Added

- `IBufferWriter<byte>` loader overloads (#8) for zero intermediate buffering. `XmlSingleStreamLoader<T>` accepts an `IBufferWriter<byte>` and `XmlMultiStreamLoader<T>` accepts a per-item `Func<TRecord, IBufferWriter<byte>>` — serialized bytes flow straight into the caller's buffer writer (e.g. a `System.IO.Pipelines.PipeWriter` or `ArrayBufferWriter<byte>`) through an internal write-only stream adapter, avoiding a `MemoryStream` round-trip. Output is byte-for-byte identical to the `Stream` overloads.
- Built-in OpenTelemetry / `System.Diagnostics.Metrics` instrumentation (#12). Every extractor and loader emits to the **`Wolfgang.Etl.Xml`** meter — counters `wolfgang.etl.xml.items.extracted` / `.loaded` / `.skipped` / `.errored` and the `wolfgang.etl.xml.operation.duration` histogram (ms) — each tagged with `etl.operation` (`extract`/`load`), `etl.component` (`XmlSingleStream`/`XmlMultiStream`), and `etl.record_type`. Subscribe with a `MeterListener` or OpenTelemetry; instruments are no-ops (zero measurable overhead) when no listener is registered, and no configuration is required from the caller.

## [0.5.0] - 2026-08-09

### Added

- Per-item error handling / dead-lettering on the multi-stream extractor and loader (#11). `XmlMultiStreamExtractor<T>` and `XmlMultiStreamLoader<T>` now honour the assignable `ErrorPolicy` inherited from the Abstractions base stages (0.21+): assign one of `Wolfgang.Etl.ErrorPolicies.ItemErrorPolicy`'s ready-made policies (`Skip`, `SkipAndLog`, `SkipAndDeadLetter`, `SkipDeadLetterAndLog`) to skip or dead-letter a stream/record that fails to deserialize/serialize and keep going, with the count surfaced via `CurrentErrorItemCount`. The default remains fail-fast. The single-stream classes keep fail-fast semantics (a shared streaming document cannot resume mid-record) — use the multi-stream variants for per-record error capture.
- Dry-run support on the loaders (#176). `XmlSingleStreamLoader<T>` and `XmlMultiStreamLoader<T>` implement `ISupportDryRun`: set `IsDryRun = true` to enumerate the source, honour `SkipItemCount` / `MaximumItemCount`, advance progress counters, and log exactly as a real load, but write nothing to the output stream(s) (the single-stream loader emits no document at all; the multi-stream loader never invokes the destination-stream factory). Defaults to `false` (fail-safe: real writes).

## [0.4.0] - 2026-08-07

### Changed

- Marked the public extractor/loader constructors (`XmlSingleStreamExtractor<T>`, `XmlMultiStreamExtractor<T>`, `XmlSingleStreamLoader<T>`, `XmlMultiStreamLoader<T>`) with `[RequiresUnreferencedCode]` (#135): the library (de)serializes via `System.Xml.Serialization.XmlSerializer`, which relies on runtime reflection / `Reflection.Emit` and is not trim / Native-AOT safe. Consumers running trim or AOT analysis now get an explicit diagnostic at these call sites rather than a silent runtime failure.
- Upgraded `Wolfgang.Etl.Abstractions` 0.16.1 → 0.20.0 (adds the use-after-dispose contract, per-item error-handling hooks, and the overflow-safe `long` `EtlPipelineProgress` counters). Test project upgraded to `Wolfgang.Etl.TestKit` / `Wolfgang.Etl.TestKit.Xunit` 0.13.0.

- Documentation for customizing the serialized XML (#14): a README "Customizing the serialized XML" section covering the standard `System.Xml.Serialization` attributes the extractors/loaders honour (`XmlRoot`, `XmlElement`, `XmlAttribute`, `XmlIgnore`, …), and a note that attribute-free customization via `XmlAttributeOverrides` is not currently supported (the serializer is built from the type alone).
- Documentation, an example, and tests for XSD validation during extraction (#10): passing an `XmlReaderSettings` with `ValidationType.Schema` and a loaded `XmlSchemaSet` to `XmlSingleStreamExtractor<T>` validates the source against the schema as it is read. A violation surfaces from `ExtractAsync` as an `InvalidOperationException` wrapping the `XmlSchemaValidationException`.

### Fixed

- `XmlSingleStreamLoader` and `XmlMultiStreamLoader` now honour a `CancellationToken` that is already cancelled when `LoadAsync` is called by consuming nothing from the source — the cancellation is observed before the first item is pulled, matching the extractors and the `LoaderBase` contract in TestKit 0.13.

## [0.3.0] - 2026-07-21

### Added

- `EtlPipeline` XML source/sink factories (#66): `XmlSingleStreamExtractor<T>`, `XmlMultiStreamExtractor<T>`, `XmlSingleStreamLoader<T>`, and `XmlMultiStreamLoader<T>` extension methods that plug XML sources and sinks straight into the fluent `EtlPipeline` chain (e.g. `EtlPipeline.Create().XmlSingleStreamExtractor<Person>("in.xml").XmlSingleStreamLoader<Person>("out.xml").RunAsync()`). Path-based factories own and close the file stream (on success and failure); stream-based factories honour the caller's `LeaveOpen`.
- Documentation and a runnable example for reading/writing compressed (`.xml.gz`) streams by wrapping the underlying stream in a `GZipStream` (#13): README "Compressed streams" section, a Features-table row, and the `CompressedStreamRoundTripAsync` example in `examples/Wolfgang.Etl.Xml.Examples`.

### Changed

- Bumped `Wolfgang.Etl.Abstractions` 0.15.0 → 0.16.0 (ships `EtlPipeline`).

## [0.2.2] - 2026-07-06

### Changed

- Dependabot bump: dotnet-dependencies group (8 packages).
## [0.2.1] - Unreleased

Canonical maintenance round + binding-stability fix. Public API and runtime behavior unchanged from 0.2.0.

### Added
- PublicApiAnalyzers scaffolding (baseline file deferred to a follow-up IDE-fix pass).
- Canonical NuGet package metadata (Authors, Copyright, SourceLink, snupkg symbols).
- Stryker mutation-testing workflow.
- Coverage report published alongside generated docs.
- CodeQL `security-extended` query pack.
- `versions.json` preservation guard on docs publish.
- BenchmarkDotNet → gh-pages chart workflow (when `benchmarks/` exists).
- `docs/DOCFX-VERSION-PICKER.md`.
- `verify-docs-build` job in `release.yaml`.

### Changed
- Fleet template-drift sync against `repo-template`.
- `<Nullable>enable</Nullable>` consolidated into `Directory.Build.props`.
- Dependabot now tracks the `github-actions` ecosystem.
- Analyzer `PackageReference`s centralized in `Directory.Build.props`.
- Removed post-setup bootstrap files (`REPO-INSTRUCTIONS.md`, `scripts/setup.ps1`, `Setup-BranchRuleset.ps1`, `Setup-GitHubPages.ps1`).

### Fixed
- Restored explicit `<AssemblyVersion>1.0.0.0</AssemblyVersion>` and prerelease-safe `<FileVersion>` so .NET Framework consumers keep binding stability across patch releases.
- Duplicate `verify-docs-build:` job key in `release.yaml`.
- Garbled "Code Quality" heading in `README.md`.

## [0.2.0] - 2026-04-28

### Added
- `leaveOpen` and `rootElementName` parameters on the single-stream extractor and loader ([#45](https://github.com/Chris-Wolfgang/ETL-Xml/pull/45)).
- `(stream, logger)` constructors on all four XML classes ([#48](https://github.com/Chris-Wolfgang/ETL-Xml/pull/48)).
- SBOM generation in the release workflow ([#20](https://github.com/Chris-Wolfgang/ETL-Xml/pull/20)).
- `gitleaks` scanning + concurrency in the PR workflow ([#21](https://github.com/Chris-Wolfgang/ETL-Xml/pull/21), [#25](https://github.com/Chris-Wolfgang/ETL-Xml/pull/25)).
- `SECURITY.md` ([#32](https://github.com/Chris-Wolfgang/ETL-Xml/pull/32)).
- `netcoreapp3.1` to the test `TargetFrameworks` matrix ([#29](https://github.com/Chris-Wolfgang/ETL-Xml/pull/29)).
- Local `build-pr.ps1` script that mirrors the PR checks ([#31](https://github.com/Chris-Wolfgang/ETL-Xml/pull/31)).
- `setup.ps1` carried over from `repo-template` ([#53](https://github.com/Chris-Wolfgang/ETL-Xml/pull/53)).

### Changed
- `ILogger` parameters are now optional rather than required ([#36](https://github.com/Chris-Wolfgang/ETL-Xml/pull/36)).
- Removed the 2-parameter `(stream, logger)` constructors that caused overload ambiguity ([#38](https://github.com/Chris-Wolfgang/ETL-Xml/pull/38)) — re-introduced cleanly in [#48](https://github.com/Chris-Wolfgang/ETL-Xml/pull/48).
- Synced `pr.yaml` and `release.yaml` with `repo-template` ([#27](https://github.com/Chris-Wolfgang/ETL-Xml/pull/27), [#28](https://github.com/Chris-Wolfgang/ETL-Xml/pull/28)).
- Release-workflow actions upgraded to Node 24 ([#57](https://github.com/Chris-Wolfgang/ETL-Xml/pull/57)).
- Analyzer `PackageReference`s moved out of `Directory.Build.props` into individual csproj files ([#46](https://github.com/Chris-Wolfgang/ETL-Xml/pull/46), [#47](https://github.com/Chris-Wolfgang/ETL-Xml/pull/47)).
- Renamed `codeql.yml` → `codeql.yaml` ([#52](https://github.com/Chris-Wolfgang/ETL-Xml/pull/52)).
- Version bumped to 0.2.0 with refreshed `Wolfgang.Etl.*` dependencies ([#56](https://github.com/Chris-Wolfgang/ETL-Xml/pull/56)).

### Fixed
- Consolidated gh-pages deploys into a single commit ([#17](https://github.com/Chris-Wolfgang/ETL-Xml/pull/17)).
- SonarAnalyzer errors that were blocking every PR ([#40](https://github.com/Chris-Wolfgang/ETL-Xml/pull/40)).
- `Microsoft.NET.Test.Sdk` incompatibility with `netcoreapp3.1` ([#42](https://github.com/Chris-Wolfgang/ETL-Xml/pull/42)).
- Stale documentation and missing `README-FORMATTING.md` ([#44](https://github.com/Chris-Wolfgang/ETL-Xml/pull/44)).
- Test analyzer errors (MA0074, S108, AsyncFixer01) ([#41](https://github.com/Chris-Wolfgang/ETL-Xml/pull/41)).
- `.gitleaks.toml` regex quoting ([#33](https://github.com/Chris-Wolfgang/ETL-Xml/pull/33)).

### Security
- Added `.gitleaks.toml` to scan for committed secrets ([#18](https://github.com/Chris-Wolfgang/ETL-Xml/pull/18)).

## [0.1.0] - 2026-03-24

Initial public release.

### Added
- `Wolfgang.Etl.Xml` library — extractors and loaders for the Wolfgang ETL pattern:
  - `XmlSingleStreamExtractor<T>` / `XmlSingleStreamLoader<T>` — root-element-wrapped multi-item documents.
  - `XmlMultiStreamExtractor<T>` / `XmlMultiStreamLoader<T>` — one document per stream.
- Benchmarks and example projects ([#2](https://github.com/Chris-Wolfgang/ETL-Xml/pull/2)).
- Multi-TFM targeting: `net462`, `net481`, `netstandard2.0`, `net8.0`, `net10.0`.

### Fixed
- Dropped `netcoreapp3.1` from the test TFM matrix where the CI image does not provide the SDK ([#16](https://github.com/Chris-Wolfgang/ETL-Xml/pull/16)).

[Unreleased]: https://github.com/Chris-Wolfgang/ETL-Xml/compare/v0.2.2...HEAD
[0.2.2]: https://github.com/Chris-Wolfgang/ETL-Xml/compare/v0.2.1...v0.2.2
[0.2.1]: https://github.com/Chris-Wolfgang/ETL-Xml/compare/v0.2.0...v0.2.1
[0.2.0]: https://github.com/Chris-Wolfgang/ETL-Xml/compare/v.0.1.0...v0.2.0
[0.1.0]: https://github.com/Chris-Wolfgang/ETL-Xml/releases/tag/v.0.1.0
