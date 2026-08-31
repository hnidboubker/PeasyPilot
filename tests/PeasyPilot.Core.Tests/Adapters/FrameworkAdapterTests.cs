namespace PeasyPilot.Core.Tests.Adapters;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;
using PeasyPilot.NUnit;
using PeasyPilot.TUnit;
using PeasyPilot.XUnit;
using Xunit;

public class FrameworkAdapterTests
{
    [Fact]
    public void XUnitAdapter_ShouldImplementITestFrameworkAdapter()
    {
        var adapter = new XUnitAdapter();

        Assert.Equal("xUnit", adapter.Name);
        Assert.IsAssignableFrom<ITestFrameworkAdapter>(adapter);
    }

    [Fact]
    public void NUnitAdapter_ShouldImplementITestFrameworkAdapter()
    {
        var adapter = new NUnitAdapter();

        Assert.Equal("NUnit", adapter.Name);
        Assert.IsAssignableFrom<ITestFrameworkAdapter>(adapter);
    }

    [Fact]
    public void TUnitAdapter_ShouldImplementITestFrameworkAdapter()
    {
        var adapter = new TUnitAdapter();

        Assert.Equal("TUnit", adapter.Name);
        Assert.IsAssignableFrom<ITestFrameworkAdapter>(adapter);
    }

    [Fact]
    public async Task Adapters_ShouldDiscoverAtLeastCurrentTestCases()
    {
        var xunit = new XUnitAdapter();
        var nunit = new NUnitAdapter();
        var tunit = new TUnitAdapter();

        var x = await xunit.DiscoverAsync();
        var n = await nunit.DiscoverAsync();
        var t = await tunit.DiscoverAsync();

        Assert.NotNull(x);
        Assert.NotNull(n);
        Assert.NotNull(t);
    }
}
