type: docs

The README and the class remarks of all four stages now state that `XmlSerializer` does synchronous I/O on the caller's stream inside `ExtractAsync` / `LoadAsync` (so streams that forbid synchronous I/O, such as ASP.NET Core bodies with `AllowSynchronousIO = false`, fail), with an in-memory buffering workaround; the README's "async-first" wording now says it covers the library's own code only.
