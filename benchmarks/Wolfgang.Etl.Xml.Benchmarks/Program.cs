using BenchmarkDotNet.Running;
using Wolfgang.Etl.Xml.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(XmlSingleStreamExtractorBenchmarks).Assembly).Run(args);
