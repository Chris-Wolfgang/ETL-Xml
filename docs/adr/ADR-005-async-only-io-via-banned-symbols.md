# ADR-005 Async-only I/O enforced through BannedSymbols.txt

- **Status**: Accepted
- **Date**: 2026-11-17

---

## Context

Every extractor and loader in `Wolfgang.Etl.Xml` is a stage in an `IAsyncEnumerable<T>` pipeline. A blocking call on that path (`Task.Result`, `Thread.Sleep`, a synchronous `Stream.Read`) holds a thread-pool thread for the length of the I/O and can deadlock a consumer that runs under a single-threaded synchronization context. Such a call compiles without complaint and is easy to miss in review.

The repository already carries the mechanism: a `BannedSymbols.txt` at the root, read by `Microsoft.CodeAnalysis.BannedApiAnalyzers`. On the date of this ADR the list had the same 65 entries in all eight ETL repositories. What was missing was a record of the decision and of where it stops.

One fact is specific to this library: `System.Xml.Serialization.XmlSerializer` has no asynchronous API.

---

## Decision

We enforce the async-only rule with the banned-API analyzer instead of relying on review.

- `BannedSymbols.txt` bans `Task.Wait`, `Task.WaitAll`, `Task.WaitAny`, `Task<T>.Result`, `GetAwaiter().GetResult()`, `Thread.Sleep`, the synchronous `File` helpers, `Stream.Read` / `Write` / `Flush` / `CopyTo`, `FileStream.Read` / `Write`, `Parallel.For` / `ForEach` and the blocking `Console` reads. Each entry names its replacement. The list also bans some obsolete network and serialization APIs and some `DateTime` patterns that have nothing to do with blocking.
- `.editorconfig` sets `RS0030` to `error` for the shipped code, and to `none` for tests, benchmarks and examples.
- The library does its own stream work asynchronously: it positions the `XmlReader` with `ReadAsync` and flushes with `FlushAsync`. The `XmlSerializer.Serialize` and `Deserialize` calls themselves are synchronous, because there is no other form.
- A suppression needs a `#pragma` with a comment that says why. There is one: `BufferWriterStream.Write`, the `Stream` override that receives the serializer's synchronous writes.

---

## Considered Options

### Option A: Rely on code review and AsyncFixer

- Pro: Nothing to maintain.
- Con: AsyncFixer catches some patterns, not all of the banned APIs, and review misses them under time pressure.

### Option B: Ban the blocking APIs with an analyzer (chosen)

- Pro: A blocking call fails the build with a message that names the replacement.
- Pro: The reason for each ban sits next to the ban.
- Con: The list is maintained by hand and has to stay in step across the ETL repositories.

---

## Consequences

**Easier:**

- A direct blocking call cannot merge into shipped code.
- A contributor who reaches for a synchronous API is told what to use instead at build time.

**Harder:**

- The rule stops direct calls to the banned members only. `XmlSerializer` is not on the list and cannot be: while a `Serialize` or `Deserialize` call runs, it reads from or writes to the underlying reader, writer or stream synchronously. The library is async around the serializer, not inside it.
- `BannedSymbols.txt` is a protected file, so a change to the list goes in its own pull request.
- Tests, benchmarks and examples are exempt, so a blocking call there is not caught.

*Recorded after the fact: the rule was already in force when this ADR was written.*
