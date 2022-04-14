namespace ResponsibleGherkin;

internal readonly record struct Line(string Content, int Indent = 0)
{
	public Line IndentBy(int amount) => new(this.Content, this.Indent + amount);

	public static implicit operator Line(string content) => new(content);
}
