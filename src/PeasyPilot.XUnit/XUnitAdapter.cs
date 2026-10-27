namespace PeasyPilot.XUnit;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

/// <summary>
/// Adapts xUnit to the shared PeasyPilot execution model.
/// </summary>
public sealed class XUnitAdapter : ITestFrameworkAdapter
{
    public string Name => "xUnit";

    public Task<IReadOnlyCollection<TestCase>> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Discovers nothing until real discovery exists (#105); it never invents test cases.
        return Task.FromResult<IReadOnlyCollection<TestCase>>([]);
    }

    public Task<TestRunResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        // Refuses instead of reporting a pass: this adapter does not execute tests yet (#105).
        return Task.FromException<TestRunResult>(new NotSupportedException(
            "The xUnit adapter does not execute tests yet. Use the --assembly runner."));
    }
}
