using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PeasyPilot.Core.Models;

namespace PeasyPilot.TestAssistant.Catalog;

/// <summary>
/// Finds test methods in C# source files by their attributes (xUnit, NUnit, TUnit, MSTest),
/// without compiling or running them. Each result records the source file in the <c>File</c> metadata,
/// which the impact analysis uses to select tests.
/// </summary>
public static class TestCatalogScanner
{
    private static readonly HashSet<string> TestAttributeNames = new(StringComparer.Ordinal)
    {
        "Fact", "Theory", "Test", "TestCase", "TestMethod", "DataTestMethod"
    };

    /// <summary>
    /// Scans source files and returns one test case per test method found.
    /// </summary>
    /// <param name="filePaths">The C# files to scan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The test cases found, in file order.</returns>
    public static async Task<IReadOnlyList<TestCase>> ScanFilesAsync(
        IEnumerable<string> filePaths,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePaths);

        var cases = new List<TestCase>();
        foreach (var path in filePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = await File.ReadAllTextAsync(path, cancellationToken);
            cases.AddRange(ScanSource(source, Path.GetFullPath(path)));
        }

        return cases;
    }

    /// <summary>
    /// Scans one source text and returns one test case per test method found.
    /// </summary>
    /// <param name="source">The C# source text.</param>
    /// <param name="filePath">The path recorded in the <c>File</c> metadata.</param>
    /// <returns>The test cases found.</returns>
    public static IReadOnlyList<TestCase> ScanSource(string source, string filePath)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(filePath);

        var root = CSharpSyntaxTree.ParseText(source).GetRoot();
        var normalizedPath = filePath.Replace('\\', '/');

        return root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(IsTestMethod)
            .Select(method => new TestCase
            {
                Name = QualifiedName(method),
                Metadata = new() { ["File"] = normalizedPath }
            })
            .ToList();
    }

    private static bool IsTestMethod(MethodDeclarationSyntax method) =>
        method.AttributeLists
            .SelectMany(list => list.Attributes)
            .Any(attribute => TestAttributeNames.Contains(LastSegment(attribute.Name.ToString())));

    private static string LastSegment(string name) => name.Split('.')[^1];

    private static string QualifiedName(MethodDeclarationSyntax method)
    {
        var parts = new List<string> { method.Identifier.Text };
        for (SyntaxNode? node = method.Parent; node is not null; node = node.Parent)
        {
            switch (node)
            {
                case BaseTypeDeclarationSyntax type:
                    parts.Insert(0, type.Identifier.Text);
                    break;
                case BaseNamespaceDeclarationSyntax ns:
                    parts.Insert(0, ns.Name.ToString());
                    break;
            }
        }

        return string.Join('.', parts);
    }
}
