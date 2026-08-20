#if !NET5_0_OR_GREATER

using System.ComponentModel;

// ReSharper disable once CheckNamespace
// The namespace is deliberate: the polyfill must live in the framework's own
// namespace for the compiler to recognise it.
namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Polyfill of <see cref="RequiresUnreferencedCodeAttribute"/> for target
/// frameworks where the type is internal.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Message.get and Url are part of the framework's public shape for this attribute; the polyfill must mirror it so trimmer / analyzer tooling that reads these properties via reflection sees the same surface as the real BCL type.")]
internal sealed class RequiresUnreferencedCodeAttribute : Attribute
{
    public RequiresUnreferencedCodeAttribute(string message)
    {
        Message = message;
    }

    public string Message { get; }

    public string? Url { get; set; }
}

#endif
