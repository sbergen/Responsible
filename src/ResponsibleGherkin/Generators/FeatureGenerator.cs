using Gherkin.Ast;
using ResponsibleGherkin.Utilities;

namespace ResponsibleGherkin.Generators;

public static class FeatureGenerator
{
	public static IEnumerable<Line> Generate(
		Feature feature,
		Flavor flavor,
		Configuration configuration)
	{
		foreach (var line in DescriptionGenerator.Generate(feature))
		{
			yield return line;
		}

		yield return $"public class {feature.Name.ToPascalCase()} : {configuration.BaseClass}";
		yield return "{";

		// TODO support background

		var scenarios = feature.Children.OfType<Scenario>().ToList();
		var rules = feature.Children.OfType<Rule>().ToList();
		foreach (var line in ScenarioGenerator
			.Generate(scenarios, flavor, configuration)
			.Concat(RuleGenerator.Generate(rules, flavor, configuration)))
		{
			yield return line.IndentBy(1);
		}

		yield return "}";
	}
}