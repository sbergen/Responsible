using Gherkin.Ast;

namespace ResponsibleGherkin;

public static class SupportedKeywords
{
	private static readonly string[] SupportedSteps =
	{
		"Given",
		"When",
		"Then",
		"And",
		"But",
	};

	private static readonly string[] SupportedScenarios =
	{
		"Scenario",
		"Example",
	};

	public static string MapKeywordToValid(this Step step) => step.Keyword.TrimEnd() switch
	{
		"*" => "And",
		var kw when SupportedSteps.Contains(kw) => kw,
		_ => throw new UnsupportedKeywordException("step", step.Keyword, step.Location),
	};

	public static string MapKeywordToValid(this Scenario scenario) => scenario.Keyword.TrimEnd() switch
	{
		var kw when SupportedScenarios.Contains(kw) => kw,
		_ => throw new UnsupportedKeywordException("scenario", scenario.Keyword, scenario.Location),
	};
}
