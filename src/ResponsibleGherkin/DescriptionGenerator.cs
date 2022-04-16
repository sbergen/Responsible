using Gherkin.Ast;

namespace ResponsibleGherkin;

public static class DescriptionGenerator
{
	public static IEnumerable<Line> Generate(IHasDescription hasDescription)
	{
		if (!string.IsNullOrEmpty(hasDescription.Description))
		{
			using var sr = new StringReader(hasDescription.Description);
			string? line;
			while ((line = sr.ReadLine()) != null)
			{
				yield return $"// {line.TrimStart()}";
			}
		}
	}
}
