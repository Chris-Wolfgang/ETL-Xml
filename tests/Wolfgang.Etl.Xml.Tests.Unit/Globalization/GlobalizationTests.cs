using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit.Globalization;

/// <summary>
/// Verifies the library is culture-invariant. XML is an invariant wire format —
/// numbers use <c>.</c> as the decimal separator and dates use ISO-8601 regardless
/// of <see cref="CultureInfo.CurrentCulture"/> — so extraction and loading must
/// produce identical results under any culture, including hostile ones (Turkish
/// dotted-I, German decimal comma, Arabic digit shaping, etc.).
/// </summary>
/// <remarks>
/// Allowlist of intentionally culture-sensitive public methods: <b>none</b>.
/// Every public method on the extractors, loaders, options, and pipeline factories
/// is culture-invariant by contract. If a method ever needs to honour
/// <see cref="CultureInfo.CurrentCulture"/>, add it here and exclude it from the
/// invariance assertions below.
/// </remarks>
public sealed class GlobalizationTests
{
    // en-US is the baseline; the rest are the "hostile" cultures from the AC:
    // tr-TR (dotted-I), de-DE (decimal comma), zh-CN (collation), ar-SA (RTL +
    // Hindi-Arabic digits), ja-JP (full-width digits).
    public static IEnumerable<object[]> Cultures() =>
        new[]
        {
            new object[] { "tr-TR" },
            new object[] { "de-DE" },
            new object[] { "zh-CN" },
            new object[] { "ar-SA" },
            new object[] { "ja-JP" },
        };



    public sealed class Measurement
    {
        public decimal Amount { get; set; }

        public double Ratio { get; set; }

        public DateTime RecordedAt { get; set; }
    }



    private static readonly Measurement[] Sample =
    {
        new()
        {
            Amount = 1234567.89m,
            Ratio = 0.5,
            RecordedAt = new DateTime(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc),
        },
    };



    [Theory]
    [MemberData(nameof(Cultures))]
    public async Task Load_produces_identical_xml_under_any_culture(string cultureName)
    {
        var baseline = await LoadToXmlUnderCultureAsync("en-US").ConfigureAwait(false);
        var underCulture = await LoadToXmlUnderCultureAsync(cultureName).ConfigureAwait(false);

        Assert.Equal(baseline, underCulture);
    }



    [Theory]
    [MemberData(nameof(Cultures))]
    public async Task Round_trip_preserves_values_under_any_culture(string cultureName)
    {
        var xml = await LoadToXmlUnderCultureAsync(cultureName).ConfigureAwait(false);

        var readBack = await RunUnderCultureAsync(cultureName, async () =>
        {
            using var source = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
            var results = new List<Measurement>();
            await foreach (var m in new XmlSingleStreamExtractor<Measurement>(source).ExtractAsync().ConfigureAwait(false))
            {
                results.Add(m);
            }

            return results;
        }).ConfigureAwait(false);

        Assert.Single(readBack);
        Assert.Equal(Sample[0].Amount, readBack[0].Amount);
        Assert.Equal(Sample[0].Ratio, readBack[0].Ratio);
        Assert.Equal(Sample[0].RecordedAt, readBack[0].RecordedAt);
    }



    private static Task<string> LoadToXmlUnderCultureAsync(string cultureName) =>
        RunUnderCultureAsync(cultureName, async () =>
        {
            using var destination = new MemoryStream();
            var loader = new XmlSingleStreamLoader<Measurement>
            (
                destination,
                new XmlSingleStreamLoaderOptions { LeaveOpen = true }
            );

            await loader.LoadAsync(ToAsync(Sample)).ConfigureAwait(false);

            return System.Text.Encoding.UTF8.GetString(destination.ToArray());
        });



    private static async Task<T> RunUnderCultureAsync<T>(string cultureName, Func<Task<T>> action)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        try
        {
            return await action().ConfigureAwait(false);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }



    private static async IAsyncEnumerable<T> ToAsync<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
