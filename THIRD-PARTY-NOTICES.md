# Third-Party Notices

`Wolfgang.Etl.Xml` ships the runtime dependencies listed below.
`license-audit.yaml` audits the shipped package's transitive dependency
licences on every PR that touches a `.csproj`, plus weekly. Regenerate this
file's table by hand (from `dotnet-project-licenses`'s console output — see the
command below) and commit it whenever the dependency graph changes.

## Wolfgang.Etl.Xml

| Package | Version | License |
|---------|---------|---------|
| [Microsoft.Bcl.AsyncInterfaces](https://www.nuget.org/packages/Microsoft.Bcl.AsyncInterfaces/) | 10.0.12 | [MIT](https://licenses.nuget.org/MIT) |
| [Microsoft.Extensions.Logging.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/) | 10.0.12 | [MIT](https://licenses.nuget.org/MIT) |
| [System.Diagnostics.DiagnosticSource](https://www.nuget.org/packages/System.Diagnostics.DiagnosticSource/) | 10.0.12 | [MIT](https://licenses.nuget.org/MIT) |

> Microsoft.Bcl.AsyncInterfaces supplies `IAsyncEnumerable<T>` /
> `IAsyncDisposable` on the down-level targets; on net8.0+ those types are part
> of the framework. System.Diagnostics.DiagnosticSource is referenced only on
> net462 / net481 / netstandard2.0 (in-box everywhere else).

## First-party dependencies

`Wolfgang.Etl.Abstractions` (MIT) is also a shipped runtime dependency, but it
is authored and published by this project's owner rather than a third party, so
it is recorded here for completeness rather than listed in the table above.

## Copyright

- Microsoft.Bcl.AsyncInterfaces, Microsoft.Extensions.Logging.Abstractions,
  System.Diagnostics.DiagnosticSource —
  © Microsoft Corporation. All rights reserved.

## Baseline scan

Generated from:

```
dotnet-project-licenses --input src/Wolfgang.Etl.Xml/Wolfgang.Etl.Xml.csproj
```

against the src project's shipped (non-analyzer, non-test) dependency graph.
Analyzer packages are `PrivateAssets=all` build-time-only and are never
distributed in the NuGet package, so they are deliberately out of scope.
