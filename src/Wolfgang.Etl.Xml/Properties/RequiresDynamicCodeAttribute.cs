#if !NET7_0_OR_GREATER

using System.ComponentModel;

// ReSharper disable once CheckNamespace
// The namespace is deliberate: the polyfill must live in the framework's own
// namespace for the compiler and the AOT analyzer to recognise it.
namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Polyfill of <see cref="RequiresDynamicCodeAttribute"/> for target frameworks
/// that predate the type (it became public in .NET 7).
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Message.get and Url are part of the framework's public shape for this attribute; the polyfill must mirror it so analyzer tooling that reads these properties sees the same surface as the real BCL type.")]
internal sealed class RequiresDynamicCodeAttribute : Attribute
{
    public RequiresDynamicCodeAttribute(string message)
    {
        Message = message;
    }



    public string Message { get; }



    public string? Url { get; set; }
}

#endif
