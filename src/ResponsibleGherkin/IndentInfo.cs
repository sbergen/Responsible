using System.Text.Json.Serialization;

namespace ResponsibleGherkin;

public readonly record struct IndentInfo(int Amount, IndentType Type)
{
	public static readonly IndentInfo Tabs = new(1, IndentType.Tabs);
	public static readonly IndentInfo Spaces = new(4, IndentType.Spaces);

	[JsonIgnore]
	public char Character => this.Type == IndentType.Tabs ? '\t' : ' ';
}
