namespace PeasyPilot.BDD;

using System.Collections.Generic;
using System.Linq;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

/// <summary>
/// Feature builder for organizing BDD scenarios.
/// </summary>
public class Feature
{
    /// <summary>
    /// Gets the feature name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the scenarios.
    /// </summary>
    public IReadOnlyList<Scenario> Scenarios { get; private set; }

    private readonly List<Scenario> _scenarios = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Feature"/> class.
    /// </summary>
    /// <param name="name">The feature name.</param>
    public Feature(string name)
    {
        Name = name;
        Scenarios = _scenarios.AsReadOnly();
    }

    /// <summary>
    /// Adds a scenario to the feature.
    /// </summary>
    /// <param name="name">The scenario name.</param>
    /// <returns>The scenario for configuration.</returns>
    public Scenario AddScenario(string name)
    {
        var scenario = new Scenario(name);
        _scenarios.Add(scenario);
        return scenario;
    }

    /// <summary>
    /// Executes all scenarios in the feature asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ExecuteAsync()
    {
        foreach (var scenario in _scenarios)
        {
            await scenario.ExecuteAsync();
        }
    }

    /// <summary>
    /// Gets a string representation of the feature in Gherkin format.
    /// </summary>
    /// <returns>The Gherkin representation.</returns>
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Feature: {Name}");
        sb.AppendLine();

        foreach (var scenario in _scenarios)
        {
            sb.AppendLine(scenario.ToString());
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts all scenarios in this feature into a collection of unified <see cref="TestCase"/> instances.
    /// </summary>
    /// <returns>Collection of unified test cases.</returns>
    public IReadOnlyCollection<TestCase> ToTestCases()
    {
        return _scenarios.Select(s => s.ToTestCase(Name)).ToList();
    }

    /// <summary>
    /// Executes all scenarios in this feature and returns an aggregated <see cref="TestRunResult"/>.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Aggregated test run result.</returns>
    public async Task<TestRunResult> ExecuteAndAsTestRunResultAsync(CancellationToken cancellationToken = default)
    {
        var start = DateTime.UtcNow;
        var results = new List<TestResult>();

        foreach (var scenario in _scenarios)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var res = await scenario.ExecuteAndAsTestResultAsync(Name);
            results.Add(res);
        }

        var passed = results.Count(r => r.Status == TestRunStatus.Passed);
        var failed = results.Count(r => r.Status == TestRunStatus.Failed);
        var skipped = results.Count(r => r.Status == TestRunStatus.Skipped);

        return new TestRunResult
        {
            Passed = passed,
            Failed = failed,
            Skipped = skipped,
            Duration = DateTime.UtcNow - start,
            Status = failed > 0 ? TestRunStatus.Failed : TestRunStatus.Passed
        };
    }
}
