namespace ResponsibleGherkin;

public readonly record struct IndentInfo(int Amount, char Character)
{
	public static readonly IndentInfo Tabs = new IndentInfo(1, '\t');
	public static readonly IndentInfo Spaces = new IndentInfo(4, ' ');
}
