using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Reporting;

/// <summary>
/// Emits CI/CD workflow annotations for GitHub Actions and Azure DevOps when test failures occur.
/// </summary>
public sealed class CiAnnotationReporter : ITestReporter
{
    /// <inheritdoc />
    public Task<string> ReportAsync(TestRunResult result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        var isGitHubActions = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));
        var isAzureDevOps = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD"));

        if (result.Status == TestRunStatus.Failed)
        {
            var msg = $"PeasyPilot: {result.Failed} test failure(s) detected out of {result.Passed + result.Failed + result.Skipped} total tests.";

            if (isGitHubActions)
            {
                Console.WriteLine($"::error title=PeasyPilot Failure::{msg}");
            }
            else if (isAzureDevOps)
            {
                Console.WriteLine($"##vso[task.logissue type=error]{msg}");
            }
        }

        return Task.FromResult(string.Empty);
    }
}
