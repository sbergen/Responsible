using System.Text;

namespace ResponsibleGherkin;

public record GeneratedClass(
	string ClassName,
	IReadOnlyList<string> FileLines)
{
	public string BuildFileContent()
	{
		var stringBuilder = new StringBuilder(
			this.FileLines.Sum(line => line.Length + Environment.NewLine.Length));

		foreach (var line in this.FileLines)
		{
			stringBuilder.AppendLine(line);
		}

		return stringBuilder.ToString();
	}

	public string ClassFileName() => $"{this.ClassName}.cs";
}
