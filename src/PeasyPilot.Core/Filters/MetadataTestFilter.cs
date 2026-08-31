using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Filters;

/// <summary>
/// Filter that evaluates test cases based on metadata key-value pairs or test classification kind.
/// </summary>
public sealed class MetadataTestFilter : ITestFilter
{
    private readonly string _key;
    private readonly string _value;
    private readonly TestKind? _targetKind;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataTestFilter"/> class for key-value metadata matching.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The expected metadata value.</param>
    public MetadataTestFilter(string key, string value)
    {
        _key = key ?? throw new ArgumentNullException(nameof(key));
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataTestFilter"/> class for TestKind matching.
    /// </summary>
    /// <param name="targetKind">The target test kind.</param>
    public MetadataTestFilter(TestKind targetKind)
    {
        _key = string.Empty;
        _value = string.Empty;
        _targetKind = targetKind;
    }

    /// <inheritdoc />
    public bool Matches(TestCase test)
    {
        ArgumentNullException.ThrowIfNull(test);

        if (_targetKind.HasValue)
        {
            return test.Kind == _targetKind.Value;
        }

        if (test.Metadata.TryGetValue(_key, out var val))
        {
            return string.Equals(val, _value, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}
