using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

namespace PeasyPilot.TUnit;
/// <summary>
/// Adapts TUnit to the shared PeasyPilot execution model.
/// </summary>
public sealed class TUnitAdapter : ITestFrameworkAdapter
{
    public string Name => "TUnit";

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
            "The TUnit adapter does not execute tests yet. Use the --assembly runner."));
    }
}
