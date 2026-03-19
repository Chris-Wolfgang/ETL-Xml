using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
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
/// </remarks>
/// <example>
/// <code>
/// using var stream = File.OpenRead("data.xml");
/// var extractor = new XmlSingleStreamExtractor&lt;Person&gt;(stream, logger);
/// await foreach (var person in extractor.ExtractAsync(cancellationToken))
/// {
///     Console.WriteLine(person.Name);
/// }
/// </code>
/// </example>
public class XmlSingleStreamExtractor<TRecord> : ExtractorBase<TRecord, XmlReport>
    where TRecord : notnull, new()
{
    private readonly Stream _stream;
    private readonly XmlReaderSettings? _readerSettings;
    private readonly XmlSerializer _serializer;
    private readonly ILogger _logger;
    private readonly IProgressTimer? _progressTimer;
    private bool _progressTimerWired;



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamExtractor{TRecord}"/> class.
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
        _serializer = new XmlSerializer(typeof(TRecord));
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamExtractor{TRecord}"/> class
    /// with custom reader settings.
    /// </summary>
    /// <param name="stream">The stream containing XML data to read from.</param>
    /// <param name="readerSettings">The XML reader settings to use for deserialization.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/>, <paramref name="readerSettings"/>, or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    public XmlSingleStreamExtractor
    (
        Stream stream,
        XmlReaderSettings readerSettings,
        ILogger<XmlSingleStreamExtractor<TRecord>> logger
    )
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _readerSettings = readerSettings ?? throw new ArgumentNullException(nameof(readerSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serializer = new XmlSerializer(typeof(TRecord));
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamExtractor{TRecord}"/> class
    /// with an injected progress timer for testing.
    /// </summary>
    /// <param name="stream">The stream containing XML data to read from.</param>
    /// <param name="readerSettings">The XML reader settings to use for deserialization.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <param name="timer">The progress timer to inject.</param>
    internal XmlSingleStreamExtractor
    (
        Stream stream,
        XmlReaderSettings readerSettings,
        ILogger logger,
        IProgressTimer timer
    )
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _readerSettings = readerSettings ?? throw new ArgumentNullException(nameof(readerSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
        _serializer = new XmlSerializer(typeof(TRecord));
    }



    /// <inheritdoc />
    protected override async IAsyncEnumerable<TRecord> ExtractWorkerAsync
    (
        [EnumeratorCancellation] CancellationToken token
    )
    {
        XmlLogMessages.StartingOperation(_logger, $"XML single-stream extraction of {typeof(TRecord).Name}", null);

        var skipBudget = SkipItemCount;
        var settings = _readerSettings?.Clone() ?? new XmlReaderSettings();
        settings.CloseInput = false;
        settings.Async = true;

        using var reader = XmlReader.Create(_stream, settings);

        await AdvancePastRootElementAsync(reader).ConfigureAwait(false);

        var needsRead = true;
        while (needsRead ? await reader.ReadAsync().ConfigureAwait(false) : true)
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

            var item = TryDeserializeChildElement(reader);
            if (item is null)
            {
                continue;
            }

            // After Deserialize, the reader is already positioned at the next node
            needsRead = false;

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



    private TRecord? TryDeserializeChildElement(XmlReader reader)
    {
        if (reader.NodeType != XmlNodeType.Element || reader.Depth != 1)
        {
            return default;
        }

        if (!_serializer.CanDeserialize(reader))
        {
            XmlLogMessages.SkippingNullElement(_logger, null);
            return default;
        }

        var item = (TRecord?)_serializer.Deserialize(reader);
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
