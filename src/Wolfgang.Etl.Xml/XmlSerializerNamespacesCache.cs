using System.Xml;
using System.Xml.Serialization;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// Holds the shared, empty <see cref="XmlSerializerNamespaces"/> used to suppress the default
/// <c>xsi</c> / <c>xsd</c> namespace declarations on serialized output.
/// </summary>
/// <remarks>
/// Deliberately NON-generic. It previously lived as a <c>static readonly</c> field on the generic
/// <c>XmlSingleStreamLoader&lt;TRecord&gt;</c>, which gave every closed generic type its own
/// identical copy for no benefit — the value does not depend on <c>TRecord</c>. (The other statics
/// on that class, such as the <see cref="XmlSerializer"/> and the metric tags, DO depend on
/// <c>TRecord</c> and correctly stay per-closed-type.)
/// </remarks>
internal static class XmlSerializerNamespacesCache
{
    /// <summary>
    /// A namespace set containing a single empty prefix/namespace pair, which suppresses the
    /// default namespace declarations the serializer would otherwise emit.
    /// </summary>
    internal static readonly XmlSerializerNamespaces Empty =
        new(new[] { new XmlQualifiedName(name: "", ns: "") });
}
