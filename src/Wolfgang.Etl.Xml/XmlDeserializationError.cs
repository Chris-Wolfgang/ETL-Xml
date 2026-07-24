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
        long itemIndex,
        long? recordNumber,
        string? rawContent,
        Exception exception
    )
    {
        ItemIndex = itemIndex;
        RecordNumber = recordNumber;
        RawContent = rawContent;
        Exception = exception;
    }


    /// <summary>Gets the zero-based position of the failed element among all elements seen so far.</summary>
    public long ItemIndex { get; }


    /// <summary>
    /// Gets the source record number of the failed element (1-based), or <see langword="null"/> when
    /// the stage does not track one.
    /// </summary>
    public long? RecordNumber { get; }


    /// <summary>Gets the raw XML of the failed element, when the stage captured it; otherwise <see langword="null"/>.</summary>
    public string? RawContent { get; }


    /// <summary>Gets the exception that caused the failure.</summary>
    public Exception Exception { get; }
}
