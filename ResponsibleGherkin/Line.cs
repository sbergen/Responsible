using System.Text;

namespace ResponsibleGherkin;

public readonly record struct Line(string Content, int Indent = 0)
{
	public Line IndentBy(int amount) => this with { Indent = this.Indent + amount };

	public void AppendToBuilder(StringBuilder stringBuilder, IndentInfo indentInfo)
	{
		var endTrimmed = this.Content.TrimEnd();
		if (endTrimmed != "")
		{
			stringBuilder.Append(indentInfo.Character, this.Indent * indentInfo.Amount);
		}

		stringBuilder.AppendLine(endTrimmed);
	}

	public static implicit operator Line(string content) => new(content);
}
