using System;
using System.Collections.Generic;
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
/// var extractor = new XmlSingleStreamExtractor&lt;Person&gt;
/// (
///     File.OpenRead("data.xml"),
///     new XmlSingleStreamExtractorOptions { LeaveOpen = false }
/// );
/// await foreach (var person in extractor.ExtractAsync(cancellationToken))
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
    private readonly IProgressTimer? _progressTimer;
    private readonly bool _leaveOpen;
    private bool _progressTimerWired;
    private readonly List<XmlDeserializationError> _errors = new();



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
    public XmlSingleStreamExtractor(Stream stream, XmlSingleStreamExtractorOptions? options = null)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _logger = NullLogger.Instance;
        _readerSettings = null;
        _leaveOpen = (options ?? new XmlSingleStreamExtractorOptions()).LeaveOpen;
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
    public XmlSingleStreamExtractor
    (
        Stream stream,
        ILogger<XmlSingleStreamExtractor<TRecord>> logger
    )
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _readerSettings = null;
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
    public XmlSingleStreamExtractor
    (
        Stream stream,
        XmlReaderSettings readerSettings,
        ILogger<XmlSingleStreamExtractor<TRecord>> logger,
        XmlSingleStreamExtractorOptions? options = null
    )
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _readerSettings = readerSettings ?? throw new ArgumentNullException(nameof(readerSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _readerSettings = readerSettings ?? throw new ArgumentNullException(nameof(readerSettings));
        _logger = logger ?? (ILogger)NullLogger.Instance;
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
        _leaveOpen = (options ?? new XmlSingleStreamExtractorOptions()).LeaveOpen;
    }



    /// <summary>
    /// Gets how deserialization errors are handled during extraction.
    /// Default is <see cref="ErrorHandling.Throw"/>.
    /// </summary>
    public ErrorHandling ErrorHandling { get; init; } = ErrorHandling.Throw;


    /// <summary>
    /// Gets the collection of deserialization errors captured during the most recent extraction.
    /// Only populated when <see cref="ErrorHandling"/> is <see cref="ErrorHandling.CaptureAndContinue"/>.
    /// </summary>
    public IReadOnlyList<XmlDeserializationError> Errors => _errors.AsReadOnly();


    /// <summary>
    /// Translates <see cref="ErrorHandling"/> into the base error-handling contract (#84): captures
    /// the failure (when <see cref="ErrorHandling.CaptureAndContinue"/>), logs it, and returns
    /// <see cref="ItemErrorAction.Skip"/> — or <see cref="ItemErrorAction.Abort"/> for
    /// <see cref="ErrorHandling.Throw"/>. The base then counts the skip in <c>CurrentErrorItemCount</c>.
    /// </summary>
    /// <param name="context">Describes the failed item. Never <see langword="null"/> via the base caller.</param>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/>.</exception>
    protected override ItemErrorAction OnItemError(ItemErrorContext context)
    {
        // Stryker disable once all: defensive — HandleItemError (the sole caller) already validates
        // this; the guard exists only to stay safe if a future caller bypasses it.
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (ErrorHandling == ErrorHandling.Throw)
        {
            return ItemErrorAction.Abort;
        }

        if (ErrorHandling == ErrorHandling.CaptureAndContinue)
        {
            _errors.Add(new XmlDeserializationError(
                itemNumber: context.ItemNumber,
                rawContent: context.RawContent?.Invoke(),
                exception: context.Exception));
        }

        XmlLogMessages.DeserializationError(_logger, context.ItemNumber, context.Exception);
        return ItemErrorAction.Skip;
    }


    /// <inheritdoc />
    protected override async IAsyncEnumerable<TRecord> ExtractWorkerAsync
    (
        [EnumeratorCancellation] CancellationToken token
    )
    {
        XmlLogMessages.StartingOperation(_logger, OperationName, null);

        _errors.Clear();
        var recordNumber = 0L;
        var skipBudget = SkipItemCount;
        var settings = _readerSettings?.Clone() ?? new XmlReaderSettings();
        settings.CloseInput = !_leaveOpen;
        settings.Async = true;

        using var reader = XmlReader.Create(_stream, settings);

        await AdvancePastRootElementAsync(reader).ConfigureAwait(false);

        var needsRead = true;
        while (!needsRead || await reader.ReadAsync().ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();
            needsRead = true;

            if (reader.NodeType != XmlNodeType.Element || reader.Depth != 1)
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == 0)
                {
                    break;
                }

                continue;
            }

            recordNumber++;
            // Reads the element whole, so the reader is already at the next node — always advance.
            var item = TryDeserializeChildElement(reader, recordNumber);
            needsRead = false;
            if (item is null)
            {
                continue;
            }

            if (skipBudget > 0)
            {
                skipBudget--;
                IncrementCurrentSkippedItemCount();
                XmlLogMessages.SkippedItem(_logger, CurrentSkippedItemCount, SkipItemCount, null);
                continue;
            }

            if (CurrentItemCount >= MaximumItemCount)
            {
                XmlLogMessages.ReachedMaximumItemCount(_logger, MaximumItemCount, null);
                break;
            }

            IncrementCurrentItemCount();
            XmlLogMessages.ExtractedItem(_logger, CurrentItemCount, null);
            yield return item;
        }

        XmlLogMessages.SingleStreamExtractionCompleted(_logger, CurrentItemCount, CurrentSkippedItemCount, null);
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



    private TRecord? TryDeserializeChildElement(XmlReader reader, long recordNumber)
    {
        if (reader.NodeType != XmlNodeType.Element || reader.Depth != 1)
        {
            return default;
        }

        // Read the whole child element as a self-contained fragment. This advances the reader
        // deterministically to the next sibling — the property that makes skip-and-continue
        // possible within a single streaming document — and captures the raw XML for the error
        // context. A malformed (not well-formed) document throws here, outside the policy, and
        // aborts: you cannot reliably skip past XML that does not parse.
        var outerXml = reader.ReadOuterXml();

        try
        {
            using var elementReader = XmlReader.Create(new StringReader(outerXml));
            var item = (TRecord?)Serializer.Deserialize(elementReader);
            if (item is null)
            {
                XmlLogMessages.SkippingNullElement(_logger, null);
            }

            return item;
        }
        catch (InvalidOperationException ex)
        {
            // XmlSerializer wraps type/mapping failures (e.g. a non-numeric value for an int
            // element) in InvalidOperationException. Defer the policy to the base #84 mechanism.
            if (HandleItemError(new ItemErrorContext(recordNumber, ex, () => outerXml)) == ItemErrorAction.Abort)
            {
                throw;
            }

            return default;
        }
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
