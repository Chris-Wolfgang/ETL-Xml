using System.IO;

namespace Wolfgang.Etl.Xml.Tests.Unit.TestModels;

/// <summary>
/// A multi-stream loader stream factory that counts its calls, so a test can assert how many
/// streams the loader opened (including none).
/// </summary>
internal sealed class CountingStreamFactory
{
    public int CallCount { get; private set; }



    public Stream Create(PersonRecord _)
    {
        CallCount++;
        return new MemoryStream();
    }
}
