namespace Wolfgang.Etl.Xml;

/// <summary>
/// Options for configuring an <see cref="XmlSingleStreamLoader{TRecord}"/>.
/// </summary>
/// <example>
/// <code>
/// var loader = new XmlSingleStreamLoader&lt;Person&gt;
/// (
///     stream,
///     new XmlSingleStreamLoaderOptions
///     {
///         RootElementName = "People",
///         LeaveOpen = false,
///     }
/// );
/// </code>
/// </example>
public sealed class XmlSingleStreamLoaderOptions
{
    /// <summary>
    /// Gets or initializes the name of the XML root element that wraps all serialized items.
    /// </summary>
    /// <remarks>
    /// When <c>null</c> (default) the root element name is derived automatically as
    /// <c>ArrayOf{TypeName}</c> (e.g. <c>ArrayOfPerson</c>). An empty or whitespace
    /// value is not valid and will cause <see cref="XmlSingleStreamLoader{TRecord}"/>
    /// to throw <see cref="System.ArgumentException"/> during construction.
    /// </remarks>
    public string? RootElementName { get; init; }



    /// <summary>
    /// Gets or initializes a value indicating whether the stream should remain open
    /// after loading completes.
    /// </summary>
    /// <remarks>
    /// When <c>true</c> (default) the caller retains responsibility for the stream
    /// lifetime. When <c>false</c> the stream is closed when loading finishes,
    /// mirroring the behaviour of <see cref="System.IO.StreamWriter"/> and
    /// <see cref="System.IO.BinaryWriter"/>.
    /// </remarks>
    public bool LeaveOpen { get; init; } = true;
}
