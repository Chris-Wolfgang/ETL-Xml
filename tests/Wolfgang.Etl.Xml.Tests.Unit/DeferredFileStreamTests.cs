using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.Xml.Tests.Unit;

public sealed class DeferredFileStreamTests
{
    [Fact]
    public async Task WriteAsync_when_disposed_before_first_write_throws_ObjectDisposedException_and_creates_no_file()
    {
        var path = Path.Combine(Path.GetTempPath(), $"deferred-{Guid.NewGuid():N}.xml");
        var sut = DeferredFileStream.ForWrite(path);
        sut.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>
        (
            () => sut.WriteAsync(new byte[] { 1 }, 0, 1)
        );

        Assert.False(File.Exists(path));
    }
}
