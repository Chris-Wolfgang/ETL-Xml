# Architecture Decision Records

| Number | Title | Status | Date |
|--------|-------|--------|------|
| [0000](0000-record-architecture-decisions.md) | Record architecture decisions | Accepted | 2026-11-20 |
| [0001](0001-assemblyversion-pinned.md) | AssemblyVersion pinned at 1.0.0.0 | Accepted | 2026-07-28 |
| [0002](0002-pipeline-factories-not-builders.md) | EtlPipeline XML factories are plain extension methods, not builder interfaces | Accepted | 2026-07-28 |
| [0003](0003-stream-ownership.md) | Path-based factories own the stream; stream-based factories honour LeaveOpen | Accepted | 2026-07-28 |
| [0004](0004-trim-and-native-aot.md) | Trim and Native AOT: annotate the XmlSerializer boundary, keep the record's members | Proposed | 2026-10-30 |
| [0005](0005-async-only-io-via-banned-symbols.md) | Async-only I/O enforced through BannedSymbols.txt | Accepted | 2026-11-17 |
| 0006 | Record architecture decisions (moved to [0000](0000-record-architecture-decisions.md)) | Retired | 2026-11-23 |

---

To add a new ADR, copy [TEMPLATE.md](TEMPLATE.md), assign the next number, and add a row to this table.
