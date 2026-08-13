using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.ErrorPolicies;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// Per-item error-handling (#11) on the multi-stream extractor and loader. Each stream/record is
/// independent, so an assignable <c>ErrorPolicy</c> (inherited from the base stages, Abstractions
/// 0.21+) can skip or dead-letter a failed record and keep going, instead of aborting the whole run.
/// </summary>
public sealed class XmlMultiStreamErrorPolicyTests
{
    // A record that serializes fine unless Explode is set, in which case the getter throws — so
    // XmlSerializer.Serialize fails for exactly the Explode instances (the loader-side failure).
    public sealed class ConditionalRecord
    {
        public bool Explode { get; set; }

        public string Value
        {
            get => Explode ? throw new InvalidOperationException("boom") : "ok";
            // XmlSerializer requires a public setter to round-trip the property; the
            // value is never read back, so it is deliberately discarded.
            set => _ = value;
        }
    }


    // === Extractor: a stream that fails to deserialize ===

    [Fact]
    public async Task Extractor_default_policy_aborts_on_a_bad_stream()
    {
        var streams = new List<Stream> { PersonStream("Alice"), BadStream(), PersonStream("Bob") };
        var extractor = new XmlMultiStreamExtractor<PersonRecord>(streams);

        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await foreach (var _ in extractor.ExtractAsync())
            {
            }
        });
    }


    [Fact]
    public async Task Extractor_Skip_policy_skips_the_bad_stream_and_continues()
    {
        var streams = new List<Stream> { PersonStream("Alice"), BadStream(), PersonStream("Bob") };
        var extractor = new XmlMultiStreamExtractor<PersonRecord>(streams) { ErrorPolicy = ItemErrorPolicy.Skip };

        var got = new List<PersonRecord>();
        await foreach (var record in extractor.ExtractAsync())
        {
            got.Add(record);
        }

        Assert.Equal(new[] { "Alice", "Bob" }, got.Select(p => p.FirstName).ToArray());
        Assert.Equal(1, extractor.CurrentErrorItemCount);
    }


    [Fact]
    public async Task Extractor_dead_letter_policy_captures_the_failed_record()
    {
        var deadLetters = new List<ItemErrorContext>();
        var streams = new List<Stream> { PersonStream("Alice"), BadStream(), PersonStream("Bob") };
        var extractor = new XmlMultiStreamExtractor<PersonRecord>(streams)
        {
            ErrorPolicy = ItemErrorPolicy.SkipAndDeadLetter(deadLetters),
        };

        var got = new List<PersonRecord>();
        await foreach (var record in extractor.ExtractAsync())
        {
            got.Add(record);
        }

        Assert.Equal(2, got.Count);
        var dead = Assert.Single(deadLetters);
        Assert.Equal(2, dead.ItemNumber);              // the second stream (1-based)
        Assert.NotNull(dead.Exception);
    }


    // === Loader: a record that fails to serialize ===

    [Fact]
    public async Task Loader_default_policy_aborts_on_a_bad_record()
    {
        var loader = new XmlMultiStreamLoader<ConditionalRecord>(_ => new MemoryStream());

        await Assert.ThrowsAnyAsync<Exception>(async () =>
            await loader.LoadAsync(ToAsync(new[] { new ConditionalRecord { Explode = true } })))
            ;
    }


    [Fact]
    public async Task Loader_Skip_policy_skips_the_bad_record_and_loads_the_rest()
    {
        var loader = new XmlMultiStreamLoader<ConditionalRecord>(_ => new MemoryStream())
        {
            ErrorPolicy = ItemErrorPolicy.Skip,
        };

        var items = new[]
        {
            new ConditionalRecord { Explode = false },
            new ConditionalRecord { Explode = true },
            new ConditionalRecord { Explode = false },
        };

        await loader.LoadAsync(ToAsync(items));

        Assert.Equal(2, loader.CurrentItemCount);
        Assert.Equal(1, loader.CurrentErrorItemCount);
    }


    private static Stream PersonStream(string firstName)
    {
        var record = new PersonRecord { FirstName = firstName, LastName = "X", Age = 1 };
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(PersonRecord));
        var ms = new MemoryStream();
        serializer.Serialize(ms, record);
        ms.Position = 0;
        return ms;
    }


    private static Stream BadStream() =>
        new MemoryStream(System.Text.Encoding.UTF8.GetBytes("<NotAPersonRecord><garbage/></NotAPersonRecord>"));


    private static async IAsyncEnumerable<T> ToAsync<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
