using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BenchmarkDotNet.Attributes;

namespace Wolfgang.Etl.Xml.Benchmarks;

[MemoryDiagnoser]
public class XmlMultiStreamExtractorBenchmarks
{
    private static readonly XmlSerializer Serializer = new(typeof(BenchmarkPerson));

    private byte[][] _xmlBuffers = null!;



    [Params(10, 100, 1000)]
    public int ItemCount { get; set; }



    [GlobalSetup]
    public void Setup()
    {
        _xmlBuffers = Enumerable.Range(0, ItemCount).Select(i =>
        {
            var stream = new MemoryStream();
            Serializer.Serialize
            (
                stream,
                new BenchmarkPerson
                {
                    FirstName = $"First{i}",
                    LastName = $"Last{i}",
                    Age = 20 + (i % 50),
                    Email = $"person{i}@example.com",
                    City = $"City{i % 20}",
                }
            );
            return stream.ToArray();
        }).ToArray();
    }



    [Benchmark]
    public async Task<int> ExtractAsync()
    {
        var streams = _xmlBuffers.Select(b => (Stream)new MemoryStream(b));
        var extractor = new XmlMultiStreamExtractor<BenchmarkPerson>(streams);

        var count = 0;
        await foreach (var _ in extractor.ExtractAsync())
        {
            count++;
        }

        return count;
    }
}
