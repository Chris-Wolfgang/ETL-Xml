using System;
using System.IO;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.DocExamples;

/// <summary>
/// Pins the branches of the doc-example harness that the current set of library examples does
/// not reach: a snippet containing <c>yield</c>, a word that first appears inside a longer
/// identifier, and a start directory with no <c>src/Wolfgang.Etl.Xml</c> above it.
/// </summary>
public sealed class DocExampleHelperTests
{
    [Fact]
    public void WrapperSignature_when_code_yields_uses_an_async_iterator()
    {
        var (signature, closer) = DocExampleCompiler.WrapperSignature("yield return \"a\";");

        Assert.Equal("async IAsyncEnumerable<string> Run()", signature);
        Assert.Equal(string.Empty, closer);
    }



    [Fact]
    public void ContainsWord_when_first_occurrence_is_inside_a_longer_word_keeps_searching()
    {
        Assert.True(DocExampleCompiler.ContainsWord("awaiter; await x;", "await"));
    }



    [Fact]
    public void ContainsWord_when_word_only_appears_inside_longer_words_returns_false()
    {
        Assert.False(DocExampleCompiler.ContainsWord("awaiter awaited", "await"));
    }



    [Fact]
    public void LocateSourceDirectory_when_no_ancestor_holds_the_src_project_throws_DirectoryNotFoundException()
    {
        var start = Path.Combine(Path.GetTempPath(), "docex-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(start);
        try
        {
            var ex = Assert.Throws<DirectoryNotFoundException>(() => DocExampleSource.LocateSourceDirectory(start));

            Assert.Contains(start, ex.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(start);
        }
    }
}
