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

		var background = feature.Children
			.OfType<Background>()
			.FirstOrDefault();
		var className =feature.Name.ToPascalCase();
		var interfaces = background != null && flavor.BackgroundFlavor.Interface != null
			? $", {flavor.BackgroundFlavor.Interface}"
			: "";

		yield return $"public class {className} : {configuration.BaseClass}{interfaces}";
		yield return "{";

		var scenarios = feature.Children.OfType<Scenario>().ToList();
		var rules = feature.Children.OfType<Rule>().ToList();
		foreach (var line in
			BackgroundGenerator.Generate(background, flavor, configuration).
			Concat(ScenarioGenerator.Generate(scenarios, flavor, configuration))
			.Concat(RuleGenerator.Generate(rules, flavor, configuration)))
		{
			yield return line.IndentBy(1);
		}

		yield return "}";
	}
}
