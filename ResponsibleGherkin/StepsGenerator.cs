using Gherkin.Ast;

namespace ResponsibleGherkin;

public static class StepsGenerator
{
	public static IEnumerable<Line> Generate(StepsContainer stepsContainer)
	{
		var steps = stepsContainer.Steps.ToList();
		return steps.Select((s, i) => GenerateStep(s, i == (steps.Count - 1)));
	}

	private static Line GenerateStep(Step step, bool isLast)
	{
		var keyword = step.Keyword.TrimEnd();

		return keyword.IsSupportedKeyword()
			? $"{keyword}({step.Text.Quote()}, Pending){(isLast ? ");" : ",")}"
			: throw new Exception($"Unknown step keyword {keyword}");
	}
}
