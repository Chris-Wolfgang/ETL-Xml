type: fix

`XmlMultiStreamExtractor` now throws `InvalidOperationException` ("The stream sequence yielded null for stream number N.") when the caller's stream sequence contains a `null` element, instead of a `NullReferenceException`, matching the multi-stream loader's null-stream error.
