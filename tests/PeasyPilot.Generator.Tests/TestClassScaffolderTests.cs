using PeasyPilot.Generator.Tests.Fixtures;
using Xunit;

namespace PeasyPilot.Generator.Tests;

public class TestClassScaffolderTests
{
    private readonly TestClassScaffolder _scaffolder = new();

    [Fact]
    public void GenerateTestClass_XUnit_UsesXUnitBaseClassAndAttribute()
    {
        var result = _scaffolder.GenerateTestClass(typeof(SampleService), TestFrameworkKind.XUnit);

        Assert.Contains("class SampleServiceTests : PeasyPilotTestBase", result);
        Assert.Contains("[Fact]", result);
        Assert.Contains("public override async Task InitializeAsync()", result);
    }

    [Fact]
    public void GenerateTestClass_NUnit_UsesNUnitBaseClassAndAttribute()
    {
        var result = _scaffolder.GenerateTestClass(typeof(SampleService), TestFrameworkKind.NUnit);

        Assert.Contains("class SampleServiceTests : PeasyPilotNUnitTestBase", result);
        Assert.Contains("[Test]", result);
        Assert.Contains("[SetUp]", result);
        Assert.Contains("public override void Setup()", result);
    }

    [Fact]
    public void GenerateTestClass_TUnit_UsesTUnitBaseClassAndLifecycleHook()
    {
        var result = _scaffolder.GenerateTestClass(typeof(SampleService), TestFrameworkKind.TUnit);

        Assert.Contains("class SampleServiceTests : PeasyPilotTUnitTestBase", result);
        Assert.Contains("[Test]", result);
        Assert.Contains("public override async ValueTask BeforeEachAsync()", result);
    }

    [Fact]
    public void GenerateTestClass_InterfaceConstructorDependency_IsMockedViaMockFactory()
    {
        var result = _scaffolder.GenerateTestClass(typeof(SampleService), TestFrameworkKind.XUnit);

        Assert.Contains("_repository = (IRepository)new PeasyPilot.Moq.MockFactory().Create(typeof(IRepository));", result);
        Assert.Contains("_sut = new SampleService(_repository);", result);
    }

    [Fact]
    public void GenerateTestClass_NoConstructorDependencies_HasNoMockFactoryUsage()
    {
        var result = _scaffolder.GenerateTestClass(typeof(NoDependencyService), TestFrameworkKind.XUnit);

        Assert.DoesNotContain("MockFactory", result);
        Assert.Contains("_sut = new NoDependencyService();", result);
    }

    [Fact]
    public void GenerateTestClass_IntParameters_GetZeroAndNegativeVariantsPerParameter()
    {
        var result = _scaffolder.GenerateTestClass(typeof(SampleService), TestFrameworkKind.XUnit);

        Assert.Contains("Add_HappyPath", result);
        Assert.Contains("Add_WithAZero", result);
        Assert.Contains("Add_WithANegative", result);
        Assert.Contains("Add_WithBZero", result);
        Assert.Contains("Add_WithBNegative", result);
    }

    [Fact]
    public void GenerateTestClass_NullableStringParameter_GetsNullAndEmptyVariants()
    {
        var result = _scaffolder.GenerateTestClass(typeof(SampleService), TestFrameworkKind.XUnit);

        Assert.Contains("FindName_HappyPath", result);
        Assert.Contains("FindName_WithQueryNull", result);
        Assert.Contains("FindName_WithQueryEmpty", result);
    }

    [Fact]
    public void GenerateTestClass_EnumParameter_GetsOneVariantPerEnumValue()
    {
        var result = _scaffolder.GenerateTestClass(typeof(SampleService), TestFrameworkKind.XUnit);

        Assert.Contains("Echo_WithInputActive", result);
        Assert.Contains("Echo_WithInputInactive", result);
        Assert.Contains("Echo_WithInputPending", result);
        Assert.Contains("Status.Active", result);
    }

    [Fact]
    public void GenerateTestClass_CollectionParameter_GetsEmptyCollectionVariant()
    {
        var result = _scaffolder.GenerateTestClass(typeof(SampleService), TestFrameworkKind.XUnit);

        Assert.Contains("Sum_WithNumbersEmptyCollection", result);
        Assert.Contains("new List<int>()", result);
    }

    [Fact]
    public void GenerateTestClass_PlainPocoParameter_HasHappyPathOnlyNoSpuriousVariants()
    {
        var result = _scaffolder.GenerateTestClass(typeof(SampleService), TestFrameworkKind.XUnit);

        // Widget has no nullable/enum/numeric/collection shape, so only a happy path is expected.
        Assert.Contains("DoSomething_HappyPath", result);
        Assert.DoesNotContain("DoSomething_With", result);
    }

    [Fact]
    public void GenerateTestClass_AsyncMethod_AwaitsAndUnwrapsTaskResult()
    {
        var result = _scaffolder.GenerateTestClass(typeof(SampleService), TestFrameworkKind.XUnit);

        Assert.Contains("var result = await _sut.GetWidgetAsync(id);", result);
    }

    [Fact]
    public void GenerateTestClass_TypeInDifferentNamespace_AddsUsingDirective()
    {
        var result = _scaffolder.GenerateTestClass(typeof(SampleService), TestFrameworkKind.XUnit);

        Assert.Contains("using PeasyPilot.Generator.Tests.Fixtures.Other;", result);
    }

    [Fact]
    public void GenerateTestClass_NonNullableValueTypeResult_LeavesTodoInsteadOfAssertingNotNull()
    {
        var result = _scaffolder.GenerateTestClass(typeof(NoDependencyService), TestFrameworkKind.XUnit);

        Assert.Contains("// TODO: assert the expected value of result (bool).", result);
    }
}
