using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Wolfgang.Etl.Xml.Tests.DocExamples;

/// <summary>
/// Compiles an extracted doc <see cref="DocExample"/> against the real
/// <c>Wolfgang.Etl.Xml</c> assembly. The snippet is wrapped in a synthetic harness
/// that supplies the imports and the placeholder identifiers the illustrative
/// snippets reference (<c>stream</c>, <c>items</c>, <c>logger</c>,
/// <c>cancellationToken</c>) while the actual API calls (<c>new XmlSingleStreamExtractor</c>,
/// <c>ExtractAsync</c>, <c>LoadAsync</c>, the option records, …) bind against the
/// shipped types — so a renamed or removed member turns a stale example into a
/// compile error.
/// </summary>
public static class DocExampleCompiler
{
    /// <summary>
    /// Wraps and compiles <paramref name="example"/>, returning only the
    /// error-severity diagnostics (an empty list means the snippet is valid).
    /// </summary>
    public static IReadOnlyList<Diagnostic> Compile(DocExample example)
    {
        ArgumentNullException.ThrowIfNull(example);
        var source = BuildSource(example);

        var tree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.Latest));

        var compilation = CSharpCompilation.Create(
            assemblyName: "DocExampleScratch",
            syntaxTrees: [tree],
            references: ReferenceAssemblies(),
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Disable));

        return compilation
            .GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();
    }


    private static string BuildSource(DocExample example)
    {
        var (signature, closer) = WrapperSignature(example.Code);

        // #line remaps compiler diagnostics onto the real doc-comment location.
        var location = example.File; // repository-relative, already forward-slashed

        return $$"""
            using System;
            using System.IO;
            using System.Linq;
            using System.Threading;
            using System.Threading.Tasks;
            using System.Collections.Generic;
            using System.Xml;
            using Microsoft.Extensions.Logging;
            using Wolfgang.Etl.Xml;

            namespace DocExamples.Generated
            {
                // A sample record type the examples project over. Carries the members the
                // snippets read (Name, Id).
                internal sealed class Person { public string Name { get; set; } public int Id { get; set; } }

                // Supplies the placeholder identifiers the illustrative snippets reference. These
                // are scaffolding, not the API under test: they are never executed (the snippets are
                // compiled, not run), so their values are irrelevant. Their TYPES are chosen so the
                // real API calls in the snippets resolve exactly as a consumer's would.
                internal abstract class DocExampleContext
                {
                    protected CancellationToken cancellationToken;
                    protected Stream stream;
                    protected IAsyncEnumerable<Person> items;
                    protected ILogger<XmlMultiStreamExtractor<Person>> logger;
                }

                internal sealed class Example : DocExampleContext
                {
                    public {{signature}}
                    {
            #line {{example.Line}} "{{location}}"
            {{example.Code}}
            #line default
                    }{{closer}}
                }
            }
            """;
    }


    // Chooses the wrapper method shape that lets the snippet compile:
    //   - a `yield` snippet must sit in an async-iterator method;
    //   - an `await` snippet needs `async Task`;
    //   - anything else (e.g. a plain construction) is a synchronous `void` body,
    //     which avoids a spurious CS1998 "async method lacks await" on those.
    private static (string Signature, string Closer) WrapperSignature(string code)
    {
        if (ContainsWord(code, "yield"))
        {
            return ("async IAsyncEnumerable<string> Run()", string.Empty);
        }

        if (ContainsWord(code, "await"))
        {
            return ("async Task Run()", string.Empty);
        }

        return ("void Run()", string.Empty);
    }


    private static bool ContainsWord(string code, string word)
    {
        var index = code.IndexOf(word, StringComparison.Ordinal);
        while (index >= 0)
        {
            var before = index == 0 || !char.IsLetterOrDigit(code[index - 1]);
            var afterIndex = index + word.Length;
            var after = afterIndex >= code.Length || !char.IsLetterOrDigit(code[afterIndex]);
            if (before && after)
            {
                return true;
            }

            index = code.IndexOf(word, index + 1, StringComparison.Ordinal);
        }

        return false;
    }


    // The compiler needs the full framework reference set plus the library under test. The
    // trusted-platform-assemblies list is the reference closure of the running test host
    // (net10.0), which already includes the project-referenced Wolfgang.Etl.Xml assembly and
    // its transitive dependencies (Abstractions, Logging.Abstractions).
    private static IReadOnlyList<MetadataReference> ReferenceAssemblies()
    {
        var references = new List<MetadataReference>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var trusted = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? string.Empty;
        foreach (var path in trusted.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            if (path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && seen.Add(path))
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        // Belt-and-braces: guarantee the library under test is referenced even if it is
        // ever loaded from outside the TPA closure.
        var libraryPath = typeof(XmlSingleStreamExtractorOptions).Assembly.Location;
        if (!string.IsNullOrEmpty(libraryPath) && seen.Add(libraryPath))
        {
            references.Add(MetadataReference.CreateFromFile(libraryPath));
        }

        return references;
    }
}
