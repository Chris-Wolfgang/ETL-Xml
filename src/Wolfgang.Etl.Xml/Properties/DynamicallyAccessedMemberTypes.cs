#if !NET5_0_OR_GREATER

using System.ComponentModel;

// ReSharper disable once CheckNamespace
// The namespace is deliberate: the polyfill must live in the framework's own
// namespace for the compiler and the trim analyzer to recognise it.
namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Polyfill of <see cref="DynamicallyAccessedMemberTypes"/> for target frameworks that predate
/// the type. Only the members this library uses are declared; the values match the BCL's.
/// </summary>
[Flags]
[EditorBrowsable(EditorBrowsableState.Never)]
[SuppressMessage("Design", "CA1008:Enums should have zero value", Justification = "Mirrors the subset of the BCL enum this library uses.")]
internal enum DynamicallyAccessedMemberTypes
{
    PublicParameterlessConstructor = 0x0001,
    PublicFields = 0x0020,
    PublicProperties = 0x0200,
}

#endif
