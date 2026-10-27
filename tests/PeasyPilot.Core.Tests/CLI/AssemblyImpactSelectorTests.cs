using PeasyPilot.CLI;
using PeasyPilot.XUnit;
using Xunit;

namespace PeasyPilot.Core.Tests.CLI;

public class AssemblyImpactSelectorTests
{
    private const string OrderTests = """
        namespace Shop.Tests;
        public class OrderServiceTests
        {
            [Fact]
            public void Adds() { }
        }
        """;

    private const string PaymentTests = """
        namespace Pay.Tests;
        public class PaymentTests
        {
            [Fact]
            public void Charges() { }
        }
        """;

    private static (string ShopExe, string PayExe) CreateSolution()
    {
        var root = Path.Combine(Path.GetTempPath(), "peasypilot-selector", Guid.NewGuid().ToString("N"));

        var shop = Path.Combine(root, "Shop");
        Directory.CreateDirectory(Path.Combine(shop, "Tests"));
        Directory.CreateDirectory(Path.Combine(shop, "bin", "Debug", "net9.0"));
        File.WriteAllText(Path.Combine(shop, "Shop.csproj"), "<Project />");
        File.WriteAllText(Path.Combine(shop, "Tests", "OrderServiceTests.cs"), OrderTests);
        var shopExe = Path.Combine(shop, "bin", "Debug", "net9.0", "Shop.exe");
        File.WriteAllText(shopExe, string.Empty);

        var pay = Path.Combine(root, "Pay");
        Directory.CreateDirectory(Path.Combine(pay, "Tests"));
        Directory.CreateDirectory(Path.Combine(pay, "bin", "Debug", "net9.0"));
        File.WriteAllText(Path.Combine(pay, "Pay.csproj"), "<Project />");
        File.WriteAllText(Path.Combine(pay, "Tests", "PaymentTests.cs"), PaymentTests);
        var payExe = Path.Combine(pay, "bin", "Debug", "net9.0", "Pay.exe");
        File.WriteAllText(payExe, string.Empty);

        return (shopExe, payExe);
    }

    [Fact]
    public async Task SelectAsync_ChangedTestOfOneProject_SelectsOnlyThatExecutable()
    {
        var (shopExe, payExe) = CreateSolution();

        var selected = await AssemblyImpactSelector.SelectAsync(
            [shopExe, payExe],
            ["Shop/Tests/OrderServiceTests.cs"]);

        XAssert.Equal(shopExe, XAssert.Single(selected));
    }

    [Fact]
    public async Task SelectAsync_NoChangedFiles_SelectsEverything()
    {
        var (shopExe, payExe) = CreateSolution();

        var selected = await AssemblyImpactSelector.SelectAsync([shopExe, payExe], []);

        XAssert.Equal(2, selected.Count);
    }

    [Fact]
    public async Task SelectAsync_ChangeMatchingNoTest_FallsBackToEverything()
    {
        var (shopExe, payExe) = CreateSolution();

        var selected = await AssemblyImpactSelector.SelectAsync([shopExe, payExe], ["docs/readme.md"]);

        XAssert.Equal(2, selected.Count);
    }

    [Fact]
    public async Task SelectAsync_ExecutableWithoutProject_IsAlwaysSelected()
    {
        var loose = Path.Combine(Path.GetTempPath(), "peasypilot-selector", Guid.NewGuid().ToString("N"), "Loose.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(loose)!);
        File.WriteAllText(loose, string.Empty);
        var (shopExe, _) = CreateSolution();

        var selected = await AssemblyImpactSelector.SelectAsync([loose, shopExe], ["Shop/Tests/OrderServiceTests.cs"]);

        XAssert.Contains(loose, selected);
    }
}
