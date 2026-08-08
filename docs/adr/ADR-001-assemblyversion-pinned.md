# ADR-001 AssemblyVersion pinned at 1.0.0.0

- **Status**: Accepted
- **Date**: 2026-07-28

---

## Context

`Wolfgang.Etl.Xml` ships a `net462` target, so it runs on the .NET Framework CLR, which resolves assembly references by their full four-part `AssemblyVersion` (the *binding identity*). If `AssemblyVersion` changed on every release, a .NET Framework consumer compiled against, say, `1.2.0.0` would fail to load `1.3.0.0` at runtime with a `FileLoadException` unless it added an `<bindingRedirect>` — one per package, per bump, forever.

The NuGet package version, by contrast, moves every release (`0.1.0`, `0.2.0`, `0.3.0`, …) and is what consumers actually select.

---

## Decision

We will pin `<AssemblyVersion>` at `1.0.0.0` and only change it on a deliberate breaking API change (a new major line). `<FileVersion>` and the informational version carry the real release version; the NuGet `<Version>` is the number consumers see.

---

## Considered Options

### Option A: Let the SDK derive AssemblyVersion from `<Version>`

- Pro: One number to bump; no special-casing.
- Con: Every minor/patch ships a different binding identity — .NET Framework consumers need a binding redirect on every upgrade or hit `FileLoadException`.
- Con: This regression actually shipped elsewhere in the fleet (DateTime-Extensions v1.3.0) before being reverted; it is a known foot-gun.

### Option B: Pin AssemblyVersion at 1.0.0.0, bump FileVersion + NuGet Version per release

- Pro: Stable binding identity across the whole `0.x`/`1.x` line — no redirects needed for minor/patch upgrades.
- Pro: Matches the convention of NodaTime, Newtonsoft.Json, and AutoMapper.
- Pro: `FileVersion` / informational version still record the exact build for diagnostics.
- Con: The three version fields can look inconsistent to someone who doesn't know the convention (mitigated by the csproj comment and this ADR).

---

## Consequences

**Easier:**

- .NET Framework consumers upgrade across minor/patch releases with no binding redirects.
- Consistent with the rest of the Wolfgang ETL family.

**Harder:**

- A genuine breaking change must remember to bump `AssemblyVersion` deliberately; it will not happen automatically.
