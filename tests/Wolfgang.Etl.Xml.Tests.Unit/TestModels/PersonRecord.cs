using System.Diagnostics.CodeAnalysis;

namespace Wolfgang.Etl.Xml.Tests.Unit.TestModels;

[ExcludeFromCodeCoverage]
public record PersonRecord
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int Age { get; set; }
}
