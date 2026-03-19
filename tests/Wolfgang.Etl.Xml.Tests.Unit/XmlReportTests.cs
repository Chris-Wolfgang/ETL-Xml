using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

public class XmlReportTests
{
    [Fact]
    public void Constructor_sets_CurrentItemCount()
    {
        var report = new XmlReport(42, 5);

        Assert.Equal(42, report.CurrentItemCount);
    }



    [Fact]
    public void Constructor_sets_CurrentSkippedItemCount()
    {
        var report = new XmlReport(42, 5);

        Assert.Equal(5, report.CurrentSkippedItemCount);
    }



    [Fact]
    public void Constructor_when_zero_values_sets_properties()
    {
        var report = new XmlReport(0, 0);

        Assert.Equal(0, report.CurrentItemCount);
        Assert.Equal(0, report.CurrentSkippedItemCount);
    }



    [Fact]
    public void Equality_when_same_values_returns_true()
    {
        var report1 = new XmlReport(10, 3);
        var report2 = new XmlReport(10, 3);

        Assert.Equal(report1, report2);
    }



    [Fact]
    public void Equality_when_different_values_returns_false()
    {
        var report1 = new XmlReport(10, 3);
        var report2 = new XmlReport(10, 4);

        Assert.NotEqual(report1, report2);
    }
}
