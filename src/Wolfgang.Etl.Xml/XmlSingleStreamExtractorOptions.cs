using System.Xml;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// Options for configuring an <see cref="XmlSingleStreamExtractor{TRecord}"/>.
/// </summary>
/// <example>
/// <code>
/// var extractor = new XmlSingleStreamExtractor&lt;Person&gt;
/// (
///     stream,
///     new XmlSingleStreamExtractorOptions { LeaveOpen = false }
/// );
/// </code>
/// </example>
public sealed record XmlSingleStreamExtractorOptions : ExtractorOptions
{
    /// <summary>
    /// Gets or initializes a value indicating whether the stream should remain open
    /// after extraction completes.
    /// </summary>
    /// <remarks>
    /// When <c>true</c> (default) the caller retains responsibility for the stream
    /// lifetime. When <c>false</c> the stream is closed when extraction finishes,
    /// mirroring the behaviour of <see cref="System.IO.StreamReader"/> and
    /// <see cref="System.IO.BinaryReader"/>.
    /// </remarks>
    public bool LeaveOpen { get; init; } = true;



    /// <summary>
    /// Gets the <see cref="XmlReaderSettings"/> the reader is created with. <see langword="null"/> (the default) uses the
    /// extractor's defaults.
    /// </summary>
    public XmlReaderSettings? ReaderSettings { get; init; }
}
