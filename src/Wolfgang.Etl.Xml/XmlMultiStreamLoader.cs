using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// Loads items of type <typeparamref name="TRecord"/> into multiple streams,
/// writing one XML document per stream.
/// </summary>
/// <typeparam name="TRecord">The type of items to load. Must be <c>notnull</c> and have a parameterless constructor.</typeparam>
/// <remarks>
/// For each item in the input sequence, calls a factory function to obtain a <see cref="Stream"/>,
/// serializes the item as a single XML document, and disposes the stream.
/// The factory receives the item being written, allowing stream creation based on item properties
/// (e.g., generating file names from record fields).
/// </remarks>
/// <example>
/// <code>
/// var loader = new XmlMultiStreamLoader&lt;Person&gt;
/// (
///     person => File.Create($"output/{person.Id}.xml")
/// );
/// await loader.LoadAsync(items, cancellationToken);
/// </code>
/// </example>
public sealed class XmlMultiStreamLoader<TRecord> : LoaderBase<TRecord, XmlReport>, ISupportDryRun
    where TRecord : notnull, new()
{
    private static readonly string OperationName = $"XML multi-stream loading of {typeof(TRecord).Name}";
    private static readonly KeyValuePair<string, object?>[] MetricTags =
    {
        new("etl.operation", "load"),
        new("etl.component", "XmlMultiStream"),
        new("etl.record_type", typeof(TRecord).Name),
    };
    private readonly Func<TRecord, Stream> _streamFactory;
    private readonly XmlWriterSettings? _writerSettings;
    private static readonly XmlSerializer Serializer = new(typeof(TRecord));
    private readonly ILogger _logger;
    private readonly IProgressTimer? _progressTimer;
    private bool _progressTimerWired;



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlMultiStreamLoader{TRecord}"/> class.
    /// </summary>
    /// <param name="streamFactory">
    /// A factory function that receives the item to be written and returns a <see cref="Stream"/> to write it to.
    /// The loader will dispose the stream after writing.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="streamFactory"/> is <c>null</c>.
    /// </exception>
    [RequiresUnreferencedCode("XmlMultiStreamLoader serializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlMultiStreamLoader(Func<TRecord, Stream> streamFactory)
    {
        _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
        _logger = NullLogger.Instance;
        _writerSettings = null;
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlMultiStreamLoader{TRecord}"/> class
    /// with a logger.
    /// </summary>
    /// <param name="streamFactory">
    /// A factory function that receives the item to be written and returns a <see cref="Stream"/> to write it to.
    /// The loader will dispose the stream after writing.
    /// </param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="streamFactory"/> or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    [RequiresUnreferencedCode("XmlMultiStreamLoader serializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlMultiStreamLoader
    (
        Func<TRecord, Stream> streamFactory,
        ILogger<XmlMultiStreamLoader<TRecord>> logger
    )
    {
        _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _writerSettings = null;
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlMultiStreamLoader{TRecord}"/> class
    /// with custom writer settings.
    /// </summary>
    /// <param name="streamFactory">
    /// A factory function that receives the item to be written and returns a <see cref="Stream"/> to write it to.
    /// The loader will dispose the stream after writing.
    /// </param>
    /// <param name="writerSettings">The XML writer settings to use for serialization.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="streamFactory"/>, <paramref name="writerSettings"/>, or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    [RequiresUnreferencedCode("XmlMultiStreamLoader serializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlMultiStreamLoader
    (
        Func<TRecord, Stream> streamFactory,
        XmlWriterSettings writerSettings,
        ILogger<XmlMultiStreamLoader<TRecord>> logger
    )
    {
        _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
        _writerSettings = writerSettings ?? throw new ArgumentNullException(nameof(writerSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlMultiStreamLoader{TRecord}"/> class whose
    /// factory returns an <see cref="IBufferWriter{T}"/> of bytes per item instead of a
    /// <see cref="Stream"/> (#8) — serialized bytes flow straight into each buffer writer.
    /// </summary>
    /// <param name="bufferWriterFactory">
    /// A factory that receives the item to be written and returns an <see cref="IBufferWriter{T}"/>
    /// of bytes to write it to.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bufferWriterFactory"/> is <c>null</c>.</exception>
    [RequiresUnreferencedCode("XmlMultiStreamLoader serializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlMultiStreamLoader(Func<TRecord, IBufferWriter<byte>> bufferWriterFactory)
        : this(ToStreamFactory(bufferWriterFactory))
    {
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlMultiStreamLoader{TRecord}"/> class with an
    /// <see cref="IBufferWriter{T}"/>-of-bytes factory (#8) and a logger.
    /// </summary>
    /// <param name="bufferWriterFactory">
    /// A factory that receives the item to be written and returns an <see cref="IBufferWriter{T}"/>
    /// of bytes to write it to.
    /// </param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="bufferWriterFactory"/> or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    [RequiresUnreferencedCode("XmlMultiStreamLoader serializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlMultiStreamLoader
    (
        Func<TRecord, IBufferWriter<byte>> bufferWriterFactory,
        ILogger<XmlMultiStreamLoader<TRecord>> logger
    )
        : this(ToStreamFactory(bufferWriterFactory), logger)
    {
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlMultiStreamLoader{TRecord}"/> class with an
    /// <see cref="IBufferWriter{T}"/>-of-bytes factory (#8) and custom writer settings.
    /// </summary>
    /// <param name="bufferWriterFactory">
    /// A factory that receives the item to be written and returns an <see cref="IBufferWriter{T}"/>
    /// of bytes to write it to.
    /// </param>
    /// <param name="writerSettings">The XML writer settings to use for serialization.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="bufferWriterFactory"/>, <paramref name="writerSettings"/>, or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    [RequiresUnreferencedCode("XmlMultiStreamLoader serializes TRecord via System.Xml.Serialization.XmlSerializer, which uses runtime reflection/Reflection.Emit the trimmer cannot follow. The library is not trim/NativeAOT safe.")]
    public XmlMultiStreamLoader
    (
        Func<TRecord, IBufferWriter<byte>> bufferWriterFactory,
        XmlWriterSettings writerSettings,
        ILogger<XmlMultiStreamLoader<TRecord>> logger
    )
        : this(ToStreamFactory(bufferWriterFactory), writerSettings, logger)
    {
    }



    // Wraps a per-item IBufferWriter<byte> factory as a Stream factory (each buffer writer wrapped
    // in a write-only BufferWriterStream). Validated here so the ArgumentNullException reports the
    // public 'bufferWriterFactory' parameter name.
    private static Func<TRecord, Stream> ToStreamFactory(Func<TRecord, IBufferWriter<byte>> bufferWriterFactory)
    {
        if (bufferWriterFactory is null)
        {
            throw new ArgumentNullException(nameof(bufferWriterFactory));
        }

        // Map a null buffer writer to a null Stream so the loader's existing StreamFactoryReturnedNull
        // guard runs (logging + a consistent InvalidOperationException), rather than the adapter
        // throwing a different exception from a different place.
        return item =>
        {
            var bufferWriter = bufferWriterFactory(item);
            return bufferWriter is null ? null! : new BufferWriterStream(bufferWriter);
        };
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlMultiStreamLoader{TRecord}"/> class
    /// with an injected progress timer for testing.
    /// </summary>
    /// <param name="streamFactory">
    /// A factory function that receives the item to be written and returns a <see cref="Stream"/> to write it to.
    /// </param>
    /// <param name="writerSettings">The XML writer settings to use for serialization.</param>
    /// <param name="logger">An optional logger instance for diagnostic output.</param>
    /// <param name="timer">The progress timer to inject.</param>
    internal XmlMultiStreamLoader
    (
        Func<TRecord, Stream> streamFactory,
        XmlWriterSettings writerSettings,
        ILogger? logger,
        IProgressTimer timer
    )
    {
        _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
        _writerSettings = writerSettings ?? throw new ArgumentNullException(nameof(writerSettings));
        _logger = logger ?? NullLogger.Instance;
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
    }



    /// <summary>
    /// Gets or sets a value indicating whether the loader runs in <em>dry-run</em> mode (#176).
    /// When <see langword="true"/>, the load enumerates the source, honours
    /// <see cref="LoaderBase{TRecord, TProgress}.SkipItemCount"/> /
    /// <see cref="LoaderBase{TRecord, TProgress}.MaximumItemCount"/>, advances the progress
    /// counters, and logs as usual, but never invokes the destination-stream factory and
    /// writes nothing. Defaults to <see langword="false"/>.
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
        // source or opening any destination stream — a pre-cancelled load must consume nothing.
        token.ThrowIfCancellationRequested();

        XmlLogMessages.StartingOperation(_logger, OperationName, null);
        using var operationScope = XmlMetrics.StartOperation(MetricTags);

        var streamIndex = 0;
        var itemNumber = 0;

        await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();
            itemNumber++;

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

            // Dry run (#176): count and log the item exactly as a real load would, but never
            // invoke the stream factory or write — no destination stream is opened.
            if (IsDryRun)
            {
                IncrementCurrentItemCount();
                XmlMetrics.RecordLoaded(MetricTags);
                XmlLogMessages.LoadedItemToStream(_logger, CurrentItemCount, streamIndex, null);
                streamIndex++;
                continue;
            }

            if (await SerializeItemOrHandleErrorAsync(item, itemNumber, streamIndex, token).ConfigureAwait(false))
            {
                streamIndex++;
            }
        }

        XmlLogMessages.MultiStreamLoadingCompleted(_logger, CurrentItemCount, CurrentSkippedItemCount, streamIndex, null);
    }



    // Writes one item to a factory-supplied stream (disposing it), routing a serialization failure
    // through the configurable ErrorPolicy. Returns true when the item was loaded, false when the
    // policy skipped a failed item — each item writes an independent document, so Skip genuinely
    // skips and continues; re-throws the original exception when the policy aborts.
    private async Task<bool> SerializeItemOrHandleErrorAsync(TRecord item, int oneBasedItemNumber, int streamIndex, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var stream = _streamFactory(item);
        if (stream is null)
        {
            XmlLogMessages.StreamFactoryReturnedNull(_logger, streamIndex, null);
            throw new InvalidOperationException($"Stream factory returned null for item number {oneBasedItemNumber}.");
        }

        Exception? error = null;
        try
        {
            SerializeToStream(stream, item);
#if NETSTANDARD2_0 || NET462 || NET481
#pragma warning disable CA2016, MA0040, S8949 // FlushAsync(CancellationToken) does not exist on this TFM
            await stream.FlushAsync().ConfigureAwait(false);
#pragma warning restore CA2016, MA0040, S8949
#else
            await stream.FlushAsync(token).ConfigureAwait(false);
#endif
        }
#pragma warning disable CA1031 // catch general exception to route it through the error policy
        catch (Exception ex) when (ex is not OperationCanceledException)
#pragma warning restore CA1031
        {
            error = ex;
        }
        finally
        {
#if NETSTANDARD2_0 || NET462 || NET481
            stream.Dispose();
#else
            await stream.DisposeAsync().ConfigureAwait(false);
#endif
        }

        if (error is null)
        {
            IncrementCurrentItemCount();
            XmlMetrics.RecordLoaded(MetricTags);
            XmlLogMessages.LoadedItemToStream(_logger, CurrentItemCount, streamIndex, null);
            return true;
        }

        if (HandleItemError(new ItemErrorContext(oneBasedItemNumber, error)) == ItemErrorAction.Abort)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }

        // The policy skipped / dead-lettered a failed item.
        XmlMetrics.RecordErrored(MetricTags);
        return false;
    }



    private void SerializeToStream(Stream stream, TRecord item)
    {
        if (_writerSettings is not null)
        {
            var settings = _writerSettings.Clone();
            settings.CloseOutput = false;
            using var writer = XmlWriter.Create(stream, settings);
            Serializer.Serialize(writer, item);
        }
        else
        {
            Serializer.Serialize(stream, item);
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
