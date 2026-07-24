using System;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// Describes a single element that failed to deserialize during extraction. Captured in the
/// extractor's <c>Errors</c> collection when <see cref="ErrorHandling.CaptureAndContinue"/> is set.
/// </summary>
public sealed class XmlDeserializationError
{
    internal XmlDeserializationError
    (
        long itemNumber,
        string? rawContent,
        Exception exception
    )
    {
        ItemNumber = itemNumber;
        RawContent = rawContent;
        Exception = exception;
    }


    /// <summary>
    /// Gets the 1-based ordinal of the failed item within the extraction — the element's position
    /// in a single-stream document, or the stream's position in a multi-stream run.
    /// </summary>
    public long ItemNumber { get; }


    /// <summary>Gets the raw XML of the failed element, when the stage captured it; otherwise <see langword="null"/>.</summary>
    public string? RawContent { get; }


    /// <summary>Gets the exception that caused the failure.</summary>
    public Exception Exception { get; }
}
