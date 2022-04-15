using System.Text;
using Gherkin.Ast;
using Microsoft.CodeAnalysis.CSharp;
using static ResponsibleGherkin.PascalCaseConverter;

namespace ResponsibleGherkin;

public static class CodeGenerator
{
	private static readonly string[] SupportedKeywords =
	{
		"Given",
		"When",
		"Then",
		"And",
	};

	private static readonly FlavorData UnityFlavorData = new(
		new[]
		{
			"System.Collections",
			"UnityEngine.TestTools",
		},
		"UnityTest",
		"IEnumerator",
		"YieldScenario");

	private static readonly FlavorData NUnitFlavorData = new(
		new[]
		{
			"System.Threading.Tasks",
			"NUnit.Framework",
		},
		"Test",
		"Task",
		"RunScenario");

	private static readonly string[] StandardNamespaces =
	{
		"Responsible",
	};

	private record FlavorData(
		string[] RequiredNamespaces,
		string TestAttribute,
		string ReturnType,
		string RunMethod);

	public enum FlavorType
	{
		Unity,
		NUnit,
	}

	public record UserContext(
		string Namespace,
		string BaseClass,
		int IndentAmount = 1,
		char IndentChar ='\t',
		string ExecutorName = "Executor");

	public static string GenerateFile(
		Feature feature,
		FlavorType flavor,
		UserContext context)
	{
		var builder = new StringBuilder();

		foreach (var usingDirective in GetFlavorData(flavor).RequiredNamespaces
			.Concat(StandardNamespaces)
			.OrderBy(ns => ns)
			.Select(ns => $"using {ns};")
			.Append("using static Responsible.Bdd.Keywords;"))
		{
			builder.AppendLine(usingDirective);
		}

		// Separator between using directives and the rest
		builder.AppendLine();

		// TODO: support file-level namespaces?
		builder.AppendLine($"namespace {context.Namespace}");
		builder.AppendLine("{");

		void AppendIndentedLine(Line line)
		{
			if (line.Content != "")
			{
				builder.Append(context.IndentChar, line.Indent * context.IndentAmount);
			}

			builder.AppendLine(line.Content);
		}

		foreach (var line in GenerateClassLines(feature, GetFlavorData(flavor), context))
		{
			AppendIndentedLine(line.IndentBy(1));
		}

		builder.AppendLine("}");
		return builder.ToString();
	}

	private static IEnumerable<Line> GenerateClassLines(
		Feature feature,
		FlavorData flavor,
		UserContext context)
	{
		yield return $"public class {ConvertToPascalCase(feature.Name)} : {context.BaseClass}";
		yield return "{";

		var scenarios = feature.Children.OfType<Scenario>().ToList();

		// TODO support background

		foreach (var (scenario, isLast) in scenarios
			.Select((s, i) => (s, i == scenarios.Count - 1)))
		{
			foreach (var scenarioLine in GenerateScenarioLines(scenario, flavor, context))
			{
				yield return scenarioLine.IndentBy(1);
			}

			if (!isLast)
			{
				yield return "";
			}
		}

		yield return "}";
	}

	private static IEnumerable<Line> GenerateScenarioLines(
		Scenario scenario,
		FlavorData flavor,
		UserContext context)
	{
		yield return $"[{flavor.TestAttribute}]";

		var methodName = $"{scenario.Keyword.Trim()}_{ConvertToPascalCase(scenario.Name)}";
		yield return $"public {flavor.ReturnType} {methodName}() => this.{context.ExecutorName}.{flavor.RunMethod}(";

		yield return GenerateScenario(scenario).IndentBy(1);

		foreach (var line in GenerateSteps(scenario))
		{
			yield return line.IndentBy(1);
		}
	}

	private static Line GenerateScenario(Scenario scenario) => $"Scenario({Quote(scenario.Name)}),";

	private static IEnumerable<Line> GenerateSteps(StepsContainer stepsContainer)
	{
		var steps = stepsContainer.Steps.ToList();
		return steps.Select((s, i) => GenerateStep(s, i == (steps.Count - 1)));
	}

	private static Line GenerateStep(Step step, bool isLast)
	{
		var keyword = step.Keyword.TrimEnd();

		return SupportedKeywords.Contains(keyword)
			? $"{keyword}({Quote(step.Text)}, Pending){(isLast ? ");" : ",")}"
			: throw new Exception($"Unknown step keyword {keyword}");
	}

	private static FlavorData GetFlavorData(FlavorType type) => type switch
	{
		FlavorType.Unity => UnityFlavorData,
		FlavorType.NUnit => NUnitFlavorData,
		_ => throw new ArgumentOutOfRangeException(
			nameof(type), type, "Invalid flavor of code generation"),
	};

	private static string Quote(string str) => SymbolDisplay.FormatLiteral(str, true);
}
