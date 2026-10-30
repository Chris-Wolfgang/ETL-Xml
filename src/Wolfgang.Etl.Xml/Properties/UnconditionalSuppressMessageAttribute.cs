#if !NET5_0_OR_GREATER

using System.ComponentModel;

// ReSharper disable once CheckNamespace
// The namespace is deliberate: the polyfill must live in the framework's own
// namespace for the compiler and the trim analyzer to recognise it.
namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Polyfill of <see cref="UnconditionalSuppressMessageAttribute"/> for target frameworks
/// that predate the type (it became public in .NET 5).
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Category.get, CheckId.get and Justification are part of the framework's public shape for this attribute; the polyfill must mirror it.")]
internal sealed class UnconditionalSuppressMessageAttribute : Attribute
{
    public UnconditionalSuppressMessageAttribute(string category, string checkId)
    {
        Category = category;
        CheckId = checkId;
    }



    public string Category { get; }



    public string CheckId { get; }



    public string? Justification { get; set; }
}

#endif
