type: fix

The path-based pipeline factories `XmlSingleStreamLoader<T>(path, options)` and `XmlSingleStreamExtractor<T>(path)` now open their file on first use instead of when the pipeline is composed. A dry run no longer truncates or creates the destination file, an abandoned or pre-cancelled pipeline no longer leaks a locked input file, and a missing input file now surfaces as `FileNotFoundException` when the pipeline runs rather than when it is composed.
