using System.Text;
using Gherkin.Ast;
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
			foreach (var scenarioLine in ScenarioGenerator.Generate(scenario, flavor, context))
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


}
