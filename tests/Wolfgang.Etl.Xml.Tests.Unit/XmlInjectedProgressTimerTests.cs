using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Wolfgang.Etl.TestKit.Xunit;
using Wolfgang.Etl.Xml.Tests.Unit.TestModels;
using Xunit;
using static Wolfgang.Etl.Xml.Tests.Unit.TestModels.XmlTestData;

namespace Wolfgang.Etl.Xml.Tests.Unit;

/// <summary>
/// Pins the <c>CreateProgressTimer</c> override of each stage: a timer injected through the
/// internal constructor is the one the stage subscribes to, so firing it mid-run reports progress.
/// The contract base attaches its own manual timer and never reaches this override, which left the
/// subscription unasserted (Stryker reported its mutants as NoCoverage).
/// </summary>
public sealed class XmlInjectedProgressTimerTests
{
    private static readonly PersonRecord[] TwoPeople = People.Take(2).ToArray();


    [Fact]
    public async Task SingleStreamExtractor_when_the_injected_timer_fires_reports_progress()
    {
        using var source = await BuildXmlAsync(TwoPeople);
        var timer = new ManualProgressTimer();
        var sut = new XmlSingleStreamExtractor<PersonRecord>(source, new XmlReaderSettings(), timer);
        var reports = new List<XmlReport>();

        await using (var enumerator = sut.ExtractAsync(new SynchronousProgress<XmlReport>(reports.Add)).GetAsyncEnumerator())
        {
            Assert.True(await enumerator.MoveNextAsync());
            timer.Fire();
            while (await enumerator.MoveNextAsync())
            {
            }
        }

        Assert.Contains(reports, r => r.CurrentItemCount == 1);
    }


    [Fact]
    public async Task MultiStreamExtractor_when_the_injected_timer_fires_reports_progress()
    {
        var timer = new ManualProgressTimer();
        var sut = new XmlMultiStreamExtractor<PersonRecord>
        (
            SerializeEach(TwoPeople),
            new XmlReaderSettings(),
            timer,
            logger: null
        );
        var reports = new List<XmlReport>();

        await using (var enumerator = sut.ExtractAsync(new SynchronousProgress<XmlReport>(reports.Add)).GetAsyncEnumerator())
        {
            Assert.True(await enumerator.MoveNextAsync());
            timer.Fire();
            while (await enumerator.MoveNextAsync())
            {
            }
        }

        Assert.Contains(reports, r => r.CurrentItemCount == 1);
    }


    [Fact]
    public async Task SingleStreamLoader_when_the_injected_timer_fires_reports_progress()
    {
        using var destination = new MemoryStream();
        var timer = new ManualProgressTimer();
        var sut = new XmlSingleStreamLoader<PersonRecord>(destination, new XmlWriterSettings(), timer);
        var reports = new List<XmlReport>();

        await sut.LoadAsync(FireAfterFirstAsync(timer), new SynchronousProgress<XmlReport>(reports.Add));

        Assert.Contains(reports, r => r.CurrentItemCount == 1);
    }


    [Fact]
    public async Task MultiStreamLoader_when_the_injected_timer_fires_reports_progress()
    {
        var timer = new ManualProgressTimer();
        var sut = new XmlMultiStreamLoader<PersonRecord>
        (
            _ => new MemoryStream(),
            new XmlWriterSettings(),
            timer,
            logger: null
        );
        var reports = new List<XmlReport>();

        await sut.LoadAsync(FireAfterFirstAsync(timer), new SynchronousProgress<XmlReport>(reports.Add));

        Assert.Contains(reports, r => r.CurrentItemCount == 1);
    }


    // Yields one record, fires the timer once the loader has asked for the next one (so the first
    // record is loaded and counted), then yields the second.
    private static async IAsyncEnumerable<PersonRecord> FireAfterFirstAsync(ManualProgressTimer timer)
    {
        await Task.Yield();
        yield return TwoPeople[0];
        timer.Fire();
        yield return TwoPeople[1];
    }
}
