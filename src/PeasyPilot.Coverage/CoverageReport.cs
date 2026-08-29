namespace PeasyPilot.Coverage;

/// <summary>
/// Code coverage information for a test run.
/// </summary>
public class CoverageReport
{
    /// <summary>
    /// Gets or sets the total lines covered.
    /// </summary>
    public int LinesCovered { get; set; }

    /// <summary>
    /// Gets or sets the total lines in code.
    /// </summary>
    public int TotalLines { get; set; }

    /// <summary>
    /// Gets or sets the total branches covered.
    /// </summary>
    public int BranchesCovered { get; set; }

    /// <summary>
    /// Gets or sets the total branches.
    /// </summary>
    public int TotalBranches { get; set; }

    /// <summary>
    /// Gets the line coverage percentage.
    /// </summary>
    public double LineCoveragePercentage
    {
        get => TotalLines > 0 ? (LinesCovered * 100.0) / TotalLines : 0;
    }

    /// <summary>
    /// Gets the branch coverage percentage.
    /// </summary>
    public double BranchCoveragePercentage
    {
        get => TotalBranches > 0 ? (BranchesCovered * 100.0) / TotalBranches : 0;
    }

    /// <summary>
    /// Gets a string representation of the coverage report.
    /// </summary>
    /// <returns>The coverage report as a string.</returns>
    public override string ToString()
    {
        return $"Coverage Report\n" +
               $"  Line Coverage: {LineCoveragePercentage:F2}% ({LinesCovered}/{TotalLines})\n" +
               $"  Branch Coverage: {BranchCoveragePercentage:F2}% ({BranchesCovered}/{TotalBranches})";
    }
}

/// <summary>
/// Provider for collecting code coverage information.
/// </summary>
public interface ICoverageProvider
{
    /// <summary>
    /// Gets the coverage report asynchronously.
    /// </summary>
    /// <returns>The coverage report.</returns>
    Task<CoverageReport> GetCoverageAsync();
}
