using System.Text;

namespace ResponsibleGherkin;

internal readonly record struct Line(string Content, int Indent = 0)
{
	public Line IndentBy(int amount) => new(this.Content, this.Indent + amount);

	public void AppendToBuilder(StringBuilder stringBuilder, GenerationContext context)
	{
		if (this.Content != "")
		{
			stringBuilder.Append(context.IndentChar, this.Indent * context.IndentAmount);
		}

		stringBuilder.AppendLine(this.Content);
	}

	public static implicit operator Line(string content) => new(content);
}
