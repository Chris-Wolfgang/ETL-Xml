# Architecture Decision Records

| Number | Title | Status | Date |
|--------|-------|--------|------|
| [ADR-001](ADR-001-assemblyversion-pinned.md) | AssemblyVersion pinned at 1.0.0.0 | Accepted | 2026-07-28 |
| [ADR-002](ADR-002-pipeline-factories-not-builders.md) | EtlPipeline XML factories are plain extension methods, not builder interfaces | Accepted | 2026-07-28 |
| [ADR-003](ADR-003-stream-ownership.md) | Path-based factories own the stream; stream-based factories honour LeaveOpen | Accepted | 2026-07-28 |
| [ADR-004](ADR-004-trim-and-native-aot.md) | Trim and Native AOT: annotate the XmlSerializer boundary, keep the record's members | Proposed | 2026-10-30 |
| [ADR-005](ADR-005-async-only-io-via-banned-symbols.md) | Async-only I/O enforced through BannedSymbols.txt | Accepted | 2026-11-17 |
| [ADR-006](ADR-006-record-architecture-decisions.md) | Record architecture decisions | Accepted | 2026-11-17 |

---

To add a new ADR, copy [TEMPLATE.md](TEMPLATE.md), assign the next number, and add a row to this table.
