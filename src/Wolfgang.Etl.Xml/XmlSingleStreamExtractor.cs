using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// Extracts items of type <typeparamref name="TRecord"/> from a single XML stream
/// containing a root element with child elements.
/// </summary>
/// <typeparam name="TRecord">The type of items to extract. Must be <c>notnull</c> and have a parameterless constructor.</typeparam>
/// <remarks>
/// Reads an XML document (e.g. <c>&lt;ArrayOfPerson&gt;&lt;Person/&gt;...&lt;/ArrayOfPerson&gt;</c>)
/// from a <see cref="Stream"/> and yields each deserialized child element as an item
/// in the async enumerable sequence. Uses <see cref="XmlReader"/> for streaming deserialization
/// so that the entire document is not buffered in memory.
/// <para>
/// By default the stream is left open after extraction completes. To have the stream closed
/// automatically when extraction finishes, set <see cref="XmlSingleStreamExtractorOptions.LeaveOpen"/>
/// to <c>false</c>, mirroring the behaviour of <see cref="System.IO.StreamReader"/> and
/// <see cref="System.IO.BinaryReader"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Leave stream open (default) — caller controls stream lifetime:
/// using var stream = File.OpenRead("data.xml");
/// var extractor = new XmlSingleStreamExtractor&lt;Person&gt;(stream);
/// await foreach (var person in extractor.ExtractAsync(cancellationToken))
/// {
///     Console.WriteLine(person.Name);
/// }
///
/// // Transfer stream ownership — closed automatically when extraction completes:
/// var owningExtractor = new XmlSingleStreamExtractor&lt;Person&gt;
/// (
///     File.OpenRead("data.xml"),
///     new XmlSingleStreamExtractorOptions { LeaveOpen = false }
/// );
/// await foreach (var person in owningExtractor.ExtractAsync(cancellationToken))
/// {
///     Console.WriteLine(person.Name);
/// }
/// </code>
/// </example>
public sealed class XmlSingleStreamExtractor<TRecord> : ExtractorBase<TRecord, XmlReport>
    where TRecord : notnull, new()
{
    private readonly Stream _stream;
    private readonly XmlReaderSettings? _readerSettings;
    private static readonly XmlSerializer Serializer = new(typeof(TRecord));
    private readonly ILogger _logger;
    private static readonly string OperationName = $"XML single-stream extraction of {typeof(TRecord).Name}";
    private static readonly KeyValuePair<string, object?>[] MetricTags =
    {
        new("etl.operation", "extract"),
        new("etl.component", "XmlSingleStream"),
        new("etl.record_type", typeof(TRecord).Name),
    };
    private readonly IProgressTimer? _progressTimer;
    private readonly bool _leaveOpen;
    private bool _progressTimerWired;



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamExtractor{TRecord}"/> class.
    /// </summary>
    /// <param name="stream">The stream containing XML data to read from.</param>
    /// <param name="options">
    /// Options that control extractor behaviour. When <c>null</c>, defaults are used.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> is <c>null</c>.
    /// </exception>
    [RequiresUnreferencedCode("XmlSingleStreamExtractor deserializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlSingleStreamExtractor(Stream stream, XmlSingleStreamExtractorOptions? options = null)
        : this(stream, options, logger: null)
    {
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamExtractor{TRecord}"/> class
    /// with a logger.
    /// </summary>
    /// <param name="stream">The stream containing XML data to read from.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    [RequiresUnreferencedCode("XmlSingleStreamExtractor deserializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlSingleStreamExtractor
    (
        Stream stream,
        ILogger<XmlSingleStreamExtractor<TRecord>> logger
    )
        // Delegates to the canonical constructor rather than assigning fields directly. The
        // hand-rolled version omitted _leaveOpen, so this overload silently defaulted it to
        // false and closed the caller's stream, contradicting the documented LeaveOpen = true.
        : this(stream, options: null, logger: logger ?? throw new ArgumentNullException(nameof(logger)))
    {
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamExtractor{TRecord}"/> class
    /// with custom reader settings.
    /// </summary>
    /// <param name="stream">The stream containing XML data to read from.</param>
    /// <param name="readerSettings">The XML reader settings to use for deserialization.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <param name="options">
    /// Options that control extractor behaviour. When <c>null</c>, defaults are used.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/>, <paramref name="readerSettings"/>, or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    [RequiresUnreferencedCode("XmlSingleStreamExtractor deserializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlSingleStreamExtractor
    (
        Stream stream,
        XmlReaderSettings readerSettings,
        ILogger<XmlSingleStreamExtractor<TRecord>> logger,
        XmlSingleStreamExtractorOptions? options = null
    )
        : this
        (
            stream,
            settings: readerSettings ?? throw new ArgumentNullException(nameof(readerSettings)),
            options: options,
            logger: logger ?? throw new ArgumentNullException(nameof(logger)),
            timer: null
        )
    {
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamExtractor{TRecord}"/> class.
    /// This is the canonical constructor — every other overload delegates to it — and it is the
    /// only one that lets <paramref name="options"/> and <paramref name="logger"/> be supplied
    /// together without also supplying <see cref="XmlReaderSettings"/>.
    /// </summary>
    /// <param name="stream">The stream containing XML data to read from.</param>
    /// <param name="options">
    /// Options that control extractor behaviour. When <c>null</c>, defaults are used.
    /// </param>
    /// <param name="logger">
    /// An optional logger instance for diagnostic output. When <c>null</c>, logging is disabled.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> is <c>null</c>.
    /// </exception>
    /// <example>
    /// <code>
    /// // Options and a logger, without having to supply XmlReaderSettings:
    /// var extractor = new XmlSingleStreamExtractor&lt;Person&gt;
    /// (
    ///     stream,
    ///     options: new XmlSingleStreamExtractorOptions { LeaveOpen = false },
    ///     logger: loggerFactory.CreateLogger&lt;XmlSingleStreamExtractor&lt;Person&gt;&gt;()
    /// );
    /// </code>
    /// </example>
    [RequiresUnreferencedCode("XmlSingleStreamExtractor deserializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlSingleStreamExtractor
    (
        Stream stream,
        XmlSingleStreamExtractorOptions? options,
        ILogger<XmlSingleStreamExtractor<TRecord>>? logger = null
    )
        : this(stream, settings: null, options: options, logger: logger, timer: null)
    {
    }



    // The single initialization path. Every public and internal constructor delegates here, so
    // there is exactly one place that resolves options -> _leaveOpen. The previous shape let each
    // overload initialize fields itself, which is how the (stream, logger) overload came to omit
    // _leaveOpen entirely and silently close the caller's stream.
    private XmlSingleStreamExtractor
    (
        Stream stream,
        XmlReaderSettings? settings,
        XmlSingleStreamExtractorOptions? options,
        ILogger? logger,
        IProgressTimer? timer
    )
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _readerSettings = settings;
        _logger = logger ?? NullLogger.Instance;
        _progressTimer = timer;
        _leaveOpen = (options ?? new XmlSingleStreamExtractorOptions()).LeaveOpen;
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamExtractor{TRecord}"/> class
    /// with an injected progress timer for testing.
    /// </summary>
    /// <param name="stream">The stream containing XML data to read from.</param>
    /// <param name="readerSettings">The XML reader settings to use for deserialization.</param>
    /// <param name="logger">An optional logger instance for diagnostic output.</param>
    /// <param name="timer">The progress timer to inject.</param>
    /// <param name="options">
    /// Options that control extractor behaviour. When <c>null</c>, defaults are used.
    /// </param>
    internal XmlSingleStreamExtractor
    (
        Stream stream,
        XmlReaderSettings readerSettings,
        ILogger? logger,
        IProgressTimer timer,
        XmlSingleStreamExtractorOptions? options = null
    )
        : this
        (
            stream,
            settings: readerSettings ?? throw new ArgumentNullException(nameof(readerSettings)),
            options: options,
            logger: logger,
            timer: timer ?? throw new ArgumentNullException(nameof(timer))
        )
    {
    }



    /// <inheritdoc />
    protected override async IAsyncEnumerable<TRecord> ExtractWorkerAsync
    (
        [EnumeratorCancellation] CancellationToken token
    )
    {
        XmlLogMessages.StartingOperation(_logger, OperationName, null);
        using var operationScope = XmlMetrics.StartOperation(MetricTags);

        var skipBudget = SkipItemCount;

        using var reader = XmlReader.Create(_stream, CreateReaderSettings());

        await AdvancePastRootElementAsync(reader).ConfigureAwait(false);

        var needsRead = true;
        while (!needsRead || await reader.ReadAsync().ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();
            needsRead = true;

            if (!IsChildElement(reader))
            {
                if (IsDocumentEnd(reader))
                {
                    break;
                }

                continue;
            }

            var item = TryDeserializeChildElement(reader, out var consumedElement);

            // Recorded even on the null path: skipping it there re-read and swallowed the
            // following sibling. See TryDeserializeChildElement.
            if (consumedElement)
            {
                needsRead = false;
            }

            if (item is null)
            {
                continue;
            }

            if (skipBudget > 0)
            {
                skipBudget--;
                RecordSkipped();
                continue;
            }

            if (CurrentItemCount >= MaximumItemCount)
            {
                XmlLogMessages.ReachedMaximumItemCount(_logger, MaximumItemCount, null);
                break;
            }

            RecordExtracted();

            yield return item;
        }

        XmlLogMessages.SingleStreamExtractionCompleted(_logger, CurrentItemCount, CurrentSkippedItemCount, null);
    }



    private XmlReaderSettings CreateReaderSettings()
    {
        var settings = _readerSettings?.Clone() ?? new XmlReaderSettings();
        settings.CloseInput = !_leaveOpen;
        settings.Async = true;
        return settings;
    }



    private static async Task AdvancePastRootElementAsync(XmlReader reader)
    {
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Depth == 0)
            {
                break;
            }
        }
    }



    // Counter + metric + log for one skipped item, kept together so the three can never drift.
    private void RecordSkipped()
    {
        IncrementCurrentSkippedItemCount();
        XmlMetrics.RecordSkipped(MetricTags);
        XmlLogMessages.SkippedItem(_logger, CurrentSkippedItemCount, SkipItemCount, null);
    }



    // Counter + metric + log for one extracted item.
    private void RecordExtracted()
    {
        IncrementCurrentItemCount();
        XmlMetrics.RecordExtracted(MetricTags);
        XmlLogMessages.ExtractedItem(_logger, CurrentItemCount, null);
    }



    // A record lives at depth 1 — a direct child of the root element.
    private static bool IsChildElement(XmlReader reader) =>
        reader.NodeType == XmlNodeType.Element && reader.Depth == 1;



    // The root element's closing tag; nothing further in the document can be a record.
    private static bool IsDocumentEnd(XmlReader reader) =>
        reader.NodeType == XmlNodeType.EndElement && reader.Depth == 0;



    // Deserializes the child element the reader is currently positioned on.
    // <paramref name="consumedElement"/> reports whether the reader was actually advanced past an
    // element — the caller needs that to decide whether to read again, and it cannot infer it from
    // a null return value, because null means both "not positioned on a child element" (reader
    // untouched) and "the element deserialized to null" (reader advanced).
    private TRecord? TryDeserializeChildElement(XmlReader reader, out bool consumedElement)
    {
        if (!IsChildElement(reader))
        {
            consumedElement = false;
            return default;
        }

        consumedElement = true;
        var item = (TRecord?)Serializer.Deserialize(reader);
        if (item is null)
        {
            XmlLogMessages.SkippingNullElement(_logger, null);
        }

        return item;
    }



    /// <inheritdoc />
    protected override XmlReport CreateProgressReport() =>
        new
        (
            CurrentItemCount,
            CurrentSkippedItemCount
        );



    /// <inheritdoc />
    protected override IProgressTimer CreateProgressTimer(IProgress<XmlReport> progress)
    {
        if (_progressTimer is not null)
        {
            if (!_progressTimerWired)
            {
                _progressTimerWired = true;
                _progressTimer.Elapsed += () => progress.Report(CreateProgressReport());
            }

            return _progressTimer;
        }

        return base.CreateProgressTimer(progress);
    }
}
