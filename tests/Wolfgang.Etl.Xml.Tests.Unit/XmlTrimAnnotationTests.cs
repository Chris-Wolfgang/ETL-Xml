using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// The trim / Native AOT contract (#293). XmlSerialization suppresses XmlSerializer's IL2026 / IL3050
/// warnings on the grounds that every public way to reach it is annotated. These tests hold that invariant:
/// a new public constructor or pipeline factory without the annotations fails here instead of silently
/// widening the suppression. Attributes are matched by name because on net462 / netstandard2.0 they are
/// the library's internal polyfills, not the BCL types.
/// </summary>
public class XmlTrimAnnotationTests
{
    private const string RequiresUnreferencedCode = "RequiresUnreferencedCodeAttribute";
    private const string RequiresDynamicCode = "RequiresDynamicCodeAttribute";
    private const string DynamicallyAccessedMembers = "DynamicallyAccessedMembersAttribute";



    public static IEnumerable<object[]> StageTypes() =>
    [
        [typeof(XmlSingleStreamExtractor<>)],
        [typeof(XmlMultiStreamExtractor<>)],
        [typeof(XmlSingleStreamLoader<>)],
        [typeof(XmlMultiStreamLoader<>)],
    ];



    public static IEnumerable<object[]> FactoryTypes() =>
    [
        [typeof(EtlPipelineXmlSourceExtensions)],
        [typeof(EtlPipelineXmlSinkExtensions)],
    ];



    [Theory]
    [MemberData(nameof(StageTypes))]
    public void Public_constructors_when_inspected_carry_RequiresUnreferencedCode_and_RequiresDynamicCode(Type stageType)
    {
        var type = stageType ?? throw new ArgumentNullException(nameof(stageType));

        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        Assert.NotEmpty(constructors);
        Assert.All
        (
            constructors,
            ctor => Assert.Equal
            (
                new[] { RequiresDynamicCode, RequiresUnreferencedCode },
                AnnotationNames(ctor)
            )
        );
    }



    [Theory]
    [MemberData(nameof(StageTypes))]
    public void Record_type_parameter_when_inspected_carries_DynamicallyAccessedMembers(Type stageType)
    {
        var type = stageType ?? throw new ArgumentNullException(nameof(stageType));

        var recordParameter = type.GetGenericArguments().Single();

        Assert.Contains
        (
            DynamicallyAccessedMembers,
            recordParameter.GetCustomAttributes(inherit: false).Select(a => a.GetType().Name)
        );
    }



    [Theory]
    [MemberData(nameof(FactoryTypes))]
    public void Pipeline_factories_when_inspected_carry_both_annotations_and_DynamicallyAccessedMembers(Type factoryType)
    {
        var type = factoryType ?? throw new ArgumentNullException(nameof(factoryType));

        var factories = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

        Assert.NotEmpty(factories);
        Assert.All
        (
            factories,
            factory =>
            {
                Assert.Equal
                (
                    new[] { RequiresDynamicCode, RequiresUnreferencedCode },
                    AnnotationNames(factory)
                );
                Assert.Contains
                (
                    DynamicallyAccessedMembers,
                    factory.GetGenericArguments().Single().GetCustomAttributes(inherit: false).Select(a => a.GetType().Name)
                );
            }
        );
    }



    private static string[] AnnotationNames(MemberInfo member) =>
        member
            .GetCustomAttributes(inherit: false)
            .Select(a => a.GetType().Name)
            .Where(name => name is RequiresUnreferencedCode or RequiresDynamicCode)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
}
