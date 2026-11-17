using System.Collections.Generic;
using System.IO;
using Wolfgang.Etl.TestKit.Xunit;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// Verifies <see cref="XmlMultiStreamLoader{TRecord}"/> can be run more than once on the same
/// instance (the opt-in <c>IdempotentLoaderContractTests</c>): it asks the stream factory for a new
/// destination per record on every run and carries no state over from the previous run.
/// </summary>
public sealed class XmlMultiStreamLoaderIdempotencyTests
    : IdempotentLoaderContractTests<XmlMultiStreamLoader<PersonRecord>, PersonRecord>
{
    protected override XmlMultiStreamLoader<PersonRecord> CreateSut(int itemCount) =>
        new(_ => new MemoryStream());



    // The loader writes to caller-supplied streams and keeps no collected items of its own, so the
    // contract's collection check does not apply.
    protected override IReadOnlyList<PersonRecord>? TryGetLoadedItems(XmlMultiStreamLoader<PersonRecord> sut) => null;



    // The loader's progress type is XmlReport, not the base Report the contract assumes.
    protected override int GetCurrentItemCount(XmlMultiStreamLoader<PersonRecord> sut) => sut?.CurrentItemCount ?? 0;



    protected override IReadOnlyList<PersonRecord> CreateSourceItems() =>
        new[]
        {
            new PersonRecord { FirstName = "Alice", LastName = "Smith", Age = 30 },
            new PersonRecord { FirstName = "Bob", LastName = "Jones", Age = 25 },
            new PersonRecord { FirstName = "Carol", LastName = "White", Age = 35 },
        };
}
