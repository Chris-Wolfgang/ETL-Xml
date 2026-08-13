// Polyfill: init-only properties require IsExternalInit, which is absent from
// netstandard2.0, net462, and net481. Declaring it here makes init accessors
// available across all target frameworks without a package dependency.
#if !NET5_0_OR_GREATER
// ReSharper disable once CheckNamespace
// The namespace is deliberate: the polyfill must live in the framework's own
// namespace for the compiler to recognise it.
namespace System.Runtime.CompilerServices
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    internal static class IsExternalInit { }
}
#endif
