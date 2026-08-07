#if NET8_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CsCheck;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// Property / fuzz tests (CsCheck) for the <see cref="XmlSingleStreamExtractor{TRecord}"/> ⇄
/// <see cref="XmlSingleStreamLoader{TRecord}"/> round trip (#120). The short version runs in PR CI
/// (CsCheck's default ~100 cases); the long version runs in <c>fuzz.yaml</c>, which scales the case
/// count via the <c>CsCheck_Time</c> / <c>CsCheck_Iter</c> environment variables CsCheck reads at
/// runtime.
///
/// Marked <c>[Trait("Category", "Fuzz")]</c> so <c>fuzz.yaml</c> can select just these tests.
/// Restricted to net8.0+ (see the csproj) — CsCheck targets net8.0.
/// </summary>
[Trait("Category", "Fuzz")]
public class XmlFuzzTests
{
    // Printable-ASCII fields with NO leading/trailing space. XmlSerializer does not preserve
    // insignificant whitespace at element boundaries on read, so a field like "  a" would round
    // trip to "a" — a distinct XML-whitespace concern, not the serialize/deserialize invariant this
    // fuzzes. Interior spaces, quotes, angle brackets and ampersands ARE exercised (they force
    // XML entity escaping) and must round trip exactly.
    private static readonly Gen<string> GenField =
        Gen.Char[' ', '~'].Array[0, 12]
            .Select(chars => new string(chars).Trim());


    private static readonly Gen<PersonRecord> GenPerson =
        Gen.Select
        (
            GenField,
            GenField,
            Gen.Int[0, 130],
            (first, last, age) => new PersonRecord { FirstName = first, LastName = last, Age = age }
        );


    [Fact]
    public void Extract_after_Load_round_trips_every_record()
    {
        GenPerson.List[0, 40].Sample
        (
            records =>
            {
                var xml = LoadToBytes(records);
                var readBack = ExtractFromBytes(xml);

                if (readBack.Count != records.Count)
                {
                    return false;
                }

                for (var i = 0; i < records.Count; i++)
                {
                    if (!string.Equals(readBack[i].FirstName, records[i].FirstName, StringComparison.Ordinal)
                        || !string.Equals(readBack[i].LastName, records[i].LastName, StringComparison.Ordinal)
                        || readBack[i].Age != records[i].Age)
                    {
                        return false;
                    }
                }

                return true;
            }
        );
    }


    private static byte[] LoadToBytes(IReadOnlyList<PersonRecord> records)
    {
        using var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            new XmlSingleStreamLoaderOptions { LeaveOpen = true }
        );

        loader.LoadAsync(ToAsync(records)).GetAwaiter().GetResult();

        return stream.ToArray();
    }


    private static List<PersonRecord> ExtractFromBytes(byte[] xml)
    {
        using var stream = new MemoryStream(xml);
        var extractor = new XmlSingleStreamExtractor<PersonRecord>(stream);

        var results = new List<PersonRecord>();
        var enumerator = extractor.ExtractAsync().GetAsyncEnumerator();

        try
        {
            while (enumerator.MoveNextAsync().AsTask().GetAwaiter().GetResult())
            {
                results.Add(enumerator.Current);
            }
        }
        finally
        {
            enumerator.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        return results;
    }


    private static async IAsyncEnumerable<PersonRecord> ToAsync(IEnumerable<PersonRecord> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask;
    }
}

#endif
