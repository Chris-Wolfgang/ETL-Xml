using System.Xml;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// Construction-time configuration for <see cref="XmlMultiStreamLoader{TRecord}"/> (ADR-0009): the settings every
/// loader shares (inherited from <see cref="LoaderOptions"/>) plus the writer settings and dry-run mode.
/// </summary>
public sealed record XmlMultiStreamLoaderOptions : LoaderOptions
{
    /// <summary>
    /// Gets the <see cref="XmlWriterSettings"/> each destination's writer is created with. <see langword="null"/> (the
    /// default) uses the loader's defaults.
    /// </summary>
    public XmlWriterSettings? WriterSettings { get; init; }



    /// <summary>
    /// Gets a value indicating whether the loader runs without writing: the source is enumerated and counted, but no
    /// destination is opened or written. Defaults to <see langword="false"/>.
    /// </summary>
    public bool IsDryRun { get; init; }
}
