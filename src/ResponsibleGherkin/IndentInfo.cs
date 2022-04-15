namespace ResponsibleGherkin;

public readonly record struct IndentInfo(int Amount, IndentType Type)
{
	public static readonly IndentInfo Tabs = new IndentInfo(1, IndentType.Tab);
	public static readonly IndentInfo Spaces = new IndentInfo(4, IndentType.Space);

	public char Character => this.Type == IndentType.Tab ? '\t' : ' ';
}
