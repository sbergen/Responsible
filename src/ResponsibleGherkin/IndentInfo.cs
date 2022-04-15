using System.Text.Json.Serialization;

namespace ResponsibleGherkin;

public readonly record struct IndentInfo(int Amount, IndentType Type)
{
	public static readonly IndentInfo Tabs = new IndentInfo(1, IndentType.Tab);
	public static readonly IndentInfo Spaces = new IndentInfo(4, IndentType.Space);

	[JsonIgnore]
	public char Character => this.Type == IndentType.Tab ? '\t' : ' ';
}
