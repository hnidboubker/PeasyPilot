using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

namespace PeasyPilot.NUnit;
/// <summary>
/// Adapts NUnit to the shared PeasyPilot execution model.
/// </summary>
public sealed class NUnitAdapter : ITestFrameworkAdapter
{
    public string Name => "NUnit";

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
            "The NUnit adapter does not execute tests yet. Use the --assembly runner."));
    }
}
