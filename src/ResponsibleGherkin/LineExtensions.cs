using System.Text;

namespace ResponsibleGherkin;

public static class LineExtensions
{
	public static void AppendToBuilder(
		this IEnumerable<Line> lines,
		StringBuilder stringBuilder,
		IndentInfo indentInfo,
		int indentBy = 0)
	{
		foreach (var line in lines)
		{
			line.IndentBy(indentBy).AppendToBuilder(stringBuilder, indentInfo);
		}
	}
}
