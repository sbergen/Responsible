using System.Text;

namespace ResponsibleGherkin;

public record GeneratedClass(IReadOnlyList<string> FileLines)
{
	public string BuildFileContent()
	{
		// String builder initial allocation is just optimization,
		// No tests will fail if this is modified:
		// Stryker disable once all
		var stringBuilder = new StringBuilder(
			this.FileLines.Sum(line => line.Length + Environment.NewLine.Length));

		foreach (var line in this.FileLines)
		{
			stringBuilder.AppendLine(line);
		}

		return stringBuilder.ToString();
	}
}
