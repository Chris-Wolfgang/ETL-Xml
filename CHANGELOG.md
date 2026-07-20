# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `EtlPipeline` XML source/sink factories (#66): `XmlSingleStreamExtractor<T>`, `XmlMultiStreamExtractor<T>`, `XmlSingleStreamLoader<T>`, and `XmlMultiStreamLoader<T>` extension methods that plug XML sources and sinks straight into the fluent `EtlPipeline` chain (e.g. `EtlPipeline.Create().XmlSingleStreamExtractor<Person>("in.xml").XmlSingleStreamLoader<Person>("out.xml").RunAsync()`). Path-based factories own and close the file stream (on success and failure); stream-based factories honour the caller's `LeaveOpen`.

### Changed

- Bumped `Wolfgang.Etl.Abstractions` 0.15.0 → 0.16.0 (ships `EtlPipeline`).

### Deprecated

### Removed

### Fixed

### Security

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
