using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// The multi-stream stages build their reader / writer the same way with and without caller settings (#571),
/// and their defaults match the single-stream stages.
/// </summary>
public class XmlMultiStreamDefaultSettingsTests
{
    private const string DocumentWithDtd =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
        + "<!DOCTYPE PersonRecord [<!ENTITY who \"Alice\">]>"
        + "<PersonRecord><FirstName>Alice</FirstName><LastName>Smith</LastName><Age>30</Age></PersonRecord>";

    private static readonly PersonRecord Alice = new() { FirstName = "Alice", LastName = "Smith", Age = 30 };



    [Fact]
    public async Task LoadAsync_when_WriterSettings_is_null_writes_utf8_with_bom()
    {
        var buffer = await LoadOneAsync(writerSettings: null);

        var preamble = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetPreamble();

        Assert.Equal
        (
            preamble,
            buffer.Take(preamble.Length).ToArray()
        );
    }



    [Fact]
    public async Task LoadAsync_when_WriterSettings_is_null_writes_indented_output()
    {
        var text = DecodeWithoutBom(await LoadOneAsync(writerSettings: null));

        Assert.Contains("\n  <FirstName>Alice</FirstName>", text.Replace("\r\n", "\n"), StringComparison.Ordinal);
    }



    [Fact]
    public async Task LoadAsync_when_WriterSettings_is_null_writes_the_same_bytes_as_the_single_stream_default()
    {
        var defaultBytes = await LoadOneAsync(writerSettings: null);
        var explicitBytes = await LoadOneAsync(new XmlWriterSettings { Indent = true });

        Assert.Equal
        (
            explicitBytes,
            defaultBytes
        );
    }



    [Fact]
    public async Task LoadAsync_when_WriterSettings_are_supplied_uses_them_instead_of_the_defaults()
    {
        var buffer = await LoadOneAsync
        (
            new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                Indent = false,
            }
        );

        Assert.Equal((byte)'<', buffer[0]);
        Assert.DoesNotContain("\n", DecodeWithoutBom(buffer), StringComparison.Ordinal);
    }



    [Fact]
    public async Task LoadAsync_when_WriterSettings_are_supplied_leaves_them_unchanged()
    {
        var settings = new XmlWriterSettings { CloseOutput = true };

        await LoadOneAsync(settings);

        Assert.True(settings.CloseOutput);
    }



    [Fact]
    public async Task ExtractAsync_when_ReaderSettings_is_null_rejects_a_dtd()
    {
        var sut = new XmlMultiStreamExtractor<PersonRecord>(new[] { ToStream(DocumentWithDtd) });

        var exception = await Record.ExceptionAsync(async () => _ = await DrainAsync(sut));

        Assert.NotNull(exception);
        Assert.Contains(Flatten(exception).ToList(), e => e is XmlException);
    }



    [Fact]
    public async Task ExtractAsync_when_ReaderSettings_allow_a_dtd_reads_the_document()
    {
        var sut = new XmlMultiStreamExtractor<PersonRecord>
        (
            new[] { ToStream(DocumentWithDtd) },
            new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse }
        );

        var results = await DrainAsync(sut);

        Assert.Equal
        (
            new[] { Alice },
            results
        );
    }



    [Fact]
    public async Task ExtractAsync_when_ReaderSettings_is_null_reads_a_document_without_a_dtd()
    {
        var bytes = await LoadOneAsync(writerSettings: null);
        var sut = new XmlMultiStreamExtractor<PersonRecord>(new[] { (Stream)new MemoryStream(bytes) });

        var results = await DrainAsync(sut);

        Assert.Equal
        (
            new[] { Alice },
            results
        );
    }



    private static async Task<byte[]> LoadOneAsync(XmlWriterSettings? writerSettings)
    {
        var buffers = new List<byte[]>();
        Func<PersonRecord, Stream> factory = _ => new CapturingMemoryStream(buffers);

        var sut = writerSettings is null
            ? new XmlMultiStreamLoader<PersonRecord>(factory)
            : new XmlMultiStreamLoader<PersonRecord>(factory, writerSettings);

        await sut.LoadAsync(new[] { Alice }.ToAsyncEnumerable());

        return Assert.Single(buffers);
    }



    private static async Task<List<PersonRecord>> DrainAsync(XmlMultiStreamExtractor<PersonRecord> sut)
    {
        var results = new List<PersonRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        return results;
    }



    private static Stream ToStream(string xml) => new MemoryStream(Encoding.UTF8.GetBytes(xml));



    private static string DecodeWithoutBom(byte[] buffer)
    {
        var preamble = Encoding.UTF8.GetPreamble();
        var offset = buffer.Length >= preamble.Length && buffer.Take(preamble.Length).SequenceEqual(preamble)
            ? preamble.Length
            : 0;

        return Encoding.UTF8.GetString(buffer, offset, buffer.Length - offset);
    }



    private static IEnumerable<Exception> Flatten(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            yield return current;
        }
    }
}
