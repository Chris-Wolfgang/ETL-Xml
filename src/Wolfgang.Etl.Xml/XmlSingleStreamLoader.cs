using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
/// async enumerable sequence as a child element of a configurable root element. The root
/// element name defaults to <c>ArrayOf{TypeName}</c> (e.g. <c>ArrayOfPerson</c>) but can be
/// overridden via <see cref="XmlSingleStreamLoaderOptions.RootElementName"/>.
/// Each item is serialized using <see cref="XmlSerializer"/>.
/// <para>
/// By default the stream is left open after loading completes. To have the stream closed
/// automatically when loading finishes, set <see cref="XmlSingleStreamLoaderOptions.LeaveOpen"/>
/// to <c>false</c>, mirroring the behaviour of <see cref="System.IO.StreamWriter"/> and
/// <see cref="System.IO.BinaryWriter"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Default root element name (ArrayOfPerson), stream left open:
/// using var stream = File.Create("output.xml");
/// var loader = new XmlSingleStreamLoader&lt;Person&gt;(stream);
/// await loader.LoadAsync(items, cancellationToken);
///
/// // Custom root element name, stream closed automatically:
/// var owningLoader = new XmlSingleStreamLoader&lt;Person&gt;
/// (
///     File.Create("output.xml"),
///     new XmlSingleStreamLoaderOptions
///     {
///         RootElementName = "People",
///         LeaveOpen = false,
///     }
/// );
/// await owningLoader.LoadAsync(items, cancellationToken);
/// </code>
/// </example>
public sealed class XmlSingleStreamLoader<TRecord> : LoaderBase<TRecord, XmlReport>, ISupportDryRun
    where TRecord : notnull, new()
{
    private static readonly string OperationName = $"XML single-stream loading of {typeof(TRecord).Name}";
    private static readonly KeyValuePair<string, object?>[] MetricTags =
    {
        new("etl.operation", "load"),
        new("etl.component", "XmlSingleStream"),
        new("etl.record_type", typeof(TRecord).Name),
    };

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
    /// <param name="options">
    /// Options that control loader behaviour. When <c>null</c>, defaults are used.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <see cref="XmlSingleStreamLoaderOptions.RootElementName"/> is an empty
    /// or whitespace string.
    /// </exception>
    [RequiresUnreferencedCode("XmlSingleStreamLoader serializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlSingleStreamLoader(Stream stream, XmlSingleStreamLoaderOptions? options = null)
        : this(stream, options, logger: null)
    {
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamLoader{TRecord}"/> class
    /// with a logger.
    /// </summary>
    /// <param name="stream">The stream to write XML data to.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    [RequiresUnreferencedCode("XmlSingleStreamLoader serializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlSingleStreamLoader
    (
        Stream stream,
        ILogger<XmlSingleStreamLoader<TRecord>> logger
    )
        // Delegates to the canonical constructor rather than assigning fields directly. The
        // hand-rolled version omitted _leaveOpen, so this overload silently defaulted it to
        // false and closed the caller's stream, contradicting the documented LeaveOpen = true.
        : this(stream, options: null, logger: logger ?? throw new ArgumentNullException(nameof(logger)))
    {
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamLoader{TRecord}"/> class
    /// with custom writer settings.
    /// </summary>
    /// <param name="stream">The stream to write XML data to.</param>
    /// <param name="writerSettings">The XML writer settings to use for serialization.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <param name="options">
    /// Options that control loader behaviour. When <c>null</c>, defaults are used.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/>, <paramref name="writerSettings"/>, or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <see cref="XmlSingleStreamLoaderOptions.RootElementName"/> is an empty
    /// or whitespace string.
    /// </exception>
    [RequiresUnreferencedCode("XmlSingleStreamLoader serializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlSingleStreamLoader
    (
        Stream stream,
        XmlWriterSettings writerSettings,
        ILogger<XmlSingleStreamLoader<TRecord>> logger,
        XmlSingleStreamLoaderOptions? options = null
    )
        : this
        (
            stream,
            settings: writerSettings ?? throw new ArgumentNullException(nameof(writerSettings)),
            options: options,
            logger: logger ?? throw new ArgumentNullException(nameof(logger)),
            timer: null
        )
    {
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamLoader{TRecord}"/> class.
    /// This is the canonical constructor — every other overload delegates to it — and it is the
    /// only one that lets <paramref name="options"/> and <paramref name="logger"/> be supplied
    /// together without also supplying <see cref="XmlWriterSettings"/>.
    /// </summary>
    /// <param name="stream">The stream to write XML data to.</param>
    /// <param name="options">
    /// Options that control loader behaviour. When <c>null</c>, defaults are used.
    /// </param>
    /// <param name="logger">
    /// An optional logger instance for diagnostic output. When <c>null</c>, logging is disabled.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <see cref="XmlSingleStreamLoaderOptions.RootElementName"/> is an empty or
    /// whitespace string, or is not a valid XML local name.
    /// </exception>
    /// <example>
    /// <code>
    /// // Options and a logger, without having to supply XmlWriterSettings:
    /// var loader = new XmlSingleStreamLoader&lt;Person&gt;
    /// (
    ///     stream,
    ///     options: new XmlSingleStreamLoaderOptions { RootElementName = "People" },
    ///     logger: loggerFactory.CreateLogger&lt;XmlSingleStreamLoader&lt;Person&gt;&gt;()
    /// );
    /// </code>
    /// </example>
    [RequiresUnreferencedCode("XmlSingleStreamLoader serializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlSingleStreamLoader
    (
        Stream stream,
        XmlSingleStreamLoaderOptions? options,
        ILogger<XmlSingleStreamLoader<TRecord>>? logger = null
    )
        : this(stream, settings: null, options: options, logger: logger, timer: null)
    {
    }



    // The single initialization path. Every public and internal constructor delegates here, so
    // there is exactly one place that resolves options -> _leaveOpen / _rootElementName. The
    // previous shape let each overload initialize fields itself, which is how the (stream, logger)
    // overload came to omit _leaveOpen entirely and silently close the caller's stream.
    private XmlSingleStreamLoader
    (
        Stream stream,
        XmlWriterSettings? settings,
        XmlSingleStreamLoaderOptions? options,
        ILogger? logger,
        IProgressTimer? timer
    )
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _writerSettings = settings;
        _logger = logger ?? NullLogger.Instance;
        _progressTimer = timer;
        var resolved = options ?? new XmlSingleStreamLoaderOptions();
        _leaveOpen = resolved.LeaveOpen;
        _rootElementName = ResolveRootElementName(resolved.RootElementName);
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamLoader{TRecord}"/> class that
    /// writes to an <see cref="IBufferWriter{T}"/> of bytes instead of a <see cref="Stream"/> (#8) —
    /// serialized bytes flow straight into the buffer writer with no intermediate stream buffering.
    /// </summary>
    /// <param name="bufferWriter">The buffer writer to write XML data to.</param>
    /// <param name="options">
    /// Options that control loader behaviour. When <c>null</c>, defaults are used.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bufferWriter"/> is <c>null</c>.</exception>
    [RequiresUnreferencedCode("XmlSingleStreamLoader serializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlSingleStreamLoader(IBufferWriter<byte> bufferWriter, XmlSingleStreamLoaderOptions? options = null)
        : this(new BufferWriterStream(bufferWriter ?? throw new ArgumentNullException(nameof(bufferWriter))), options)
    {
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamLoader{TRecord}"/> class writing
    /// to an <see cref="IBufferWriter{T}"/> of bytes (#8) with a logger.
    /// </summary>
    /// <param name="bufferWriter">The buffer writer to write XML data to.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="bufferWriter"/> or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    [RequiresUnreferencedCode("XmlSingleStreamLoader serializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlSingleStreamLoader(IBufferWriter<byte> bufferWriter, ILogger<XmlSingleStreamLoader<TRecord>> logger)
        : this(new BufferWriterStream(bufferWriter ?? throw new ArgumentNullException(nameof(bufferWriter))), logger)
    {
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamLoader{TRecord}"/> class writing
    /// to an <see cref="IBufferWriter{T}"/> of bytes (#8) with custom writer settings.
    /// </summary>
    /// <param name="bufferWriter">The buffer writer to write XML data to.</param>
    /// <param name="writerSettings">The XML writer settings to use for serialization.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <param name="options">
    /// Options that control loader behaviour. When <c>null</c>, defaults are used.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="bufferWriter"/>, <paramref name="writerSettings"/>, or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    [RequiresUnreferencedCode("XmlSingleStreamLoader serializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlSingleStreamLoader
    (
        IBufferWriter<byte> bufferWriter,
        XmlWriterSettings writerSettings,
        ILogger<XmlSingleStreamLoader<TRecord>> logger,
        XmlSingleStreamLoaderOptions? options = null
    )
        : this(new BufferWriterStream(bufferWriter ?? throw new ArgumentNullException(nameof(bufferWriter))), writerSettings, logger, options)
    {
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamLoader{TRecord}"/> class writing
    /// to an <see cref="IBufferWriter{T}"/> of bytes (#8). This is the canonical buffer-writer
    /// overload — it is the only one that lets <paramref name="options"/> and
    /// <paramref name="logger"/> be supplied together without also supplying
    /// <see cref="XmlWriterSettings"/>.
    /// </summary>
    /// <param name="bufferWriter">The buffer writer to write XML data to.</param>
    /// <param name="options">
    /// Options that control loader behaviour. When <c>null</c>, defaults are used.
    /// </param>
    /// <param name="logger">
    /// An optional logger instance for diagnostic output. When <c>null</c>, logging is disabled.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="bufferWriter"/> is <c>null</c>.
    /// </exception>
    [RequiresUnreferencedCode("XmlSingleStreamLoader serializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlSingleStreamLoader
    (
        IBufferWriter<byte> bufferWriter,
        XmlSingleStreamLoaderOptions? options,
        ILogger<XmlSingleStreamLoader<TRecord>>? logger = null
    )
        : this(new BufferWriterStream(bufferWriter ?? throw new ArgumentNullException(nameof(bufferWriter))), options, logger)
    {
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSingleStreamLoader{TRecord}"/> class
    /// with an injected progress timer for testing.
    /// </summary>
    /// <param name="stream">The stream to write XML data to.</param>
    /// <param name="writerSettings">The XML writer settings to use for serialization.</param>
    /// <param name="logger">An optional logger instance for diagnostic output.</param>
    /// <param name="timer">The progress timer to inject.</param>
    /// <param name="options">
    /// Options that control loader behaviour. When <c>null</c>, defaults are used.
    /// </param>
    internal XmlSingleStreamLoader
    (
        Stream stream,
        XmlWriterSettings writerSettings,
        ILogger? logger,
        IProgressTimer timer,
        XmlSingleStreamLoaderOptions? options = null
    )
        : this
        (
            stream,
            settings: writerSettings ?? throw new ArgumentNullException(nameof(writerSettings)),
            options: options,
            logger: logger,
            timer: timer ?? throw new ArgumentNullException(nameof(timer))
        )
    {
    }



    /// <summary>
    /// Gets or sets a value indicating whether the loader runs in <em>dry-run</em> mode (#176).
    /// When <see langword="true"/>, the load enumerates the source, honours
    /// <see cref="LoaderBase{TRecord, TProgress}.SkipItemCount"/> /
    /// <see cref="LoaderBase{TRecord, TProgress}.MaximumItemCount"/>, advances the progress
    /// counters, and logs as usual, but writes nothing to the output stream. Defaults to
    /// <see langword="false"/>.
    /// </summary>
    public bool IsDryRun { get; set; }



    /// <inheritdoc />
    protected override async Task LoadWorkerAsync
    (
        IAsyncEnumerable<TRecord> items,
        CancellationToken token
    )
    {
        // Honour a token that is already cancelled before pulling the first item from the
        // source or writing anything — a pre-cancelled load must consume nothing.
        token.ThrowIfCancellationRequested();

        XmlLogMessages.StartingOperation(_logger, OperationName, null);
        using var operationScope = XmlMetrics.StartOperation(MetricTags);

        // Dry run (#176): enumerate, count, and log exactly as a real load, but write
        // nothing to the output stream — no writer is created, so the document (including
        // the root-element wrapper) is never emitted and the stream is left untouched.
        var writer = IsDryRun ? null : await CreateDocumentWriterAsync().ConfigureAwait(false);
        try
        {
            await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();

                if (CurrentSkippedItemCount < SkipItemCount)
                {
                    IncrementCurrentSkippedItemCount();
                    XmlMetrics.RecordSkipped(MetricTags);
                    XmlLogMessages.SkippedItem(_logger, CurrentSkippedItemCount, SkipItemCount, null);
                    continue;
                }

                if (CurrentItemCount >= MaximumItemCount)
                {
                    XmlLogMessages.ReachedMaximumItemCount(_logger, MaximumItemCount, null);
                    break;
                }

                if (writer is not null)
                {
                    Serializer.Serialize(writer, item, XmlSerializerNamespacesCache.Empty);
                }

                IncrementCurrentItemCount();
                XmlMetrics.RecordLoaded(MetricTags);
                XmlLogMessages.LoadedItem(_logger, CurrentItemCount, null);
            }

            if (writer is not null)
            {
                await writer.WriteEndElementAsync().ConfigureAwait(false);
                await writer.WriteEndDocumentAsync().ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            await DisposeOutputAsync(writer).ConfigureAwait(false);
        }

        XmlLogMessages.SingleStreamLoadingCompleted(_logger, CurrentItemCount, CurrentSkippedItemCount, null);
    }



    // Disposes the output at the end of a load. For a real load that is the writer, whose
    // CloseOutput (== !_leaveOpen) closes the stream. A dry run creates no writer, so the
    // destination stream is closed directly when LeaveOpen = false — close-on-complete is
    // honoured either way.
#if NETSTANDARD2_0 || NET462 || NET481
#pragma warning disable CS1998 // only synchronous disposal is available on this TFM
#endif
    private async Task DisposeOutputAsync(XmlWriter? writer)
    {
        if (writer is not null)
        {
#if NETSTANDARD2_0 || NET462 || NET481
            writer.Dispose();
#else
            await writer.DisposeAsync().ConfigureAwait(false);
#endif
        }
        else if (!_leaveOpen)
        {
#if NETSTANDARD2_0 || NET462 || NET481
            _stream.Dispose();
#else
            await _stream.DisposeAsync().ConfigureAwait(false);
#endif
        }
    }
#if NETSTANDARD2_0 || NET462 || NET481
#pragma warning restore CS1998
#endif



    // Creates the output XmlWriter over the destination stream and writes the document
    // prolog + root-element start. Only called for a real (non-dry-run) load.
    private async Task<XmlWriter> CreateDocumentWriterAsync()
    {
        var settings = _writerSettings?.Clone() ?? new XmlWriterSettings { Indent = true };
        settings.CloseOutput = !_leaveOpen;
        settings.Async = true;

        var writer = XmlWriter.Create(_stream, settings);
        var ready = false;
        try
        {
            await writer.WriteStartDocumentAsync().ConfigureAwait(false);
            await writer.WriteStartElementAsync(prefix: null, localName: _rootElementName, ns: null).ConfigureAwait(false);
            ready = true;
            return writer;
        }
        finally
        {
            // If a prolog write threw, the writer never reaches LoadWorkerAsync's finally —
            // dispose it here so the (possibly stream-owning) writer doesn't leak.
            if (!ready)
            {
#if NETSTANDARD2_0 || NET462 || NET481
                writer.Dispose();
#else
                await writer.DisposeAsync().ConfigureAwait(false);
#endif
            }
        }
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
            XmlConvert.VerifyNCName(rootElementName);
        }
        catch (XmlException ex)
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
