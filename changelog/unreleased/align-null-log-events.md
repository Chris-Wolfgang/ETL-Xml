type: fix

Two log events now match their counterparts: a multi-stream document that deserializes to null logs at `Debug` (was `Warning`), the same level as the single-stream extractor's null-element skip, and the null-stream-factory error logs the one-based item number (`{ItemNumber}`, was the zero-based `{StreamIndex}`), the same number the thrown `InvalidOperationException` reports.
