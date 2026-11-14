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
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Url is part of the framework's public shape for this attribute; the polyfill must mirror it so analyzer tooling that reads these properties sees the same surface as the real BCL type.")]
internal sealed class RequiresDynamicCodeAttribute : Attribute
{
    private readonly string _message;



    public RequiresDynamicCodeAttribute(string message)
    {
        _message = message;
    }



    /// <summary>
    /// Gets the message that explains why the annotated member is not compatible.
    /// </summary>
    /// <remarks>
    /// A get-only property over a field rather than an auto-property: nothing in this
    /// assembly reads <c>Message</c> (tooling reads the constructor argument from
    /// metadata), and ReSharper reports an unread auto-property getter as
    /// UnusedAutoPropertyAccessor.Global.
    /// </remarks>
    public string Message => _message;



    public string? Url { get; set; }
}

#endif
