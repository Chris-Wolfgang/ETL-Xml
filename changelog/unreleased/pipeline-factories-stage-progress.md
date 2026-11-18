type: feature

The `EtlPipeline` XML factories (`XmlSingleStreamExtractor`, `XmlMultiStreamExtractor`, `XmlSingleStreamLoader`, `XmlMultiStreamLoader`) gain overloads that take an `IProgress<XmlReport>`, so a pipeline stage reports its own typed progress.
