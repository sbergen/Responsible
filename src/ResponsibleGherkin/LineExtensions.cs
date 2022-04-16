namespace ResponsibleGherkin;

public static class LineExtensions
{
	public static void AppendToList(
		this IEnumerable<Line> lines,
		IList<string> resultList,
		IndentInfo indentInfo,
		int indentBy = 0)
	{
		foreach (var line in lines)
		{
			resultList.Add(line.IndentBy(indentBy).BuildString(indentInfo));
		}
	}
}
