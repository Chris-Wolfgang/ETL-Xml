using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// Loads items of type <typeparamref name="TRecord"/> into a single XML stream
/// wrapped in a root element.
/// </summary>
/// <typeparam name="TRecord">The type of items to load. Must be <c>notnull</c> and have a parameterless constructor.</typeparam>
/// <remarks>
/// Writes an XML document to a <see cref="Stream"/> by serializing each item from the input
/// async enumerable sequence as a child element of a configurable root element. When no root
/// element name is specified the name defaults to <c>ArrayOf{TypeName}</c>
/// (e.g. <c>ArrayOfPerson</c>). Each item is serialized using <see cref="XmlSerializer"/>.
/// <para>
/// By default the stream is left open after loading completes. Pass <c>leaveOpen: false</c>
/// to have the stream closed when loading finishes, mirroring the behaviour of
/// <see cref="System.IO.StreamWriter"/> and <see cref="System.IO.BinaryWriter"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Default root element name (ArrayOfPerson):
/// using var stream = File.Create("output.xml");
/// var loader = new XmlSingleStreamLoader&lt;Person&gt;(stream);
/// await loader.LoadAsync(items, cancellationToken);
///
/// // Custom root element name:
/// var loader = new XmlSingleStreamLoader&lt;Person&gt;(stream, rootElementName: "People");
/// await loader.LoadAsync(items, cancellationToken);
///
/// // Transfer stream ownership — closed automatically when loading completes:
/// var loader = new XmlSingleStreamLoader&lt;Person&gt;(File.Create("output.xml"), leaveOpen: false);
/// await loader.LoadAsync(items, cancellationToken);
/// </code>
/// </example>
public sealed class XmlSingleStreamLoader<TRecord> : LoaderBase<TRecord, XmlReport>
    where TRecord : notnull, new()
{
    private static readonly string OperationName = $"XML single-stream loading of {typeof(TRecord).Name}";
    private static readonly XmlSerializerNamespaces EmptyNamespaces =
        new(new[] { new XmlQualifiedName(name: "", ns: "") });

    private readonly Stream _stream;
    private readonly XmlWriterSettings? _writerSettings;
    private static readonly XmlSerializer Serializer = new(typeof(TRecord));
    private readonly string _rootElementName;
    private readonly ILogger _logger;
    private readonly IProgressTimer? _progressTimer;
    private readonly bool _leaveOpen;
    private bool _progressTimerWired;



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamLoader{TRecord}"/> class.
    /// </summary>
    /// <param name="stream">The stream to write XML data to.</param>
    /// <param name="rootElementName">
    /// The name of the XML root element that wraps all items. When <c>null</c> the name
    /// defaults to <c>ArrayOf{TypeName}</c> (e.g. <c>ArrayOfPerson</c>).
    /// </param>
    /// <param name="leaveOpen">
    /// <c>true</c> to leave <paramref name="stream"/> open after loading completes;
    /// <c>false</c> to close it. Defaults to <c>true</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="rootElementName"/> is an empty or whitespace string.
    /// </exception>
    public XmlSingleStreamLoader(Stream stream, string? rootElementName = null, bool leaveOpen = true)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _logger = NullLogger.Instance;
        _writerSettings = null;
        _leaveOpen = leaveOpen;
        _rootElementName = ResolveRootElementName(rootElementName);
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamLoader{TRecord}"/> class
    /// with custom writer settings.
    /// </summary>
    /// <param name="stream">The stream to write XML data to.</param>
    /// <param name="writerSettings">The XML writer settings to use for serialization.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <param name="rootElementName">
    /// The name of the XML root element that wraps all items. When <c>null</c> the name
    /// defaults to <c>ArrayOf{TypeName}</c> (e.g. <c>ArrayOfPerson</c>).
    /// </param>
    /// <param name="leaveOpen">
    /// <c>true</c> to leave <paramref name="stream"/> open after loading completes;
    /// <c>false</c> to close it. Defaults to <c>true</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/>, <paramref name="writerSettings"/>, or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="rootElementName"/> is an empty or whitespace string.
    /// </exception>
    public XmlSingleStreamLoader
    (
        Stream stream,
        XmlWriterSettings writerSettings,
        ILogger<XmlSingleStreamLoader<TRecord>> logger,
        string? rootElementName = null,
        bool leaveOpen = true
    )
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _writerSettings = writerSettings ?? throw new ArgumentNullException(nameof(writerSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _leaveOpen = leaveOpen;
        _rootElementName = ResolveRootElementName(rootElementName);
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamLoader{TRecord}"/> class
    /// with an injected progress timer for testing.
    /// </summary>
    /// <param name="stream">The stream to write XML data to.</param>
    /// <param name="writerSettings">The XML writer settings to use for serialization.</param>
    /// <param name="logger">An optional logger instance for diagnostic output.</param>
    /// <param name="timer">The progress timer to inject.</param>
    /// <param name="rootElementName">
    /// The name of the XML root element that wraps all items. When <c>null</c> the name
    /// defaults to <c>ArrayOf{TypeName}</c> (e.g. <c>ArrayOfPerson</c>).
    /// </param>
    /// <param name="leaveOpen">
    /// <c>true</c> to leave <paramref name="stream"/> open after loading completes;
    /// <c>false</c> to close it. Defaults to <c>true</c>.
    /// </param>
    internal XmlSingleStreamLoader
    (
        Stream stream,
        XmlWriterSettings writerSettings,
        ILogger? logger,
        IProgressTimer timer,
        string? rootElementName = null,
        bool leaveOpen = true
    )
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _writerSettings = writerSettings ?? throw new ArgumentNullException(nameof(writerSettings));
        _logger = logger ?? (ILogger)NullLogger.Instance;
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
        _leaveOpen = leaveOpen;
        _rootElementName = ResolveRootElementName(rootElementName);
    }



    /// <inheritdoc />
    protected override async Task LoadWorkerAsync
    (
        IAsyncEnumerable<TRecord> items,
        CancellationToken token
    )
    {
        XmlLogMessages.StartingOperation(_logger, OperationName, null);

        var settings = _writerSettings?.Clone() ?? new XmlWriterSettings { Indent = true };
        settings.CloseOutput = !_leaveOpen;
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

            Serializer.Serialize(writer, item, EmptyNamespaces);
            IncrementCurrentItemCount();

            XmlLogMessages.LoadedItem(_logger, CurrentItemCount, null);
        }

        await writer.WriteEndElementAsync().ConfigureAwait(false);
        await writer.WriteEndDocumentAsync().ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);

        XmlLogMessages.SingleStreamLoadingCompleted(_logger, CurrentItemCount, CurrentSkippedItemCount, null);
    }



    private static string ResolveRootElementName(string? rootElementName)
    {
        if (rootElementName is null)
        {
            return "ArrayOf" + typeof(TRecord).Name;
        }

        if (string.IsNullOrWhiteSpace(rootElementName))
        {
            throw new ArgumentException
            (
                "Root element name cannot be empty or whitespace.",
                nameof(rootElementName)
            );
        }

        try
        {
            System.Xml.XmlConvert.VerifyNCName(rootElementName);
        }
        catch (System.Xml.XmlException ex)
        {
            throw new ArgumentException
            (
                $"Root element name '{rootElementName}' is not a valid XML local name.",
                nameof(rootElementName),
                ex
            );
        }

        return rootElementName;
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
