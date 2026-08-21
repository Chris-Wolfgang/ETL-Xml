using System.Diagnostics.CodeAnalysis;

namespace Wolfgang.Etl.Xml.Benchmarks;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Properties are populated by object initializer and consumed by XmlSerializer via reflection; the getters have no direct reader in source.")]
public class BenchmarkPerson
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int Age { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
}
