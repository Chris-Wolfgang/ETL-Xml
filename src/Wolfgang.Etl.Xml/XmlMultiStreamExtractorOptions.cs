using System.Xml;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// Construction-time configuration for <see cref="XmlMultiStreamExtractor{TRecord}"/> (ADR-0009): the settings every
/// extractor shares (inherited from <see cref="ExtractorOptions"/>) plus the reader settings.
/// </summary>
public sealed record XmlMultiStreamExtractorOptions : ExtractorOptions
{
    /// <summary>
    /// Gets the <see cref="XmlReaderSettings"/> each stream's reader is created with. <see langword="null"/> (the default)
    /// uses the extractor's defaults, the same as <see cref="XmlSingleStreamExtractor{TRecord}"/>:
    /// <see cref="DtdProcessing.Prohibit"/> and no <see cref="XmlResolver"/>, on every target framework.
    /// </summary>
    /// <remarks>
    /// The settings are cloned for each stream, and <see cref="XmlReaderSettings.CloseInput"/> is always forced to
    /// <see langword="false"/>. Supplied settings replace the defaults as a whole.
    /// </remarks>
    public XmlReaderSettings? ReaderSettings { get; init; }
}
