using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// Behavioural tests that pin the diagnostic side effects — the structured log events (their
/// message templates and the running item / stream counters baked into them) and the structural
/// reader logic — that the contract base and happy-path tests leave under-asserted. These assert
/// observable behaviour rather than internals, but they close the mutation-testing gaps where a
/// dropped log call, a corrupted counter, or an inverted reader guard would otherwise go unnoticed.
/// </summary>
public sealed class XmlDiagnosticsMutationTests
{
    private static readonly PersonRecord[] ThreePeople =
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25 },
        new() { FirstName = "Carol", LastName = "White", Age = 35 },
    };


    private static readonly PersonRecord[] TwoPeople =
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25 },
    };


    // ── SingleStream extractor: log sequence + running counts ─────────────────

    [Fact]
    public async Task SingleStreamExtractor_logs_start_skip_extract_and_completion_with_counts()
    {
        using var source = await BuildXmlAsync(ThreePeople).ConfigureAwait(false);
        var logger = new CapturingLogger<XmlSingleStreamExtractor<PersonRecord>>();

        var extractor = new XmlSingleStreamExtractor<PersonRecord>(source, logger)
        {
            SkipItemCount = 1,
        };

        await DrainAsync(extractor).ConfigureAwait(false);

        Assert.Contains(logger.Messages, m => m.Contains("Starting XML single-stream extraction of PersonRecord.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Skipped item 1 of 1.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Extracted item 1.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Extracted item 2.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Extracted: 2, skipped: 1.", StringComparison.Ordinal));
    }


    // ── SingleStream loader: log sequence + running counts ────────────────────

    [Fact]
    public async Task SingleStreamLoader_logs_start_skip_load_and_completion_with_counts()
    {
        using var stream = new MemoryStream();
        var logger = new CapturingLogger<XmlSingleStreamLoader<PersonRecord>>();

        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            new XmlWriterSettings(),
            logger,
            new XmlSingleStreamLoaderOptions { LeaveOpen = true }
        )
        {
            SkipItemCount = 1,
        };

        await loader.LoadAsync(ThreePeople.ToAsyncEnumerable()).ConfigureAwait(false);

        Assert.Contains(logger.Messages, m => m.Contains("Starting XML single-stream loading of PersonRecord.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Skipped item 1 of 1.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Loaded item 1.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Loaded item 2.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Loaded: 2, skipped: 1.", StringComparison.Ordinal));
    }


    // ── MultiStream extractor: per-stream index in log messages ───────────────

    [Fact]
    public async Task MultiStreamExtractor_logs_reading_and_extracted_with_stream_index()
    {
        var logger = new CapturingLogger<XmlMultiStreamExtractor<PersonRecord>>();
        var extractor = new XmlMultiStreamExtractor<PersonRecord>(SerializeEach(TwoPeople), logger);

        await DrainAsync(extractor).ConfigureAwait(false);

        Assert.Contains(logger.Messages, m => m.Contains("Reading stream 0.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Reading stream 1.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Extracted item 1 from stream 0.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Extracted item 2 from stream 1.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Extracted: 2, skipped: 0, streams: 2.", StringComparison.Ordinal));
    }


    // ── MultiStream loader: per-stream index in log messages ──────────────────

    [Fact]
    public async Task MultiStreamLoader_logs_loaded_with_stream_index_and_completion()
    {
        var logger = new CapturingLogger<XmlMultiStreamLoader<PersonRecord>>();
        var loader = new XmlMultiStreamLoader<PersonRecord>(_ => new MemoryStream(), logger);

        await loader.LoadAsync(TwoPeople.ToAsyncEnumerable()).ConfigureAwait(false);

        Assert.Contains(logger.Messages, m => m.Contains("Starting XML multi-stream loading of PersonRecord.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Loaded item 1 to stream 0.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Loaded item 2 to stream 1.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Loaded: 2, skipped: 0, streams: 2.", StringComparison.Ordinal));
    }


    // ── SingleStream extractor: skip non-element nodes before the root ─────────

    [Fact]
    public async Task SingleStreamExtractor_advances_past_declaration_and_comment_before_root()
    {
        // An XML declaration and a comment sit at depth 0 ahead of the real root element. The
        // extractor must keep advancing until the depth-0 *element* — a guard that mutates to
        // "stop at the first depth-0 node" would break on the declaration/comment and extract
        // nothing.
        const string xml =
            "<?xml version=\"1.0\"?><!-- leading comment -->" +
            "<ArrayOfPersonRecord>" +
            "<PersonRecord><FirstName>Alice</FirstName><LastName>Smith</LastName><Age>30</Age></PersonRecord>" +
            "<PersonRecord><FirstName>Bob</FirstName><LastName>Jones</LastName><Age>25</Age></PersonRecord>" +
            "</ArrayOfPersonRecord>";
        using var source = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        var extractor = new XmlSingleStreamExtractor<PersonRecord>(source);

        var results = new List<PersonRecord>();
        await foreach (var item in extractor.ExtractAsync().ConfigureAwait(false))
        {
            results.Add(item);
        }

        Assert.Equal(2, results.Count);
        Assert.Equal("Alice", results[0].FirstName);
        Assert.Equal("Bob", results[1].FirstName);
    }


    // ── SingleStream extractor: a child that deserializes to null is skipped ──

    [Fact]
    public async Task SingleStreamExtractor_skips_child_that_deserializes_to_null()
    {
        // An xsi:nil child deserializes to a null record. The extractor must drop it — a mutated
        // "keep it" guard would yield a null into the sequence.
        const string xml =
            "<?xml version=\"1.0\"?>" +
            "<ArrayOfPersonRecord xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<PersonRecord xsi:nil=\"true\" />" +
            "</ArrayOfPersonRecord>";
        using var source = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        var extractor = new XmlSingleStreamExtractor<PersonRecord>(source);

        var results = new List<PersonRecord>();
        await foreach (var item in extractor.ExtractAsync().ConfigureAwait(false))
        {
            results.Add(item);
        }

        Assert.Empty(results);
    }


    // ── MultiStream loader: dry-run counts / logs but never opens a stream ─────

    [Fact]
    public async Task MultiStreamLoader_dry_run_logs_stream_index_and_never_invokes_factory()
    {
        var factoryCalls = 0;
        var logger = new CapturingLogger<XmlMultiStreamLoader<PersonRecord>>();
        var loader = new XmlMultiStreamLoader<PersonRecord>
        (
            _ =>
            {
                factoryCalls++;
                return new MemoryStream();
            },
            logger
        )
        {
            IsDryRun = true,
        };

        await loader.LoadAsync(TwoPeople.ToAsyncEnumerable()).ConfigureAwait(false);

        Assert.Equal(0, factoryCalls);
        Assert.Equal(2, loader.CurrentItemCount);
        Assert.Contains(logger.Messages, m => m.Contains("Loaded item 1 to stream 0.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Loaded item 2 to stream 1.", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, m => m.Contains("Loaded: 2, skipped: 0, streams: 2.", StringComparison.Ordinal));
    }


    // ── SingleStream loader: default root name via the (stream, logger) ctor ──

    [Fact]
    public async Task SingleStreamLoader_stream_logger_ctor_wraps_items_in_default_root_element()
    {
        using var stream = new MemoryStream();
        var logger = new CapturingLogger<XmlSingleStreamLoader<PersonRecord>>();
        var loader = new XmlSingleStreamLoader<PersonRecord>(stream, logger);

        await loader.LoadAsync(TwoPeople.ToAsyncEnumerable()).ConfigureAwait(false);

        var content = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Contains("<ArrayOfPersonRecord>", content, StringComparison.Ordinal);
        Assert.Contains("</ArrayOfPersonRecord>", content, StringComparison.Ordinal);
    }


    // ── SingleStream loader: no namespace declarations on the output ──────────

    [Fact]
    public async Task SingleStreamLoader_output_has_no_namespace_declarations()
    {
        using var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            new XmlSingleStreamLoaderOptions { LeaveOpen = true }
        );

        await loader.LoadAsync(TwoPeople.ToAsyncEnumerable()).ConfigureAwait(false);

        var content = Encoding.UTF8.GetString(stream.ToArray());
        // The loader passes an empty-namespace set to the serializer; the default xsi/xsd
        // declarations must not appear.
        Assert.DoesNotContain("xmlns:xsi", content, StringComparison.Ordinal);
        Assert.DoesNotContain("xmlns:xsd", content, StringComparison.Ordinal);
    }


    // ── LeaveOpen=false honoured through the settings constructors ────────────

    [Fact]
    public async Task SingleStreamExtractor_settings_ctor_leaveOpen_false_closes_stream()
    {
        using var source = await BuildXmlAsync(TwoPeople).ConfigureAwait(false);
        var extractor = new XmlSingleStreamExtractor<PersonRecord>
        (
            source,
            new XmlReaderSettings(),
            new CapturingLogger<XmlSingleStreamExtractor<PersonRecord>>(),
            new XmlSingleStreamExtractorOptions { LeaveOpen = false }
        );

        await DrainAsync(extractor).ConfigureAwait(false);

        Assert.False(source.CanRead);
    }


    [Fact]
    public async Task SingleStreamLoader_settings_ctor_leaveOpen_false_closes_stream()
    {
        var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            new XmlWriterSettings(),
            new CapturingLogger<XmlSingleStreamLoader<PersonRecord>>(),
            new XmlSingleStreamLoaderOptions { LeaveOpen = false }
        );

        await loader.LoadAsync(TwoPeople.ToAsyncEnumerable()).ConfigureAwait(false);

        Assert.False(stream.CanWrite);
    }


    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task DrainAsync(XmlSingleStreamExtractor<PersonRecord> extractor)
    {
        await foreach (var _ in extractor.ExtractAsync().ConfigureAwait(false))
        {
        }
    }


    private static async Task DrainAsync(XmlMultiStreamExtractor<PersonRecord> extractor)
    {
        await foreach (var _ in extractor.ExtractAsync().ConfigureAwait(false))
        {
        }
    }


    private static async Task<MemoryStream> BuildXmlAsync(PersonRecord[] items)
    {
        var ms = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            ms,
            new XmlSingleStreamLoaderOptions { LeaveOpen = true }
        );
        await loader.LoadAsync(items.ToAsyncEnumerable()).ConfigureAwait(false);
        ms.Position = 0;
        return ms;
    }


    private static IEnumerable<Stream> SerializeEach(IEnumerable<PersonRecord> items)
    {
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(PersonRecord));
        foreach (var item in items)
        {
            var ms = new MemoryStream();
            serializer.Serialize(ms, item);
            ms.Position = 0;
            yield return ms;
        }
    }


    private sealed class CapturingLogger<T> : ILogger<T>
    {
        private readonly List<string> _messages = new();


        public IReadOnlyList<string> Messages => _messages;


        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull => NullScope.Instance;


        public bool IsEnabled(LogLevel logLevel) => true;


        public void Log<TState>
        (
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            _messages.Add(formatter(state, exception));
        }


        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();


            public void Dispose()
            {
            }
        }
    }
}
