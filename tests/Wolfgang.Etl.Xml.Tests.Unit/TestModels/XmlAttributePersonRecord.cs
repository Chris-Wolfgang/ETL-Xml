using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Wolfgang.Etl.Xml.Tests.Unit.TestModels;

[ExcludeFromCodeCoverage]
[XmlRoot("person")]
public record XmlAttributePersonRecord
{
    [XmlElement("first_name")]
    public string? FirstName { get; set; }

    [XmlElement("last_name")]
    public string? LastName { get; set; }

    [XmlElement("age")]
    public int Age { get; set; }
}
