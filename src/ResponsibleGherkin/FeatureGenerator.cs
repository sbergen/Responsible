using Gherkin.Ast;

namespace ResponsibleGherkin;

public static class FeatureGenerator
{
	public static IEnumerable<Line> Generate(
		Feature feature,
		Flavor flavor,
		Configuration configuration)
	{
		yield return $"public class {feature.Name.ToPascalCase()} : {configuration.BaseClass}";
		yield return "{";

		var scenarios = feature.Children.OfType<Scenario>().ToList();

		// TODO support background

		foreach (var (scenario, isLast) in scenarios
			.Select((s, i) => (s, i == scenarios.Count - 1)))
		{
			foreach (var scenarioLine in ScenarioGenerator.Generate(scenario, flavor, configuration))
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
