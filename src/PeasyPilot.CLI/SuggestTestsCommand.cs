using PeasyPilot.TestAssistant.Analysis;
using PeasyPilot.TestAssistant.Abstractions;
using PeasyPilot.TestAssistant.Models;
using PeasyPilot.TestAssistant.Rendering;
using System.Reflection;
using System.Text.Json;

namespace PeasyPilot.CLI;

public static class SuggestTestsCommand
{
    public static async Task<int> RunAsync(string[] args)
    {
        try
        {
            var assemblyPath = GetArgValue(args, "--assembly", "-a");
            var typeName = GetArgValue(args, "--type", "-t");
            var framework = GetArgValue(args, "--framework", "-fw") ?? "xunit";
            var outputDir = GetArgValue(args, "--output-dir", "-o") ?? "./generated-tests";
            var format = GetArgValue(args, "--format", "-fmt") ?? "json";
            var maxEnumCases = GetArgValue(args, "--max-enum-cases", "-m") ?? "8";
            var force = args.Contains("--force");

            if (string.IsNullOrWhiteSpace(assemblyPath) || string.IsNullOrWhiteSpace(typeName))
            {
                Console.Error.WriteLine("Error: --assembly and --type are required.");
                Console.Error.WriteLine("Usage: peasypilot suggest-tests --assembly <path> --type <FullName> [options]");
                return 1;
            }

            if (!File.Exists(assemblyPath))
            {
                Console.Error.WriteLine($"Error: Assembly not found: {assemblyPath}");
                return 1;
            }

            if (!int.TryParse(maxEnumCases, out var maxEnum))
            {
                maxEnum = 8;
            }

            Directory.CreateDirectory(outputDir);

            var assembly = Assembly.LoadFrom(assemblyPath);
            var targetType = assembly.GetType(typeName) ??
                             assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);

            if (targetType is null)
            {
                Console.Error.WriteLine($"Error: Type '{typeName}' not found in assembly.");
                return 1;
            }

            var options = new TestBatteryAnalysisOptions
            {
                TargetFramework = framework,
                MaxEnumCases = maxEnum
            };

            var analyzer = new ReflectionTestScenarioAnalyzer();
            var proposal = analyzer.Analyze(targetType, options);

            var jsonPath = Path.Combine(outputDir, $"{targetType.Name}.testbattery.json");
            var csPath = Path.Combine(outputDir, $"{targetType.Name}Tests.Proposed.cs");

            if ((File.Exists(jsonPath) || File.Exists(csPath)) && !force)
            {
                Console.Error.WriteLine("Error: Output files already exist. Use --force to overwrite.");
                return 1;
            }

            if (format == "json" || format == "both")
            {
                var json = JsonSerializer.Serialize(proposal, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(jsonPath, json);
                Console.WriteLine($"Generated: {jsonPath}");
            }

            if (format == "cs" || format == "both")
            {
                var renderer = new TestBatteryRendererRegistry().GetRenderer(framework);
                var codeOutput = renderer.Render(proposal, new RenderOptions
                {
                    OutputNamespace = targetType.Namespace
                });
                await File.WriteAllTextAsync(csPath, codeOutput);
                Console.WriteLine($"Generated: {csPath}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static string? GetArgValue(string[] args, string longForm, string shortForm)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if ((args[i] == longForm || args[i] == shortForm) && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
