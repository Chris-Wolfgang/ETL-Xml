using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// Loads items of type <typeparamref name="TRecord"/> into a single XML stream
/// wrapped in a root element.
/// </summary>
/// <typeparam name="TRecord">The type of items to load. Must be <c>notnull</c> and have a parameterless constructor.</typeparam>
/// <remarks>
/// Writes an XML document (e.g. <c>&lt;ArrayOfPerson&gt;&lt;Person/&gt;...&lt;/ArrayOfPerson&gt;</c>)
/// to a <see cref="Stream"/> by serializing each item from the input async enumerable sequence.
/// Each item is serialized as a child element of the root using <see cref="XmlSerializer"/>.
/// </remarks>
/// <example>
/// <code>
/// using var stream = File.Create("output.xml");
/// var loader = new XmlSingleStreamLoader&lt;Person&gt;(stream, logger);
/// await loader.LoadAsync(items, cancellationToken);
/// </code>
/// </example>
public sealed class XmlSingleStreamLoader<TRecord> : LoaderBase<TRecord, XmlReport>
    where TRecord : notnull, new()
{
    private static readonly XmlSerializerNamespaces EmptyNamespaces =
        new(new[] { new XmlQualifiedName(name: "", ns: "") });

    private readonly Stream _stream;
    private readonly XmlWriterSettings? _writerSettings;
    private readonly XmlSerializer _serializer;
    private readonly string _rootElementName;
    private readonly ILogger _logger;
    private readonly IProgressTimer? _progressTimer;
    private bool _progressTimerWired;



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamLoader{TRecord}"/> class.
    /// </summary>
    /// <param name="stream">The stream to write XML data to.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    public XmlSingleStreamLoader
    (
        Stream stream,
        ILogger<XmlSingleStreamLoader<TRecord>> logger
    )
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _writerSettings = null;
        _serializer = new XmlSerializer(typeof(TRecord));
        _rootElementName = "ArrayOf" + typeof(TRecord).Name;
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamLoader{TRecord}"/> class
    /// with custom writer settings.
    /// </summary>
    /// <param name="stream">The stream to write XML data to.</param>
    /// <param name="writerSettings">The XML writer settings to use for serialization.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/>, <paramref name="writerSettings"/>, or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    public XmlSingleStreamLoader
    (
        Stream stream,
        XmlWriterSettings writerSettings,
        ILogger<XmlSingleStreamLoader<TRecord>> logger
    )
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _writerSettings = writerSettings ?? throw new ArgumentNullException(nameof(writerSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serializer = new XmlSerializer(typeof(TRecord));
        _rootElementName = "ArrayOf" + typeof(TRecord).Name;
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamLoader{TRecord}"/> class
    /// with an injected progress timer for testing.
    /// </summary>
    /// <param name="stream">The stream to write XML data to.</param>
    /// <param name="writerSettings">The XML writer settings to use for serialization.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <param name="timer">The progress timer to inject.</param>
    internal XmlSingleStreamLoader
    (
        Stream stream,
        XmlWriterSettings writerSettings,
        ILogger logger,
        IProgressTimer timer
    )
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _writerSettings = writerSettings ?? throw new ArgumentNullException(nameof(writerSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
        _serializer = new XmlSerializer(typeof(TRecord));
        _rootElementName = "ArrayOf" + typeof(TRecord).Name;
    }



    /// <inheritdoc />
    protected override async Task LoadWorkerAsync
    (
        IAsyncEnumerable<TRecord> items,
        CancellationToken token
    )
    {
        XmlLogMessages.StartingOperation(_logger, $"XML single-stream loading of {typeof(TRecord).Name}", null);

        var settings = _writerSettings ?? new XmlWriterSettings { Indent = true };
        settings.CloseOutput = false;
        settings.Async = true;

        using var writer = XmlWriter.Create(_stream, settings);

        await writer.WriteStartDocumentAsync().ConfigureAwait(false);
        await writer.WriteStartElementAsync(prefix: null, localName: _rootElementName, ns: null).ConfigureAwait(false);

        await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();

            if (CurrentSkippedItemCount < SkipItemCount)
            {
                IncrementCurrentSkippedItemCount();
                XmlLogMessages.SkippedItem(_logger, CurrentSkippedItemCount, SkipItemCount, null);
                continue;
            }

            if (CurrentItemCount >= MaximumItemCount)
            {
                XmlLogMessages.ReachedMaximumItemCount(_logger, MaximumItemCount, null);
                break;
            }

            _serializer.Serialize(writer, item, EmptyNamespaces);
            IncrementCurrentItemCount();

            XmlLogMessages.LoadedItem(_logger, CurrentItemCount, null);
        }

        await writer.WriteEndElementAsync().ConfigureAwait(false);
        await writer.WriteEndDocumentAsync().ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);

        XmlLogMessages.SingleStreamLoadingCompleted(_logger, CurrentItemCount, CurrentSkippedItemCount, null);
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
