type: fix

`XmlMultiStreamExtractor` now records the `wolfgang.etl.xml.items.errored` metric when the error policy skips or dead-letters a stream, matching `XmlMultiStreamLoader`.
