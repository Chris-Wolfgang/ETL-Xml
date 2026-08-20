using System.Diagnostics.CodeAnalysis;

namespace Wolfgang.Etl.Xml.ShadowWorkloads;

/// <summary>
/// A simple production-shaped record the shadow workloads round-trip through the
/// XML extractor/loader. Public with a parameterless constructor as
/// <see cref="System.Xml.Serialization.XmlSerializer"/> requires.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Properties are populated by object initializer and consumed by XmlSerializer via reflection; the getters have no direct reader in source.")]
public sealed class WidgetRecord
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }
}
