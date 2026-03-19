using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;

namespace Wolfgang.Etl.Xml.Benchmarks;

[MemoryDiagnoser]
public class XmlSingleStreamExtractorBenchmarks
{
    private static readonly XmlSerializer Serializer = new(typeof(BenchmarkPerson));

    private static readonly XmlSerializerNamespaces EmptyNamespaces =
        new(new[] { new XmlQualifiedName(name: "", ns: "") });

    private byte[] _xmlData = null!;



    [Params(10, 100, 1000)]
    public int ItemCount { get; set; }



    [GlobalSetup]
    public void Setup()
    {
        var stream = new MemoryStream();
        var settings = new XmlWriterSettings { Indent = false, CloseOutput = false };
        using var writer = XmlWriter.Create(stream, settings);

        writer.WriteStartDocument();
        writer.WriteStartElement("ArrayOfBenchmarkPerson");

        for (var i = 0; i < ItemCount; i++)
        {
            Serializer.Serialize
            (
                writer,
                new BenchmarkPerson
                {
                    FirstName = $"First{i}",
                    LastName = $"Last{i}",
                    Age = 20 + (i % 50),
                    Email = $"person{i}@example.com",
                    City = $"City{i % 20}",
                },
                EmptyNamespaces
            );
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();

        _xmlData = stream.ToArray();
    }



    [Benchmark]
    public async Task<int> ExtractAsync()
    {
        var stream = new MemoryStream(_xmlData);
        var extractor = new XmlSingleStreamExtractor<BenchmarkPerson>
        (
            stream,
            NullLogger<XmlSingleStreamExtractor<BenchmarkPerson>>.Instance
        );

        var count = 0;
        await foreach (var _ in extractor.ExtractAsync())
        {
            count++;
        }

        return count;
    }
}
