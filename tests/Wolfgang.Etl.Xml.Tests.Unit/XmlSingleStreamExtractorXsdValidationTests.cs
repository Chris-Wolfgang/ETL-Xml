using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Extensions.Logging.Abstractions;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// XSD validation during extraction (#10). <see cref="XmlSingleStreamExtractor{TRecord}"/> accepts a
/// custom <see cref="XmlReaderSettings"/> and clones it before use, so setting
/// <see cref="XmlReaderSettings.ValidationType"/> to <see cref="ValidationType.Schema"/> with a
/// loaded <see cref="XmlSchemaSet"/> validates the source against the schema as it is read — no
/// validation handler is attached, so a schema violation surfaces as an
/// <see cref="XmlSchemaValidationException"/> from <c>ExtractAsync</c>.
/// </summary>
public sealed class XmlSingleStreamExtractorXsdValidationTests
{
    // Schema for the XmlSerializer output shape: <ArrayOfPersonRecord> of <PersonRecord> elements
    // with string FirstName/LastName and an integer Age. No target namespace — the loader writes the
    // records with an empty namespace.
    private const string PersonSchemaXsd = """
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
          <xs:element name="ArrayOfPersonRecord">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="PersonRecord" minOccurs="0" maxOccurs="unbounded">
                  <xs:complexType>
                    <xs:sequence>
                      <xs:element name="FirstName" type="xs:string" minOccurs="0" />
                      <xs:element name="LastName" type="xs:string" minOccurs="0" />
                      <xs:element name="Age" type="xs:int" />
                    </xs:sequence>
                  </xs:complexType>
                </xs:element>
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """;


    [Fact]
    public async Task ExtractAsync_when_XmlReaderSettings_has_XSD_schema_validates_during_extraction()
    {
        var validXml = await LoadToBytesAsync(new[]
        {
            new PersonRecord { FirstName = "Alice", LastName = "Smith", Age = 30 },
            new PersonRecord { FirstName = "Bob", LastName = "Jones", Age = 25 },
        }).ConfigureAwait(false);

        using var stream = new MemoryStream(validXml);
        var extractor = new XmlSingleStreamExtractor<PersonRecord>
        (
            stream,
            SchemaValidatingSettings(),
            NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance
        );

        var results = new List<PersonRecord>();
        await foreach (var record in extractor.ExtractAsync().ConfigureAwait(false))
        {
            results.Add(record);
        }

        Assert.Equal(2, results.Count);
        Assert.Equal("Alice", results[0].FirstName);
        Assert.Equal(25, results[1].Age);
    }


    [Fact]
    public async Task ExtractAsync_when_source_violates_the_XSD_surfaces_a_schema_validation_error()
    {
        // Age is not an xs:int — the schema-validating reader rejects it as it reads.
        var invalidXml =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
            + "<ArrayOfPersonRecord>"
            + "<PersonRecord><FirstName>Carol</FirstName><LastName>White</LastName><Age>not-a-number</Age></PersonRecord>"
            + "</ArrayOfPersonRecord>";

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidXml));
        var extractor = new XmlSingleStreamExtractor<PersonRecord>
        (
            stream,
            SchemaValidatingSettings(),
            NullLogger<XmlSingleStreamExtractor<PersonRecord>>.Instance
        );

        // XmlSerializer.Deserialize wraps the schema-validation failure that the reader raises
        // as it reads the offending element, so it surfaces as an InvalidOperationException whose
        // inner exception is the XmlSchemaValidationException with the details.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in extractor.ExtractAsync().ConfigureAwait(false))
            {
            }
        }).ConfigureAwait(false);

        Assert.IsType<XmlSchemaValidationException>(ex.InnerException);
    }


    private static XmlReaderSettings SchemaValidatingSettings()
    {
        var schemas = new XmlSchemaSet();
        using var schemaReader = XmlReader.Create(new StringReader(PersonSchemaXsd));
        schemas.Add(targetNamespace: null, schemaReader);

        return new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            Schemas = schemas,
        };
    }


    private static async Task<byte[]> LoadToBytesAsync(IReadOnlyList<PersonRecord> records)
    {
        using var stream = new MemoryStream();
        var loader = new XmlSingleStreamLoader<PersonRecord>
        (
            stream,
            new XmlSingleStreamLoaderOptions { LeaveOpen = true }
        );

        await loader.LoadAsync(ToAsync(records)).ConfigureAwait(false);

        return stream.ToArray();
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
