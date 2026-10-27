using PeasyPilot.TestAssistant.Catalog;
using Xunit;

namespace PeasyPilot.TestAssistant.Tests.Catalog;

public class TestCatalogScannerTests
{
    private const string Source = """
        namespace Shop.Tests;

        public class OrderTests
        {
            [Fact]
            public void Adds() { }

            [Theory]
            public void Totals() { }

            [Test]
            public void NUnitStyle() { }

            public void Helper() { }
        }
        """;

    [Fact]
    public void ScanSource_FindsOnlyTestAttributedMethods()
    {
        var cases = TestCatalogScanner.ScanSource(Source, "C:/repo/tests/OrderTests.cs");

        Assert.Equal(3, cases.Count);
        Assert.DoesNotContain(cases, c => c.Name.EndsWith("Helper"));
    }

    [Fact]
    public void ScanSource_QualifiesNameWithNamespaceAndClass()
    {
        var cases = TestCatalogScanner.ScanSource(Source, "C:/repo/tests/OrderTests.cs");

        Assert.Equal("Shop.Tests.OrderTests.Adds", cases[0].Name);
    }

    [Fact]
    public void ScanSource_RecordsFileWithForwardSlashes()
    {
        var cases = TestCatalogScanner.ScanSource(Source, @"C:\repo\tests\OrderTests.cs");

        Assert.Equal("C:/repo/tests/OrderTests.cs", cases[0].Metadata["File"]);
    }

    [Fact]
    public void ScanSource_NestedClass_IncludesEnclosingType()
    {
        const string nested = """
            namespace Shop;

            public class Outer
            {
                public class Inner
                {
                    [Fact]
                    public void Works() { }
                }
            }
            """;

        var cases = TestCatalogScanner.ScanSource(nested, "x.cs");

        Assert.Equal("Shop.Outer.Inner.Works", cases.Single().Name);
    }
}
