using System.Xml.Serialization;

namespace Wolfgang.Etl.Xml.Tests.Unit.TestModels;

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
