namespace Wolfgang.Etl.Xml.ShadowWorkloads;

/// <summary>
/// A simple production-shaped record the shadow workloads round-trip through the
/// XML extractor/loader. Public with a parameterless constructor as
/// <see cref="System.Xml.Serialization.XmlSerializer"/> requires.
/// </summary>
public sealed class WidgetRecord
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }
}
