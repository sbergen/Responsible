namespace ResponsibleGherkin;

public static class MultipleItemGenerator
{
	/// <summary>
	/// Generates multiple items with an empty line in between, but not after the last.
	/// </summary>
	public static IEnumerable<Line> GenerateMultiple<TData>(
		IReadOnlyList<TData> data,
		Func<TData, IEnumerable<Line>> linesGenerator)
	{
		foreach (var (datum, isLast) in data
			.Select((d, i) => (d, i == data.Count - 1)))
		{
			foreach (var scenarioLine in linesGenerator(datum))
			{
				yield return scenarioLine;
			}

			if (!isLast)
			{
				yield return "";
			}
		}
	}
}
