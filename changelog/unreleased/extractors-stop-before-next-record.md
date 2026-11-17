type: fix

`XmlSingleStreamExtractor<T>` no longer deserializes one record past `MaximumItemCount` (which could throw on a malformed record the caller never asked for), and `XmlMultiStreamExtractor<T>` no longer pulls — and so opens, for a lazy sequence such as `Select(File.OpenRead)` — one stream past `MaximumItemCount` or after cancellation, which leaked the stream.
