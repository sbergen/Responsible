using System.Text;

namespace ResponsibleGherkin.Utilities;

public readonly record struct Line(string Content, int Indent = 0)
{
	public Line IndentBy(int amount) => amount != 0
		? this with { Indent = this.Indent + amount }
		: this;

	public string BuildString(IndentInfo indentInfo)
	{
		StringBuilder stringBuilder = new();
		var endTrimmed = this.Content.TrimEnd();
		if (endTrimmed != "")
		{
			stringBuilder.Append(indentInfo.Character, this.Indent * indentInfo.Amount);
		}

		stringBuilder.Append(endTrimmed);
		return stringBuilder.ToString();
	}

	public static implicit operator Line(string content) => new(content);
}
