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
///     person => File.Create($"output/{person.Id}.xml"),
///     logger
/// );
/// await loader.LoadAsync(items, cancellationToken);
/// </code>
/// </example>
public sealed class XmlMultiStreamLoader<TRecord> : LoaderBase<TRecord, XmlReport>
    where TRecord : notnull, new()
{
    private static readonly string OperationName = $"XML multi-stream loading of {typeof(TRecord).Name}";
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
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="streamFactory"/> or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
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
    /// Initializes a new instance of the <see cref="XmlMultiStreamLoader{TRecord}"/> class
    /// with an injected progress timer for testing.
    /// </summary>
    /// <param name="streamFactory">
    /// A factory function that receives the item to be written and returns a <see cref="Stream"/> to write it to.
    /// </param>
    /// <param name="writerSettings">The XML writer settings to use for serialization.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <param name="timer">The progress timer to inject.</param>
    internal XmlMultiStreamLoader
    (
        Func<TRecord, Stream> streamFactory,
        XmlWriterSettings writerSettings,
        ILogger logger,
        IProgressTimer timer
    )
    {
        _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
        _writerSettings = writerSettings ?? throw new ArgumentNullException(nameof(writerSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));

    }



    /// <inheritdoc />
    protected override async Task LoadWorkerAsync
    (
        IAsyncEnumerable<TRecord> items,
        CancellationToken token
    )
    {
        XmlLogMessages.StartingOperation(_logger, OperationName, null);

        var streamIndex = 0;

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

            var stream = _streamFactory(item);
            if (stream is null)
            {
                XmlLogMessages.StreamFactoryReturnedNull(_logger, streamIndex, null);
                throw new InvalidOperationException($"Stream factory returned null for item at index {streamIndex}.");
            }

            try
            {
                Serializer.Serialize(stream, item);
#if NETSTANDARD2_0 || NET462 || NET481
#pragma warning disable CA2016, MA0040 // FlushAsync(CancellationToken) not available on this TFM
                await stream.FlushAsync().ConfigureAwait(false);
#pragma warning restore CA2016, MA0040
#else
                await stream.FlushAsync(token).ConfigureAwait(false);
#endif
                IncrementCurrentItemCount();
                streamIndex++;
                XmlLogMessages.LoadedItemToStream(_logger, CurrentItemCount, streamIndex - 1, null);
            }
            finally
            {
#if NETSTANDARD2_0 || NET462 || NET481
                stream.Dispose();
#else
                await stream.DisposeAsync().ConfigureAwait(false);
#endif
            }
        }

        XmlLogMessages.MultiStreamLoadingCompleted(_logger, CurrentItemCount, CurrentSkippedItemCount, streamIndex, null);
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
