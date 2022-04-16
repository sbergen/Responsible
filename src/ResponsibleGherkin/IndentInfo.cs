using System.Text.Json.Serialization;

namespace ResponsibleGherkin;

public readonly record struct IndentInfo(int Amount, IndentType Type)
{
	public static readonly IndentInfo Tabs = new IndentInfo(1, IndentType.Tabs);
	public static readonly IndentInfo Spaces = new IndentInfo(4, IndentType.Spaces);

	[JsonIgnore]
	public char Character => this.Type == IndentType.Tabs ? '\t' : ' ';
}
