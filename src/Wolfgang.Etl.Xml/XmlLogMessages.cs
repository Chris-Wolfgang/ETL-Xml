using System;
using Microsoft.Extensions.Logging;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// Cached <see cref="LoggerMessage"/> delegates for high-performance structured logging
/// across all XML extractors and loaders.
/// </summary>
internal static class XmlLogMessages
{
    // ── Shared: Debug ────────────────────────────────────────────────

    internal static readonly Action<ILogger, string, Exception?> StartingOperation =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(1, nameof(StartingOperation)), "Starting {Operation}.");

    internal static readonly Action<ILogger, int, int, Exception?> SkippedItem =
        LoggerMessage.Define<int, int>(LogLevel.Debug, new EventId(2, nameof(SkippedItem)), "Skipped item {SkippedCount} of {SkipTotal}.");

    internal static readonly Action<ILogger, int, Exception?> ReachedMaximumItemCount =
        LoggerMessage.Define<int>(LogLevel.Debug, new EventId(3, nameof(ReachedMaximumItemCount)), "Reached MaximumItemCount of {MaximumItemCount}. Stopping.");



    // ── SingleStream Extractor ───────────────────────────────────────

    internal static readonly Action<ILogger, Exception?> SkippingNullElement =
        LoggerMessage.Define(LogLevel.Debug, new EventId(100, nameof(SkippingNullElement)), "Skipping null element encountered in XML stream.");

    internal static readonly Action<ILogger, int, Exception?> ExtractedItem =
        LoggerMessage.Define<int>(LogLevel.Debug, new EventId(101, nameof(ExtractedItem)), "Extracted item {CurrentItemCount}.");

    internal static readonly Action<ILogger, int, int, Exception?> SingleStreamExtractionCompleted =
        LoggerMessage.Define<int, int>(LogLevel.Information, new EventId(102, nameof(SingleStreamExtractionCompleted)), "XML single-stream extraction completed. Extracted: {ItemCount}, skipped: {SkippedCount}.");



    // ── SingleStream Loader ──────────────────────────────────────────

    internal static readonly Action<ILogger, int, Exception?> LoadedItem =
        LoggerMessage.Define<int>(LogLevel.Debug, new EventId(110, nameof(LoadedItem)), "Loaded item {CurrentItemCount}.");

    internal static readonly Action<ILogger, int, int, Exception?> SingleStreamLoadingCompleted =
        LoggerMessage.Define<int, int>(LogLevel.Information, new EventId(111, nameof(SingleStreamLoadingCompleted)), "XML single-stream loading completed. Loaded: {ItemCount}, skipped: {SkippedCount}.");



    // ── MultiStream Extractor ────────────────────────────────────────

    internal static readonly Action<ILogger, int, Exception?> ReadingStream =
        LoggerMessage.Define<int>(LogLevel.Debug, new EventId(200, nameof(ReadingStream)), "Reading stream {StreamIndex}.");

    internal static readonly Action<ILogger, int, Exception?> StreamDeserializedToNull =
        LoggerMessage.Define<int>(LogLevel.Warning, new EventId(201, nameof(StreamDeserializedToNull)), "Stream {StreamIndex} deserialized to null.");

    internal static readonly Action<ILogger, int, int, Exception?> ExtractedItemFromStream =
        LoggerMessage.Define<int, int>(LogLevel.Debug, new EventId(202, nameof(ExtractedItemFromStream)), "Extracted item {CurrentItemCount} from stream {StreamIndex}.");

    internal static readonly Action<ILogger, int, int, int, Exception?> MultiStreamExtractionCompleted =
        LoggerMessage.Define<int, int, int>(LogLevel.Information, new EventId(203, nameof(MultiStreamExtractionCompleted)), "Multi-stream extraction completed. Extracted: {ItemCount}, skipped: {SkippedCount}, streams: {StreamCount}.");



    // ── MultiStream Loader ───────────────────────────────────────────

    internal static readonly Action<ILogger, int, Exception?> StreamFactoryReturnedNull =
        LoggerMessage.Define<int>(LogLevel.Error, new EventId(210, nameof(StreamFactoryReturnedNull)), "Stream factory returned null for item at index {StreamIndex}.");

    internal static readonly Action<ILogger, int, int, Exception?> LoadedItemToStream =
        LoggerMessage.Define<int, int>(LogLevel.Debug, new EventId(211, nameof(LoadedItemToStream)), "Loaded item {CurrentItemCount} to stream {StreamIndex}.");

    internal static readonly Action<ILogger, int, int, int, Exception?> MultiStreamLoadingCompleted =
        LoggerMessage.Define<int, int, int>(LogLevel.Information, new EventId(212, nameof(MultiStreamLoadingCompleted)), "Multi-stream loading completed. Loaded: {ItemCount}, skipped: {SkippedCount}, streams: {StreamCount}.");
}
