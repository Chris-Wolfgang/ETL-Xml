using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging.Abstractions;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// Covers the loader constructor overloads and the invalid-root-element path that the
/// contract base and the domain tests don't otherwise exercise — the <c>(stream, logger)</c> /
/// <c>(streamFactory, logger)</c> constructors, the custom-<see cref="XmlWriterSettings"/>
/// serialize branch, and <c>ResolveRootElementName</c>'s NCName validation.
/// </summary>
public sealed class XmlLoaderConstructorCoverageTests
{
    private static readonly PersonRecord[] Sample =
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
    };


    [Fact]
    public async Task SingleStreamLoader_stream_logger_ctor_loads_records()
    {
        using var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            NullLogger<XmlSingleStreamLoader<PersonRecord>>.Instance
        );

        await loader.LoadAsync(ToAsync(Sample)).ConfigureAwait(false);

        Assert.Equal(1, loader.CurrentItemCount);
    }


    [Fact]
    public void SingleStreamLoader_when_rootElementName_is_not_a_valid_NCName_throws_ArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>
        (
            () => new XmlSingleStreamLoader<PersonRecord>
            (
                new MemoryStream(),
                new XmlSingleStreamLoaderOptions { RootElementName = "not a valid name" }
            )
        );

        Assert.Equal("rootElementName", ex.ParamName);
    }


    [Fact]
    public async Task MultiStreamLoader_streamFactory_logger_ctor_loads_records()
    {
        var buffers = new List<MemoryStream>();
        var loader = new XmlMultiStreamLoader<PersonRecord>
        (
            _ =>
            {
                var ms = new MemoryStream();
                buffers.Add(ms);
                return ms;
            },
            NullLogger<XmlMultiStreamLoader<PersonRecord>>.Instance
        );

        await loader.LoadAsync(ToAsync(Sample)).ConfigureAwait(false);

        Assert.Equal(1, loader.CurrentItemCount);
    }


    [Fact]
    public async Task MultiStreamLoader_writerSettings_ctor_serializes_with_custom_settings()
    {
        var buffers = new List<MemoryStream>();
        var loader = new XmlMultiStreamLoader<PersonRecord>
        (
            _ =>
            {
                var ms = new MemoryStream();
                buffers.Add(ms);
                return ms;
            },
            new XmlWriterSettings { Indent = true },
            NullLogger<XmlMultiStreamLoader<PersonRecord>>.Instance
        );

        await loader.LoadAsync(ToAsync(Sample)).ConfigureAwait(false);

        Assert.Equal(1, loader.CurrentItemCount);
        Assert.Single(buffers);
    }


    private static async IAsyncEnumerable<PersonRecord> ToAsync(IEnumerable<PersonRecord> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
