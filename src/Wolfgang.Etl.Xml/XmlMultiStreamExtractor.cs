using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// Extracts items of type <typeparamref name="TRecord"/> from multiple streams,
/// reading one XML document per stream.
/// </summary>
/// <typeparam name="TRecord">The type of items to extract. Must be <c>notnull</c> and have a parameterless constructor.</typeparam>
/// <remarks>
/// Iterates over an <see cref="IEnumerable{T}"/> of <see cref="Stream"/> instances,
/// deserializing a single <typeparamref name="TRecord"/> from each stream.
/// Each stream is disposed after the item is read.
/// Extraction stops when the enumerable is exhausted or <see cref="ExtractorBase{TSource, TProgress}.MaximumItemCount"/> is reached.
/// </remarks>
/// <example>
/// <code>
/// var streams = Directory.GetFiles("data/", "*.xml").Select(File.OpenRead);
/// var extractor = new XmlMultiStreamExtractor&lt;Person&gt;(streams, logger);
/// await foreach (var person in extractor.ExtractAsync(cancellationToken))
/// {
///     Console.WriteLine(person.Name);
/// }
/// </code>
/// </example>
public sealed class XmlMultiStreamExtractor<TRecord> : ExtractorBase<TRecord, XmlReport>
    where TRecord : notnull, new()
{
    private static readonly string OperationName = $"XML multi-stream extraction of {typeof(TRecord).Name}";
    private readonly IEnumerable<Stream> _streams;
    private static readonly XmlSerializer Serializer = new(typeof(TRecord));
    private readonly ILogger _logger;
    private readonly IProgressTimer? _progressTimer;
    private bool _progressTimerWired;



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlMultiStreamExtractor{TRecord}"/> class.
    /// </summary>
    /// <param name="streams">An enumerable of streams, each containing a single XML document.</param>
    /// <param name="logger">An optional logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="streams"/> is <c>null</c>.
    /// </exception>
    public XmlMultiStreamExtractor
    (
        IEnumerable<Stream> streams,
        ILogger<XmlMultiStreamExtractor<TRecord>>? logger = null
    )
    {
        _streams = streams ?? throw new ArgumentNullException(nameof(streams));
        _logger = logger ?? (ILogger)NullLogger.Instance;

    }



    /// <summary>
    /// Initializes a new instance of the <see cref="XmlMultiStreamExtractor{TRecord}"/> class
    /// with an injected progress timer for testing.
    /// </summary>
    /// <param name="streams">An enumerable of streams, each containing a single XML document.</param>
    /// <param name="logger">An optional logger instance for diagnostic output.</param>
    /// <param name="timer">The progress timer to inject.</param>
    internal XmlMultiStreamExtractor
    (
        IEnumerable<Stream> streams,
        ILogger logger,
        IProgressTimer timer
    )
    {
        _streams = streams ?? throw new ArgumentNullException(nameof(streams));
        _logger = logger ?? (ILogger)NullLogger.Instance;
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));

    }



    /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators — XmlSerializer is synchronous
    protected override async IAsyncEnumerable<TRecord> ExtractWorkerAsync
    (
        [EnumeratorCancellation] CancellationToken token
    )
#pragma warning restore CS1998
    {
        XmlLogMessages.StartingOperation(_logger, OperationName, null);

        var skipBudget = SkipItemCount;
        var streamIndex = 0;

        foreach (var stream in _streams)
        {
            token.ThrowIfCancellationRequested();
            XmlLogMessages.ReadingStream(_logger, streamIndex, null);

            TRecord? item;
            try
            {
                item = (TRecord?)Serializer.Deserialize(stream);
            }
            finally
            {
#if NETSTANDARD2_0 || NET462 || NET481
                stream.Dispose();
#else
                await stream.DisposeAsync().ConfigureAwait(false);
#endif
            }

            streamIndex++;

            if (item is null)
            {
                XmlLogMessages.StreamDeserializedToNull(_logger, streamIndex - 1, null);
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
            XmlLogMessages.ExtractedItemFromStream(_logger, CurrentItemCount, streamIndex - 1, null);

            yield return item;
        }

        XmlLogMessages.MultiStreamExtractionCompleted(_logger, CurrentItemCount, CurrentSkippedItemCount, streamIndex, null);
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
