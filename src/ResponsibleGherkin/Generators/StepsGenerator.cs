using Gherkin.Ast;
using ResponsibleGherkin.Utilities;

namespace ResponsibleGherkin.Generators;

public static class StepsGenerator
{
	public static IEnumerable<Line> Generate(StepsContainer stepsContainer)
	{
		var steps = stepsContainer.Steps.ToList();
		return steps.Select((s, i) => GenerateStep(s, i == (steps.Count - 1)));
	}

	private static Line GenerateStep(Step step, bool isLast) =>
		$"{step.MapKeywordToValid()}({step.Text.Quote()}, Pending){(isLast ? ");" : ",")}";
}
