type: internal

On net5.0, net6.0 and net7.0 the buffer-writer loaders now write spans and memory straight into the `IBufferWriter<byte>` (they previously rented and copied an array), and net8.0+ also skips that copy for `WriteAsync(ReadOnlyMemory<byte>)`.
