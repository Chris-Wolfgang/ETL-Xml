// Polyfill: init-only properties require IsExternalInit, which is absent from
// netstandard2.0, net462, and net481. Declaring it here makes init accessors
// available across all target frameworks without a package dependency.
#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    internal static class IsExternalInit { }
}
#endif
