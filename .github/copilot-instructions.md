# Copilot Coding Agent Instructions

## Repository summary

`Wolfgang.Etl.Xml` is a shipping NuGet library: XML extractors and loaders built on the
`Wolfgang.Etl.Abstractions` ETL pattern. The public surface is `XmlSingleStreamExtractor<T>`,
`XmlMultiStreamExtractor<T>`, `XmlSingleStreamLoader<T>`, `XmlMultiStreamLoader<T>`, their options
records, the `XmlReport` progress type, and the `EtlPipeline` source/sink extension methods.

- **Target frameworks (src)**: `net462`, `net481`, `netstandard2.0`, `net5.0`, `net6.0`, `net7.0`,
  `net8.0`, `net10.0` (see `src/Wolfgang.Etl.Xml/Wolfgang.Etl.Xml.csproj`)
- **Unit-test frameworks**: `net462`-`net481`, `netcoreapp3.1`, `net5.0`-`net10.0`
- **Solution**: `ETL-Xml.slnx`
- **Language**: C# (`LangVersion` `latest`, set in `Directory.Build.props`)

## Layout

| Path | Contents |
|------|----------|
| `src/Wolfgang.Etl.Xml/` | The library, its `PublicAPI.*.txt` baselines and a src-only `.editorconfig` |
| `tests/Wolfgang.Etl.Xml.Tests.Unit/` | Unit tests, including the TestKit.Xunit contract tests (full TFM matrix) |
| `tests/Wolfgang.Etl.Xml.Tests.DocExamples/` | Compiles every `<example>` in the XML docs (net10.0) |
| `tests/Wolfgang.Etl.Xml.Tests.SourceLink/` | Checks the PDB's SourceLink map against GitHub (net10.0; needs the commit pushed) |
| `tests/Wolfgang.Etl.Xml.AotSmoke/` | Native-AOT publish-and-run smoke app (not a test project; `aot.yaml` runs it) |
| `benchmarks/`, `examples/`, `samples/`, `tools/` | BenchmarkDotNet suite, runnable examples, shadow workloads, GC profiling workload |
| `docs/` | ADRs (`docs/adr/`), migration guides and CI/release notes; the docfx site lives in `docfx_project/` |
| `changelog/unreleased/` | Changelog fragments (see its `README.md`); `CHANGELOG.md` is assembled from them at release |

## Build and validate

Restore the pinned tools first. ReportGenerator, DevSkim, InspectCode and the rest are local tools in
`.config/dotnet-tools.json`:

```powershell
dotnet tool restore
dotnet build ETL-Xml.slnx -c Release
```

`TreatWarningsAsErrors` applies in Release, so always build Release before pushing.

**`pwsh scripts/build-pr.ps1` is the local mirror of CI.** It builds Release, runs every test project
on every TFM with coverage, enforces the coverage thresholds, and runs DevSkim and gitleaks. Run it
before opening a PR.

## Gates a PR must pass

- **Coverage** (`.github/workflows/pr.yaml` is the source of truth): production code at
  `CODECOV_MINIMUM`, every test assembly at `CODECOV_TEST_MINIMUM`, both set in the `env:` block at
  the top of `pr.yaml`. Gates only ratchet up, never down.
- **Changelog fragment**: a PR that changes `src/` adds one `changelog/unreleased/<name>.md`
  (`pwsh scripts/changelog.ps1 check`).
- **Public API**: new public members go in `src/Wolfgang.Etl.Xml/PublicAPI.Unshipped.txt`.
- **Protected files**: `protected-files.yaml` fails a PR that mixes protected files with other
  changes. Protected files are `.editorconfig` (any level), `Directory.Build.*`,
  `BannedSymbols.txt`, `coverlet.runsettings`, `.config/dotnet-tools.json`, `.gitleaks.toml`,
  `*.globalconfig`, `*.ruleset`, `*.DotSettings`, `.github/workflows/*`,
  `.github/license-audit/*.json`, and the CI scripts `scripts/build-pr.ps1`,
  `scripts/changelog.ps1`, `scripts/tfm-parity.ps1` and `scripts/third-party-notices.ps1`. Put
  changes to them in their own PR.
- **Analyzers / security**: Roslyn analyzers from `Directory.Build.props`, ReSharper InspectCode,
  CodeQL (`codeql.yaml`), DevSkim, semgrep, gitleaks and zizmor (`actions-audit.yaml`).

## Workflows

`pr.yaml` (build, tests, coverage, InspectCode), `protected-files.yaml` (protected-file guard),
`release.yaml` (pack, validate and publish through NuGet Trusted Publishing), `docfx.yaml` (docs
site), plus scheduled or targeted checks: `codeql`, `semgrep`, `scorecard`, `actions-audit`, `aot`,
`fuzz`, `stryker`, `sbom`, `license-audit`, `sourcelink`, `reproducible-build`,
`cross-platform-differential`, `build-all-versions`, `benchmarks`, `pr-benchmarks`, `shadow` and
`security-alerts`.

## Conventions

- Allman braces, 4-space indentation, file-scoped namespaces, `var` where the type is obvious.
- Public members carry `///` XML docs. Examples in `<example><code>` blocks must compile (the
  DocExamples tests enforce it).
- Test names follow `MethodUnderTest_when_condition_expected_result`.
- All I/O is async. The banned-API analyzer (`BannedSymbols.txt`) rejects the synchronous file and
  stream APIs.
- Issue templates are YAML: `BUG_REPORT.yaml`, `feature_request.yaml`, `maintenance-task.yaml`.
- `CONTRIBUTING.md` has the contributor workflow. `SECURITY.md` covers vulnerability reporting and
  the release signing and publishing model.
