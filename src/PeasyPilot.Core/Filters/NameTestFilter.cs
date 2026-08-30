
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;
namespace PeasyPilot.Core.Filters;
/// <summary>
/// Filters tests by name, case-insensitive.
/// </summary>
public sealed class NameTestFilter : ITestFilter
{
    private readonly string _value;

    public NameTestFilter(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        _value = value;
    }

    public bool Matches(TestCase test)
    {
        ArgumentNullException.ThrowIfNull(test);

        return test.Name.Contains(_value, StringComparison.OrdinalIgnoreCase)
            || test.Category.Contains(_value, StringComparison.OrdinalIgnoreCase);
    }
}
