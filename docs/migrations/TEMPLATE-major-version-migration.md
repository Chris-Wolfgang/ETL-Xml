<!--
  Copy this file to `migrating-vN-to-vM.md` (e.g. `migrating-v1-to-v2.md`) during
  release prep for any release that removes, renames, or changes the behaviour of
  a public API. Fill in every section, delete the guidance comments, and link the
  finished guide from the GitHub Release notes.

  A migration guide is written BEFORE the release ships (as part of release prep),
  not after — the breaking changes are known while the PR is open.
-->

# Migrating from vN to vM

`Wolfgang.Etl.Xml` vM contains breaking changes. This guide lists what changed and how to update.

> **TL;DR** — one or two sentences: the headline breaking change and the single most common fix.

## Who is affected

_(State which consumers need to act. e.g. "Anyone constructing `XmlSingleStreamLoader<T>` directly" or "only callers of the removed `X` overload". If a consumer only uses the fluent `EtlPipeline` factories, say whether they're affected.)_

## Breaking-change inventory

| Change | Kind | What to do |
|--------|------|-----------|
| `OldType.OldMember` | Removed / Renamed / Behaviour | Use `NewMember` / see below |
| … | … | … |

## Before / after

### <Short name of the change>

**Before (vN):**

```csharp
// old code that no longer compiles or behaves differently
```

**After (vM):**

```csharp
// updated code
```

_(Repeat one before/after block per breaking change. Show the smallest realistic snippet.)_

## Behavioural changes (compile-clean but different at runtime)

_(List changes that still compile but behave differently — e.g. a default value changed, an exception is now thrown where none was before, disposal semantics changed. These are the dangerous ones because the compiler won't catch them.)_

- …

## Deprecation timeline

_(For anything shipped `[Obsolete]` rather than removed outright: which version marked it obsolete, which version removes it, and the replacement.)_

| API | Obsolete since | Removed in | Replacement |
|-----|----------------|-----------|-------------|
| … | vN | vM | … |

## Dependencies

_(Note any minimum-bump of `Wolfgang.Etl.Abstractions` or other packages this major requires, and whether a `net462` consumer needs a binding redirect — see [ADR-001](../adr/ADR-001-assemblyversion-pinned.md); a redirect is only needed if `AssemblyVersion` moved, which happens only on a deliberate major.)_
