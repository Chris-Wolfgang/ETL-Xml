# ADR-006 Record architecture decisions

- **Status**: Accepted
- **Date**: 2026-11-17

---

## Context

`Wolfgang.Etl.Xml` makes several choices that the code alone does not explain: the assembly version is pinned while the package version moves, the pipeline factories are plain extension methods where a reader might expect builders, stream ownership depends on which overload opened the stream, and blocking APIs are banned by an analyzer. Without the reason next to it, each of these reads as an accident that a later change might "tidy up".

The repository has kept such records in `docs/adr` since ADR-001. The practice itself was never written down, which is why this record carries a later number than the decisions it describes.

---

## Decision

We record each significant, non-obvious design decision as an Architecture Decision Record in `docs/adr`.

- One Markdown file per decision, named `ADR-NNN-short-title.md` with the next free number, copied from [TEMPLATE.md](TEMPLATE.md).
- Every ADR has a row in [index.md](index.md).
- An ADR starts as **Proposed** and becomes **Accepted** when the decision is agreed.
- A decision that changes gets a new ADR. The old one stays in place with its status changed to **Superseded by ADR-NNN**.

---

## Considered Options

### Option A: Leave the reasoning in commit messages and pull requests

- Pro: No extra file to write.
- Con: The reason is far from the code and has to be dug out of history, if it was written down at all.

### Option B: Keep ADRs next to the code (chosen)

- Pro: The reason is versioned with the code.
- Pro: Reversing a decision is explicit, and the earlier reasoning survives.
- Con: A decision worth recording costs a few minutes to write up.

---

## Consequences

**Easier:**

- A contributor can find out why something is the way it is without reading the history.
- The other ETL repositories keep their records the same way, so the habit carries across.

**Harder:**

- A record written after the fact, as this one and ADR-005 are, reconstructs the reasoning and can miss part of it.
- The index has to be kept in step with the files by hand.
