#if !NET5_0_OR_GREATER

using System.ComponentModel;

// ReSharper disable once CheckNamespace
// The namespace is deliberate: the polyfill must live in the framework's own
// namespace for the compiler and the trim analyzer to recognise it.
namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Polyfill of <see cref="DynamicallyAccessedMembersAttribute"/> for target frameworks
/// that predate the type (it became public in .NET 5).
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[ExcludeFromCodeCoverage]
[AttributeUsage
(
    AttributeTargets.Field | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter |
    AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Method |
    AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
    Inherited = false
)]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "MemberTypes.get is part of the framework's public shape for this attribute; the polyfill must mirror it.")]
internal sealed class DynamicallyAccessedMembersAttribute : Attribute
{
    public DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes memberTypes)
    {
        MemberTypes = memberTypes;
    }



    public DynamicallyAccessedMemberTypes MemberTypes { get; }
}

#endif
