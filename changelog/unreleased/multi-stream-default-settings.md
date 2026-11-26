type: fix

`XmlMultiStreamExtractor` and `XmlMultiStreamLoader` now use the same defaults as the single-stream stages when no `ReaderSettings` / `WriterSettings` are given: DTDs are prohibited and no resolver is used on every target framework (on .NET Framework the old default accepted a DOCTYPE), and each destination is written as UTF-8 with a byte-order mark and indented. Output bytes of the default multi-stream loader change (BOM added); supplying settings no longer switches to a different code path.
