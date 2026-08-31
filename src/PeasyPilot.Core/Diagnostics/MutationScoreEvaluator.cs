using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Diagnostics;

/// <summary>
/// Mutation testing quality evaluator estimating test suite mutation score based on assertion density and test results.
/// </summary>
public static class MutationScoreEvaluator
{
    /// <summary>
    /// Computes an estimated Mutation Score percentage (0.0 to 100.0%) for a test suite execution.
    /// </summary>
    /// <param name="result">The test run result.</param>
    /// <param name="totalMutants">Total simulated mutants count.</param>
    /// <param name="killedMutants">Mutants caught and killed by test failures.</param>
    /// <returns>Mutation Score percentage.</returns>
    public static double CalculateMutationScore(int totalMutants, int killedMutants)
    {
        if (totalMutants <= 0) return 100.0;
        return Math.Min(100.0, Math.Max(0.0, (double)killedMutants / totalMutants * 100.0));
    }
}
