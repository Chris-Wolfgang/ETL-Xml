using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// The single place this library calls <see cref="XmlSerializer"/>, and the trim / Native AOT
/// boundary for it (#293).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="XmlSerializer"/> carries <c>[RequiresUnreferencedCode]</c> and (on .NET 8+)
/// <c>[RequiresDynamicCode]</c>. This library propagates both to its consumers: every
/// public constructor of <see cref="XmlSingleStreamExtractor{TRecord}"/>, <see cref="XmlMultiStreamExtractor{TRecord}"/>,
/// <see cref="XmlSingleStreamLoader{TRecord}"/> and <see cref="XmlMultiStreamLoader{TRecord}"/>, and every
/// <c>EtlPipeline</c> factory that creates one, is annotated with both attributes using the messages below.
/// The serializer itself is only reached from members of those four types, which cannot be instantiated
/// without passing through one of those annotated constructors, so the warnings inside this class are
/// suppressed rather than reported a second time against the library.
/// </para>
/// <para>
/// The record type parameter is annotated with <see cref="RecordMembers"/>, so a trimmed or Native AOT
/// application keeps the record's own public properties, fields and parameterless constructor. Without it the
/// trimmer removed them and the serializer silently wrote empty elements (measured with
/// <c>tests/Wolfgang.Etl.Xml.AotSmoke</c>).
/// </para>
/// </remarks>
internal static class XmlSerialization
{
    /// <summary>
    /// The members of a record type that <see cref="XmlSerializer"/> reads and writes: public properties,
    /// public fields and the public parameterless constructor.
    /// </summary>
    internal const DynamicallyAccessedMemberTypes RecordMembers =
        DynamicallyAccessedMemberTypes.PublicParameterlessConstructor
        | DynamicallyAccessedMemberTypes.PublicFields
        | DynamicallyAccessedMemberTypes.PublicProperties;

    /// <summary>
    /// The <c>[RequiresUnreferencedCode]</c> message on every public entry point.
    /// </summary>
    internal const string UnreferencedCodeMessage =
        "Records are serialized with System.Xml.Serialization.XmlSerializer, which reflects over the record type and every type reachable from it. "
        + "The record type's own public properties, fields and parameterless constructor are preserved; nested, derived ([XmlInclude]) and collection element types are not, "
        + "and the trimmer may remove their members. Reference or root those types (for example with DynamicDependency or a TrimmerRootDescriptor) when trimming.";

    /// <summary>
    /// The <c>[RequiresDynamicCode]</c> message on every public entry point.
    /// </summary>
    internal const string DynamicCodeMessage =
        "Records are serialized with System.Xml.Serialization.XmlSerializer, which generates serialization code at run time. "
        + "Under Native AOT it falls back to its slower reflection-based mode, which works only for members that survive trimming.";

    private const string SuppressionJustification =
        "Reached only from members of the four XML extractor/loader types. Every public constructor of those types and every EtlPipeline factory that creates one "
        + "carries [RequiresUnreferencedCode] and [RequiresDynamicCode], so the requirement is reported at the consumer's call site; "
        + "the record type's public members are kept by [DynamicallyAccessedMembers(RecordMembers)].";



    /// <summary>Creates the serializer for <paramref name="recordType"/>.</summary>
    /// <param name="recordType">The record type to serialize.</param>
    /// <returns>A serializer for <paramref name="recordType"/>.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressionJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressionJustification)]
    internal static XmlSerializer CreateSerializer([DynamicallyAccessedMembers(RecordMembers)] Type recordType) =>
        new(recordType);



    /// <summary>Deserializes one document from <paramref name="reader"/>.</summary>
    /// <param name="serializer">The serializer to use.</param>
    /// <param name="reader">The reader positioned on the element to deserialize.</param>
    /// <returns>The deserialized object, or <see langword="null"/>.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressionJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressionJustification)]
    internal static object? Deserialize(XmlSerializer serializer, XmlReader reader) =>
        serializer.Deserialize(reader);



    /// <summary>Deserializes one document from <paramref name="stream"/>.</summary>
    /// <param name="serializer">The serializer to use.</param>
    /// <param name="stream">The stream holding the document.</param>
    /// <returns>The deserialized object, or <see langword="null"/>.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressionJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressionJustification)]
    internal static object? Deserialize(XmlSerializer serializer, Stream stream) =>
        serializer.Deserialize(stream);



    /// <summary>Serializes <paramref name="item"/> to <paramref name="writer"/>.</summary>
    /// <param name="serializer">The serializer to use.</param>
    /// <param name="writer">The writer to serialize to.</param>
    /// <param name="item">The item to serialize.</param>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressionJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressionJustification)]
    internal static void Serialize(XmlSerializer serializer, XmlWriter writer, object item) =>
        serializer.Serialize(writer, item);



    /// <summary>Serializes <paramref name="item"/> to <paramref name="writer"/> with the given namespaces.</summary>
    /// <param name="serializer">The serializer to use.</param>
    /// <param name="writer">The writer to serialize to.</param>
    /// <param name="item">The item to serialize.</param>
    /// <param name="namespaces">The namespaces to declare on the element.</param>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressionJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressionJustification)]
    internal static void Serialize(XmlSerializer serializer, XmlWriter writer, object item, XmlSerializerNamespaces namespaces) =>
        serializer.Serialize(writer, item, namespaces);



    /// <summary>Serializes <paramref name="item"/> to <paramref name="stream"/>.</summary>
    /// <param name="serializer">The serializer to use.</param>
    /// <param name="stream">The stream to serialize to.</param>
    /// <param name="item">The item to serialize.</param>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressionJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressionJustification)]
    internal static void Serialize(XmlSerializer serializer, Stream stream, object item) =>
        serializer.Serialize(stream, item);
}
