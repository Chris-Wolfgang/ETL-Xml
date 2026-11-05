type: fix

The path-based `XmlSingleStreamLoader<T>(path, options)` pipeline sink now closes the file it opened when the loader rejects its options (for example an empty `RootElementName`), instead of leaking the handle.
