type: fix

`XmlSingleStreamLoader<T>` now derives a valid default root element name for generic record types (`G<int>` gives `ArrayOfGOfInt32`, as `XmlSerializer` would name it) instead of an invalid `ArrayOfG`1` that failed on the first write. Documentation corrected: the README gzip snippet now passes `LeaveOpen = false` as its comment says, and the `RootElementName` and loader constructor `ArgumentException` docs now list the invalid-local-name case.
