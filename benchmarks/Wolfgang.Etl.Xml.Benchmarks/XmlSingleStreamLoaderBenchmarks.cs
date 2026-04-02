using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;

namespace Wolfgang.Etl.Xml.Benchmarks;

[MemoryDiagnoser]
public class XmlSingleStreamLoaderBenchmarks
{
    private List<BenchmarkPerson> _items = null!;



    [Params(10, 100, 1000)]
    public int ItemCount { get; set; }



    [GlobalSetup]
    public void Setup()
    {
        _items = Enumerable.Range(0, ItemCount).Select(i => new BenchmarkPerson
        {
            FirstName = $"First{i}",
            LastName = $"Last{i}",
            Age = 20 + (i % 50),
            Email = $"person{i}@example.com",
            City = $"City{i % 20}",
        }).ToList();
    }



    [Benchmark]
    public async Task LoadAsync()
    {
        var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<BenchmarkPerson>(stream);

        await loader.LoadAsync(_items.ToAsyncEnumerable());
    }



    [Benchmark]
    public async Task LoadAsync_NoIndent()
    {
        var stream = new MemoryStream();
        var settings = new XmlWriterSettings { Indent = false };
        var loader = new XmlSingleStreamLoader<BenchmarkPerson>
        (
            stream,
            settings,
            NullLogger<XmlSingleStreamLoader<BenchmarkPerson>>.Instance
        );

        await loader.LoadAsync(_items.ToAsyncEnumerable());
    }
}
