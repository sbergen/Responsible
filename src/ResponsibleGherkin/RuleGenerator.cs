using Gherkin.Ast;

namespace ResponsibleGherkin;

using static MultipleItemGenerator;

public static class RuleGenerator
{
	public static IEnumerable<Line> Generate(
		IReadOnlyList<Rule> rules,
		Flavor flavor,
		Configuration configuration) =>
		GenerateMultiple(rules, r => Generate(r, flavor, configuration));

	private static IEnumerable<Line> Generate(
		Rule rule,
		Flavor flavor,
		Configuration configuration)
	{
		yield return $"#region {rule.Keyword} {rule.Name}";

		if (!string.IsNullOrEmpty(rule.Description))
		{
			yield return "";

			foreach (var line in DescriptionGenerator.Generate(rule))
			{
				yield return line;
			}
		}

		yield return "";

		var scenarios = rule.Children.OfType<Scenario>().ToList();
		foreach (var line in ScenarioGenerator.Generate(scenarios, flavor, configuration))
		{
			yield return line;
		}

		yield return "";
		yield return "#endregion";
	}
}
