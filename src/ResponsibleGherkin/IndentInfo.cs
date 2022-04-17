namespace ResponsibleGherkin;

public readonly record struct IndentInfo(int Amount, char Character)
{
	public static readonly IndentInfo Tabs = new(1, '\t');
	public static readonly IndentInfo Spaces = new(4, ' ');
}
