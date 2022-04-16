using Gherkin.Ast;

namespace ResponsibleGherkin;

public static class ScenarioGenerator
{
	public static IEnumerable<Line> Generate(
		Scenario scenario,
		Flavor flavor,
		Configuration configuration)
	{
		yield return $"[{flavor.TestAttribute}]";

		var methodName = $"{scenario.Keyword.Trim()}_{scenario.Name.ToPascalCase()}";
		yield return $"public {flavor.ReturnType} {methodName}() => this.{configuration.ExecutorName}.{flavor.RunMethod}(";

		yield return GenerateScenario(scenario).IndentBy(1);

		foreach (var line in StepsGenerator.Generate(scenario))
		{
			yield return line.IndentBy(1);
		}
	}

	private static Line GenerateScenario(Scenario scenario) => $"Scenario({scenario.Name.Quote()}),";
}
