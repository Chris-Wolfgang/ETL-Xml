using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// The options records (ADR-0009): each stage's record inherits the per-stage-kind base record from
/// Wolfgang.Etl.Abstractions 0.24, the reader/writer settings and dry-run mode travel on the record, and the
/// record constructors apply the inherited settings.
/// </summary>
public class XmlOptionsRecordTests
{
    private static Func<ItemErrorContext, ItemErrorAction> AnyPolicy => _ => default;



    public static IEnumerable<object[]> ExtractorRecords() =>
    [
        [new XmlSingleStreamExtractorOptions()],
        [new XmlMultiStreamExtractorOptions()],
    ];



    public static IEnumerable<object[]> LoaderRecords() =>
    [
        [new XmlSingleStreamLoaderOptions()],
        [new XmlMultiStreamLoaderOptions()],
    ];



    [Theory]
    [MemberData(nameof(ExtractorRecords))]
    public void Extractor_records_inherit_ExtractorOptions(object record)
    {
        Assert.IsAssignableFrom<ExtractorOptions>(record);
    }



    [Theory]
    [MemberData(nameof(LoaderRecords))]
    public void Loader_records_inherit_LoaderOptions(object record)
    {
        Assert.IsAssignableFrom<LoaderOptions>(record);
    }



    [Fact]
    public void XmlSingleStreamExtractor_when_constructed_with_options_applies_the_inherited_settings()
    {
        var policy = AnyPolicy;
        var options = new XmlSingleStreamExtractorOptions
        {
            ReportingInterval = 5,
            SkipItemCount = 2,
            MaximumItemCount = 3,
            ErrorPolicy = policy,
            LeaveOpen = false,
        };

        var sut = new XmlSingleStreamExtractor<PersonRecord>(new MemoryStream(), options);

        Assert.Equal(5, sut.ReportingInterval);
        Assert.Equal(2, sut.SkipItemCount);
        Assert.Equal(3, sut.MaximumItemCount);
        Assert.Same(policy, sut.ErrorPolicy);
    }



    [Fact]
    public void XmlMultiStreamExtractor_when_constructed_with_options_applies_the_inherited_settings()
    {
        var options = new XmlMultiStreamExtractorOptions { SkipItemCount = 4, ReaderSettings = new XmlReaderSettings() };

        var sut = new XmlMultiStreamExtractor<PersonRecord>([new MemoryStream()], options);

        Assert.Equal(4, sut.SkipItemCount);
    }



    [Fact]
    public void XmlSingleStreamLoader_when_constructed_with_options_applies_inherited_settings_and_IsDryRun()
    {
        var options = new XmlSingleStreamLoaderOptions
        {
            ReportingInterval = 5,
            MaximumItemCount = 3,
            IsDryRun = true,
            WriterSettings = new XmlWriterSettings { Indent = true },
        };

        var sut = new XmlSingleStreamLoader<PersonRecord>(new MemoryStream(), options);

        Assert.Equal(5, sut.ReportingInterval);
        Assert.Equal(3, sut.MaximumItemCount);
        Assert.True(sut.IsDryRun);
    }



    [Fact]
    public void XmlMultiStreamLoader_when_constructed_with_options_applies_inherited_settings_and_IsDryRun()
    {
        var options = new XmlMultiStreamLoaderOptions { SkipItemCount = 2, IsDryRun = true };

        var sut = new XmlMultiStreamLoader<PersonRecord>(_ => new MemoryStream(), options);

        Assert.Equal(2, sut.SkipItemCount);
        Assert.True(sut.IsDryRun);
    }



    [Fact]
    public void Stages_when_constructed_without_options_keep_the_base_defaults()
    {
        var defaults = new ExtractorOptions();

        var sut = new XmlMultiStreamExtractor<PersonRecord>([new MemoryStream()]);

        Assert.Equal(defaults.ReportingInterval, sut.ReportingInterval);
        Assert.Equal(defaults.SkipItemCount, sut.SkipItemCount);
        Assert.Equal(defaults.MaximumItemCount, sut.MaximumItemCount);
    }



    [Fact]
    public void Positional_null_after_the_source_still_binds_on_the_multi_stream_stages()
    {
        // (source, ILogger?) has every parameter supplied by these calls and is preferred over the record
        // and settings overloads, which would need default substitution; a compile-time guard.
        var extractor = new XmlMultiStreamExtractor<PersonRecord>([new MemoryStream()], null);
        var loader = new XmlMultiStreamLoader<PersonRecord>(_ => new MemoryStream(), null);

        Assert.NotNull(extractor);
        Assert.NotNull(loader);
    }



    // ---- the nested settings on the records are applied, observed through behaviour

    private static readonly PersonRecord[] Sample = [new() { FirstName = "Alice", LastName = "Smith", Age = 30 }];

    private static byte[] SerializedPeopleDocument()
    {
        using var ms = new MemoryStream();
        new XmlSerializer(typeof(PersonRecord[]), new XmlRootAttribute("ArrayOfPersonRecord")).Serialize(ms, Sample);
        return ms.ToArray();
    }

    private static byte[] SerializedPersonDocument()
    {
        using var ms = new MemoryStream();
        new XmlSerializer(typeof(PersonRecord)).Serialize(ms, Sample[0]);
        return ms.ToArray();
    }

    private static async Task<int> CountAsync<T>(IAsyncEnumerable<T> items)
    {
        var n = 0;
        await foreach (var _ in items)
        {
            n++;
        }

        return n;
    }

    private static bool ChainContains<TException>(Exception ex)
        where TException : Exception
    {
        for (Exception? e = ex; e is not null; e = e.InnerException)
        {
            if (e is TException)
            {
                return true;
            }
        }

        return false;
    }



    [Fact]
    public async Task XmlSingleStreamExtractor_applies_ReaderSettings_from_the_record()
    {
        // A one-character document limit makes the read fail if, and only if, the record's settings reach the reader.
        var options = new XmlSingleStreamExtractorOptions { ReaderSettings = new XmlReaderSettings { MaxCharactersInDocument = 1 } };
        var limited = new XmlSingleStreamExtractor<PersonRecord>(new MemoryStream(SerializedPeopleDocument()), options);
        var unlimited = new XmlSingleStreamExtractor<PersonRecord>(new MemoryStream(SerializedPeopleDocument()), new XmlSingleStreamExtractorOptions());

        var ex = await Assert.ThrowsAnyAsync<Exception>(() => CountAsync(limited.ExtractAsync()));

        Assert.True(ChainContains<XmlException>(ex), ex.ToString());
        Assert.Equal(1, await CountAsync(unlimited.ExtractAsync()));
    }



    [Fact]
    public async Task XmlMultiStreamExtractor_applies_ReaderSettings_from_the_record()
    {
        var options = new XmlMultiStreamExtractorOptions { ReaderSettings = new XmlReaderSettings { MaxCharactersInDocument = 1 } };
        var limited = new XmlMultiStreamExtractor<PersonRecord>([new MemoryStream(SerializedPersonDocument())], options);
        var unlimited = new XmlMultiStreamExtractor<PersonRecord>([new MemoryStream(SerializedPersonDocument())], new XmlMultiStreamExtractorOptions());

        var ex = await Assert.ThrowsAnyAsync<Exception>(() => CountAsync(limited.ExtractAsync()));

        Assert.True(ChainContains<XmlException>(ex), ex.ToString());
        Assert.Equal(1, await CountAsync(unlimited.ExtractAsync()));
    }



    [Fact]
    public async Task XmlSingleStreamLoader_applies_WriterSettings_from_the_record()
    {
        // OmitXmlDeclaration is visible in the first bytes of the output; the default writer emits the declaration.
        var stream = new MemoryStream();
        var options = new XmlSingleStreamLoaderOptions { WriterSettings = new XmlWriterSettings { OmitXmlDeclaration = true }, LeaveOpen = true };

        var sut = new XmlSingleStreamLoader<PersonRecord>(stream, options);
        await sut.LoadAsync(Sample.ToAsyncEnumerable());

        var content = Encoding.UTF8.GetString(stream.ToArray());
        Assert.DoesNotContain("<?xml", content, StringComparison.Ordinal);
        Assert.Contains("<ArrayOfPersonRecord", content, StringComparison.Ordinal);
    }



    [Fact]
    public async Task XmlMultiStreamLoader_applies_WriterSettings_from_the_record()
    {
        var streams = new List<MemoryStream>();
        var options = new XmlMultiStreamLoaderOptions { WriterSettings = new XmlWriterSettings { OmitXmlDeclaration = true } };

        var sut = new XmlMultiStreamLoader<PersonRecord>(_ => { var s = new MemoryStream(); streams.Add(s); return s; }, options);
        await sut.LoadAsync(Sample.ToAsyncEnumerable());

        var content = Encoding.UTF8.GetString(Assert.Single(streams).ToArray());
        Assert.DoesNotContain("<?xml", content, StringComparison.Ordinal);
        Assert.Contains("<PersonRecord", content, StringComparison.Ordinal);
    }
}
