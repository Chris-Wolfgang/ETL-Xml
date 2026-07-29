# ADR-003 Path-based factories own the stream; stream-based factories honour LeaveOpen

- **Status**: Accepted
- **Date**: 2026-07-28

---

## Context

The `EtlPipeline` XML factories (ADR-002) come in two flavours per direction:

- A **path** overload — `XmlSingleStreamExtractor<T>("in.xml")` / `XmlSingleStreamLoader<T>("out.xml")` — where the factory itself opens the file.
- A **stream** overload — `XmlSingleStreamExtractor<T>(stream, options)` — where the caller supplies the stream.

A stream opened by the factory has no other owner, so someone must close it, and it must be closed on **both** success and failure or the file handle leaks. A stream supplied by the caller may be reused after the run (e.g. a `MemoryStream` read back, or a long-lived stream), so the factory must not close it unless told to.

---

## Decision

Path-based factories own the file stream they open and close it when the run finishes — on success and on failure. Stream-based factories never close the caller's stream; they honour `XmlSingleStream…Options.LeaveOpen` (default `true` for the caller's stream), leaving lifetime control with the caller.

Mechanically: the source path factory constructs the extractor with `LeaveOpen = false` (so `XmlReader.CloseInput` disposes the stream when the reader is disposed); the sink path factory wraps the terminator with the Abstractions `DisposingOwned` helper so the owned stream is disposed after `RunAsync` completes or throws.

---

## Considered Options

### Option A: Caller always owns the stream, even for path overloads

- Con: A path factory that opens a file but does not close it leaks the handle — the caller never sees the stream, so it cannot close it.

### Option B: Factory always closes the stream

- Con: Breaks the common case of writing to a `MemoryStream` and reading it back, or feeding a long-lived caller-owned stream — the factory would dispose a stream the caller still needs.

### Option C: Path factories own+close; stream factories honour LeaveOpen (chosen)

- Pro: No handle leak — the only party that can close a factory-opened file does close it, in success and failure paths.
- Pro: Caller-supplied streams stay under caller control, matching every other extractor/loader in the library.
- Con: The two overloads have different ownership semantics, which must be documented (README "Stream ownership", XML doc `<remarks>`).

---

## Consequences

**Easier:**

- `…Extractor<T>("file.xml")` / `…Loader<T>("file.xml")` "just work" with no using-block around a stream the caller never sees.
- Round-tripping through a caller-owned `MemoryStream` is safe.

**Harder:**

- Contributors must keep the two ownership rules straight when adding overloads; the split is called out in code comments and README to reduce that risk.
