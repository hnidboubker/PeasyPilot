namespace PeasyPilot.TUnit.Tests;

using global::TUnit;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.NUnit;
using PeasyPilot.TUnit;
using PeasyPilot.XUnit;

public class FrameworkAdapterTests
{
    [Test]
    public async Task XUnitAdapter_ShouldImplementITestFrameworkAdapter()
    {
        var adapter = new XUnitAdapter();

        await TAssert.Equal("xUnit", adapter.Name);
        await TAssert.That(adapter).IsAssignableTo<ITestFrameworkAdapter>();
    }

    [Test]
    public async Task NUnitAdapter_ShouldImplementITestFrameworkAdapter()
    {
        var adapter = new NUnitAdapter();

        await TAssert.Equal("NUnit", adapter.Name);
        await TAssert.That(adapter).IsAssignableTo<ITestFrameworkAdapter>();
    }

    [Test]
    public async Task TUnitAdapter_ShouldImplementITestFrameworkAdapter()
    {
        var adapter = new TUnitAdapter();

        await TAssert.Equal("TUnit", adapter.Name);
        await TAssert.That(adapter).IsAssignableTo<ITestFrameworkAdapter>();
    }

    [Test]
    public async Task Adapters_ShouldDiscoverAtLeastCurrentTestCases()
    {
        var xunit = new XUnitAdapter();
        var nunit = new NUnitAdapter();
        var tunit = new TUnitAdapter();

        var x = await xunit.DiscoverAsync();
        var n = await nunit.DiscoverAsync();
        var t = await tunit.DiscoverAsync();

        await TAssert.NotNull(x);
        await TAssert.NotNull(n);
        await TAssert.NotNull(t);
    }
}
