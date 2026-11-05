# Reproducing & verifying the build

`Wolfgang.Etl.Xml` is built **deterministically** — `Deterministic` +
`ContinuousIntegrationBuild` + SourceLink — so the compiled assembly is a pure
function of the source at a given commit. That means **you don't have to trust our
CI**: you can rebuild the library yourself from the published tag and confirm, byte
for byte, that the assembly we shipped is the one the source produces.

Our CI already proves the build is reproducible *across operating systems* —
`.github/workflows/reproducible-build.yaml` packs the library on Ubuntu and on Windows
and fails if any target framework's `.dll` or `.pdb` hash differs between the two
(#138, #456). Two inputs used to differ per OS and are now pinned:
`Directory.Build.targets` rewrites the SDK-generated sources (`AssemblyInfo.cs`,
`*.AssemblyAttributes.cs`, `GlobalUsings.g.cs`) to LF before compilation, and
`Directory.Build.props` makes the .NET Framework builds compile against the
`Microsoft.NETFramework.ReferenceAssemblies` package on every OS. This page is the
**consumer side**: how *you* verify it independently (#147).

## What is verified

The guarantee covers the **compiled assemblies and their symbols**
(`Wolfgang.Etl.Xml.dll` and `.pdb`, every target framework). It does **not** cover
the package files: NuGet stamps each zip entry of a `.nupkg` / `.snupkg` with the pack
time, so the same commit gives a different package file on every pack, even on one
machine. Verify the assembly inside the package, not the package envelope.

Each release attaches a **`reproducible-build-manifest.json`** listing the expected
`sha256` of the shipped assembly for every target framework, e.g.:

```json
{
  "package": "Wolfgang.Etl.Xml",
  "version": "X.Y.Z",
  "algorithm": "sha256",
  "assemblies": {
    "lib/net10.0/Wolfgang.Etl.Xml.dll": "d34db33f…",
    "lib/net8.0/Wolfgang.Etl.Xml.dll": "…"
  }
}
```

## Verify a published package

You need the same **major .NET SDK** the release was built with (see the SDK line in
`release.yaml` — `10.0.x` for current releases — plus `global.json` if the repo pins one).

```bash
# 1. Get the exact published assembly hash (from the package on nuget.org)
dotnet nuget locals -c all >/dev/null   # optional: clean caches
unzip -p Wolfgang.Etl.Xml.<version>.nupkg \
  lib/net10.0/Wolfgang.Etl.Xml.dll | sha256sum

# 2. Rebuild from source at the tag and hash your own output
git clone https://github.com/Chris-Wolfgang/ETL-Xml
cd ETL-Xml
git checkout v<version>
dotnet build src/Wolfgang.Etl.Xml/Wolfgang.Etl.Xml.csproj \
  -c Release -f net10.0 -p:ContinuousIntegrationBuild=true
sha256sum src/Wolfgang.Etl.Xml/bin/Release/net10.0/Wolfgang.Etl.Xml.dll

# 3. Compare — the two hashes, and the value in reproducible-build-manifest.json,
#    must all match.
```

A match means the published binary is exactly what the tagged source compiles to,
on your machine, under your control.

## If the hashes differ

First rule out toolchain drift: a different SDK **feature band**, or a different .NET
runtime running the compiler (its version string is recorded in the PDB), changes the
output. The operating system no longer does. Confirm you used the SDK named in the
release and built with `-p:ContinuousIntegrationBuild=true`.

If they still differ with matching tooling, that is a genuine reproducibility
discrepancy worth reporting: open an issue titled `reproducibility: <version> hash
mismatch` with your **SDK version** (`dotnet --version`), **OS**, the **two hashes**,
and the target framework. It is treated as a security-relevant report.

## Third-party verification attestations

Independent verifiers are encouraged to publish their result following the
[Reproducible Builds project](https://reproducible-builds.org/) conventions (a
signed statement that "version X's assembly rebuilds to hash H"), for example via
[vouchsafe.io](https://vouchsafe.io/) or an [`actions/attest-build-provenance`](https://github.com/actions/attest-build-provenance)
attestation over your own rebuild. Link such an attestation on the discrepancy/verification
issue so others can find it.
