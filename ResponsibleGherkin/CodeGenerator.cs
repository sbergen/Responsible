using System.Text;
using Gherkin.Ast;
using Microsoft.CodeAnalysis.CSharp;
using static ResponsibleGherkin.PascalCaseConverter;

namespace ResponsibleGherkin;

public static class CodeGenerator
{


	public static string GenerateFile(
		Feature feature,
		FlavorType flavorType,
		GenerationContext context)
	{
		var builder = new StringBuilder();
		var flavor = Flavor.FromType(flavorType);
		var indentInfo = context.IndentInfo;

		UsingDirectivesGenerator.Generate(flavor)
			.AppendToBuilder(builder, indentInfo);

		builder.AppendLine();

		// TODO: support file-level namespaces?
		builder.AppendLine($"namespace {context.Namespace}");
		builder.AppendLine("{");

		GenerateClassLines(feature, flavor, context)
			.AppendToBuilder(builder, indentInfo, indentBy: 1);

		builder.AppendLine("}");

		return builder.ToString();
	}

	private static IEnumerable<Line> GenerateClassLines(
		Feature feature,
		Flavor flavor,
		GenerationContext context)
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
		Flavor flavor,
		GenerationContext context)
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

		return keyword.IsSupportedKeyword()
			? $"{keyword}({Quote(step.Text)}, Pending){(isLast ? ");" : ",")}"
			: throw new Exception($"Unknown step keyword {keyword}");
	}

	private static string Quote(string str) => SymbolDisplay.FormatLiteral(str, true);
}
