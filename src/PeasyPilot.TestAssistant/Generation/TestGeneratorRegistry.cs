using PeasyPilot.TestAssistant.Abstractions;

namespace PeasyPilot.TestAssistant.Generation;

public class TestGeneratorRegistry
{
    private readonly Dictionary<string, ITestGenerator> _generators = new();

    public TestGeneratorRegistry()
    {
        RegisterGenerator(new XUnitTestGenerator());
        RegisterGenerator(new NUnitTestGenerator());
        RegisterGenerator(new TUnitTestGenerator());
    }

    public void RegisterGenerator(ITestGenerator generator)
    {
        _generators[generator.Framework.ToLower()] = generator;
    }

    public ITestGenerator GetGenerator(string framework)
    {
        var key = framework.ToLower();
        if (!_generators.ContainsKey(key))
            throw new InvalidOperationException($"Unknown test framework: {framework}. Supported: {string.Join(", ", _generators.Keys)}");

        return _generators[key];
    }

    public List<string> GetSupportedFrameworks() => _generators.Keys.ToList();

    public bool IsSupported(string framework) => _generators.ContainsKey(framework.ToLower());
}
