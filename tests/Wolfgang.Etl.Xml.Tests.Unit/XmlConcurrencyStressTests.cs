using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// Concurrency / race stress tests (#129). Production use of an async library hits real
/// interleavings: many concurrent extractors/loaders sharing the process-wide static
/// <see cref="System.Xml.Serialization.XmlSerializer"/> per record type, and cancellation
/// arriving mid-<c>await</c>. These drive high-fan-out
/// <see cref="Task.WhenAll(System.Collections.Generic.IEnumerable{Task})"/> workloads and assert
/// isolation, correctness, and prompt cancellation without hangs — in particular that the shared
/// static serializer is safe to use from many threads at once (its only mutable state, the
/// reader/writer, is per-operation).
///
/// Plain xunit rather than Coyote — the library surface is <see cref="IAsyncEnumerable{T}"/>-based
/// and Coyote's async-stream scheduling support is too rough to instrument it cleanly (matching the
/// ETL-Csv sibling). Marked <c>[Trait("Category", "Concurrency")]</c> so a soak workflow can select
/// and repeat them.
/// </summary>
[Trait("Category", "Concurrency")]
public class XmlConcurrencyStressTests
{
    private const int Workers = 64;


    private static readonly PersonRecord[] Sample =
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25 },
        new() { FirstName = "Carol", LastName = "White", Age = 42 },
    };


    [Fact]
    public async Task Concurrent_extractions_of_the_same_record_type_stay_isolated_and_correct()
    {
        var xml = await BuildXmlAsync(Sample);

        var tasks = Enumerable.Range(0, Workers).Select(_ => Task.Run(async () =>
        {
            using var stream = new MemoryStream(xml);
            var extractor = new XmlSingleStreamExtractor<PersonRecord>(stream);

            var got = new List<PersonRecord>();
            await foreach (var record in extractor.ExtractAsync())
            {
                got.Add(record);
            }

            return got;
        }));

        var results = await Task.WhenAll(tasks);

        foreach (var got in results)
        {
            Assert.Equal(3, got.Count);
            Assert.Equal("Alice", got[0].FirstName);
            Assert.Equal("White", got[2].LastName);
            Assert.Equal(25, got[1].Age);
        }
    }


    [Fact]
    public async Task Concurrent_loads_produce_correct_independent_output()
    {
        var expected = await BuildXmlAsync(Sample);

        var tasks = Enumerable.Range(0, Workers).Select(_ => Task.Run(async () =>
            await BuildXmlAsync(Sample)));

        var outputs = await Task.WhenAll(tasks);

        Assert.All(outputs, output => Assert.Equal(expected, output));
    }


    [Fact]
    public async Task Cancellation_mid_concurrent_extraction_throws_and_never_hangs()
    {
        var many = Enumerable.Range(0, 2000)
            .Select(i => new PersonRecord { FirstName = $"First{i}", LastName = $"Last{i}", Age = i % 120 })
            .ToArray();
        var xml = await BuildXmlAsync(many);

        var tasks = Enumerable.Range(0, Workers).Select(_ => Task.Run(async () =>
        {
            using var cts = new CancellationTokenSource();
            using var stream = new MemoryStream(xml);
            var extractor = new XmlSingleStreamExtractor<PersonRecord>(stream);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                var seen = 0;
                await foreach (var record in extractor.ExtractAsync(cts.Token))
                {
                    if (record is not null && ++seen == 5)
                    {
                        cts.Cancel();
                    }
                }
            });
        }));

        var all = Task.WhenAll(tasks);
        var finished = await Task.WhenAny(all, Task.Delay(TimeSpan.FromSeconds(30)));

        Assert.Same(all, finished);
        await all;
    }


    private static async Task<byte[]> BuildXmlAsync(IReadOnlyList<PersonRecord> people)
    {
        using var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            new XmlSingleStreamLoaderOptions { LeaveOpen = true }
        );

        await loader.LoadAsync(ToAsync(people)).ConfigureAwait(false);

        return stream.ToArray();
    }


    private static async IAsyncEnumerable<PersonRecord> ToAsync(IEnumerable<PersonRecord> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
