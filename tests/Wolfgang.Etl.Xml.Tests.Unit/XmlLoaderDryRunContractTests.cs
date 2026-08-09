using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.TestKit.Xunit;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// Verifies <see cref="XmlSingleStreamLoader{TRecord}"/> honours the
/// <c>ISupportDryRun</c> contract (#176): in dry-run mode it writes nothing to the
/// output stream, and in a real run it does.
/// </summary>
public sealed class XmlSingleStreamLoaderDryRunContractTests
    : SupportsDryRunContractTests<XmlSingleStreamLoader<PersonRecord>>
{
    private static readonly PersonRecord[] Sample =
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
    };


    protected override XmlSingleStreamLoader<PersonRecord> CreateSut() =>
        new(new MemoryStream());


    protected override async Task<bool> RunAndReportSideEffectAsync(bool isDryRun)
    {
        using var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            new XmlSingleStreamLoaderOptions { LeaveOpen = true }
        )
        {
            IsDryRun = isDryRun,
        };

        await loader.LoadAsync(Sample.ToAsyncEnumerable()).ConfigureAwait(false);

        return stream.ToArray().Length > 0;
    }


    [Xunit.Fact]
    public async Task DryRun_with_LeaveOpen_false_still_closes_the_destination_stream()
    {
        var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            new XmlSingleStreamLoaderOptions { LeaveOpen = false }
        )
        {
            IsDryRun = true,
        };

        await loader.LoadAsync(Sample.ToAsyncEnumerable()).ConfigureAwait(false);

        // Nothing was written, but LeaveOpen = false must still close the stream on completion.
        Xunit.Assert.Throws<System.ObjectDisposedException>(() => stream.Position);
    }
}


/// <summary>
/// Verifies <see cref="XmlMultiStreamLoader{TRecord}"/> honours the
/// <c>ISupportDryRun</c> contract (#176): in dry-run mode it never invokes the
/// destination-stream factory or writes, and in a real run it does.
/// </summary>
public sealed class XmlMultiStreamLoaderDryRunContractTests
    : SupportsDryRunContractTests<XmlMultiStreamLoader<PersonRecord>>
{
    private static readonly PersonRecord[] Sample =
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
    };


    protected override XmlMultiStreamLoader<PersonRecord> CreateSut() =>
        new(_ => new MemoryStream());


    protected override async Task<bool> RunAndReportSideEffectAsync(bool isDryRun)
    {
        var buffers = new List<MemoryStream>();
        var loader = new XmlMultiStreamLoader<PersonRecord>
        (
            _ =>
            {
                var ms = new MemoryStream();
                buffers.Add(ms);
                return ms;
            }
        )
        {
            IsDryRun = isDryRun,
        };

        await loader.LoadAsync(Sample.ToAsyncEnumerable()).ConfigureAwait(false);

        return buffers.Exists(b => b.ToArray().Length > 0);
    }
}
