#if NET10_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VerifyXunit;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// Snapshot / approval tests (Verify) for <see cref="XmlSingleStreamLoader{TRecord}"/>'s and
/// <see cref="XmlMultiStreamLoader{TRecord}"/>'s output (#132). These lock the exact serialized
/// shape — XML declaration, root-element wrapper, element names, indentation, and line
/// structure — so accidental format drift a targeted assertion would miss fails loudly
/// against the committed <c>Snapshots/*.verified.txt</c> baseline.
///
/// The XML string is normalized (CRLF → LF) and split into a line array, which is verified
/// rather than the raw string. Verify then owns the on-disk serialization (LF-terminated), which
/// keeps the snapshot files git/OS-portable. A bare <c>\n</c>-vs-<c>\r\n</c> terminator change is
/// therefore intentionally NOT flagged; what the snapshot captures is structural drift — added,
/// removed, or reordered lines, changed indentation, or changed element content.
///
/// Restricted to net10.0 (see the csproj) — the output is TFM-independent, so one modern-TFM pass
/// is sufficient and keeps a single shared snapshot per test.
/// </summary>
public class XmlSnapshotTests
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);


    private static readonly IReadOnlyList<PersonRecord> People = new[]
    {
        new PersonRecord { FirstName = "Alice", LastName = "Smith", Age = 30 },
        new PersonRecord { FirstName = "Bob", LastName = "Jones", Age = 25 },
        new PersonRecord { FirstName = "Carol", LastName = "White", Age = 42 },
    };


    [Fact]
    public async Task Default_root_element_wraps_records()
    {
        var xml = await LoadSingleStreamAsync(People, options: null).ConfigureAwait(false);

        await Verifier.Verify(SplitLines(xml)).UseDirectory("Snapshots").ConfigureAwait(false);
    }


    [Fact]
    public async Task Custom_root_element_name()
    {
        var options = new XmlSingleStreamLoaderOptions { RootElementName = "People", LeaveOpen = true };

        var xml = await LoadSingleStreamAsync(People, options).ConfigureAwait(false);

        await Verifier.Verify(SplitLines(xml)).UseDirectory("Snapshots").ConfigureAwait(false);
    }


    [Fact]
    public async Task Empty_sequence_writes_empty_root()
    {
        var xml = await LoadSingleStreamAsync(Array.Empty<PersonRecord>(), options: null).ConfigureAwait(false);

        await Verifier.Verify(SplitLines(xml)).UseDirectory("Snapshots").ConfigureAwait(false);
    }


    private static async Task<string> LoadSingleStreamAsync(
        IReadOnlyList<PersonRecord> people,
        XmlSingleStreamLoaderOptions? options)
    {
        using var stream = new MemoryStream();
        var effective = options ?? new XmlSingleStreamLoaderOptions { LeaveOpen = true };
        effective = new XmlSingleStreamLoaderOptions
        {
            RootElementName = effective.RootElementName,
            LeaveOpen = true,
        };

        var loader = new XmlSingleStreamLoader<PersonRecord>(stream, effective);
        await loader.LoadAsync(ToAsync(people)).ConfigureAwait(false);

        // Strip a leading UTF-8 BOM if the writer emitted one, so the snapshot is a clean
        // document; terminator/shape regressions are still captured.
        return Utf8NoBom.GetString(stream.ToArray()).TrimStart('﻿');
    }


    private static string[] SplitLines(string value) =>
        value.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');


    private static async IAsyncEnumerable<T> ToAsync<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}

#endif
