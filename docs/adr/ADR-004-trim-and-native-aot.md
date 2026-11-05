# ADR-004 Trim and Native AOT: annotate the XmlSerializer boundary, keep the record's members

- **Status**: Proposed
- **Date**: 2026-10-30

---

## Context

Issue #293 asked for `IsAotCompatible` / `IsTrimmable`. Every extractor and loader serializes records with
`System.Xml.Serialization.XmlSerializer`, which carries `[RequiresUnreferencedCode]` and, on .NET 8+,
`[RequiresDynamicCode]`.

Measured on the `src/` project with the trim, AOT and single-file analyzer categories raised to warning
(Release, before this change):

| TFM | IL2026 | IL3050 | Distinct sites |
|-----|-------:|-------:|----------------|
| net8.0 | 16 | 0 | 4 static `XmlSerializer` field initializers, 6 `Serialize`/`Deserialize` calls, 6 `EtlPipeline` factories |
| net10.0 | 16 | 10 | the same 16 sites; the 10 `XmlSerializer` sites also report IL3050 |

The public constructors already carried `[RequiresUnreferencedCode]`, but not `[RequiresDynamicCode]`, and
the six `EtlPipeline` factories carried neither.

A Native AOT publish of `tests/Wolfgang.Etl.Xml.AotSmoke` (Linux container, SDK 10.0.402) showed the real
cost: the round trip did **not** throw. `XmlSerializer` fell back to its reflection-based mode, but the
trimmer had removed the record's property getters and setters, so every record was written as an empty
`<Person />` and read back with default values. The smoke counted records only and reported success.

`XmlSerializer` has no source generator. `Microsoft.XmlSerializer.Generator` (sgen) emits a separate
assembly that is loaded by name at run time, which is not available under Native AOT.

---

## Decision

We will propagate the requirement instead of hiding it, and preserve what the serializer needs:

1. Every public constructor of the four stage types and every `EtlPipeline` factory carries
   `[RequiresUnreferencedCode]` **and** `[RequiresDynamicCode]`, with one shared message each
   (`XmlSerialization.UnreferencedCodeMessage` / `DynamicCodeMessage`).
2. The record type parameter (`TRecord` on the four stage types, `T` on the six factories) carries
   `[DynamicallyAccessedMembers(PublicParameterlessConstructor | PublicFields | PublicProperties)]`, so a
   trimmed or Native AOT application keeps the record's own public members.
3. All `XmlSerializer` calls go through one internal class, `XmlSerialization`. Its six methods suppress
   IL2026 / IL3050 with a justification: they are reached only from members of the four stage types, which
   cannot be instantiated without passing an annotated constructor. `XmlTrimAnnotationTests` holds that
   invariant, so a new unannotated public constructor or factory fails the test run.
4. The library declares `IsAotCompatible` on net8.0 and net10.0 (the TFMs that have the analyzers).
5. The AOT smoke compares the round-tripped values, not only the count, and its publish is
   warnings-as-errors.

---

## Considered Options

### Option A: Annotations plus `[DynamicallyAccessedMembers]` (chosen)

- Pro: the analyzer reports every remaining risk at the consumer's call site, with an actionable message.
- Pro: top-level records round-trip under Native AOT (measured).
- Con: consumers that forward an unannotated generic `T` into these types get IL2091 in trim-analyzed builds.
- Con: nested, derived (`[XmlInclude]`) and collection element types are still not preserved automatically.

### Option B: Declare the package not applicable for AOT

- Pro: no public-surface change.
- Con: leaves the silent data loss in place for anyone who publishes trimmed or AOT.

### Option C: Write an XML source generator, or accept caller-supplied read/write delegates

- Pro: fully trim-safe with no reflection.
- Con: a new serializer to own and keep compatible with `XmlSerializer`'s output; a much larger change. It can
  be added later as an opt-in path without undoing this decision.

---

## Consequences

**Easier:**

- Trimmed and Native AOT consumers get a correct round trip for records with public members, and a warning
  that names what is still at risk.
- A new reflection path in the library is reported by the analyzers on net8.0+.

**Harder:**

- Consumers see IL3050 in addition to IL2026 at the construction site, and IL2091 when they pass an
  unannotated generic type parameter.
- The repository `.editorconfig` still lowers every analyzer to `suggestion`, so the trim and AOT analyzers
  report but do not fail the build until the `category-Trimming` / `category-AOT` / `category-SingleFile`
  severities are raised to `warning` (a protected-file follow-up).
