namespace ResponsibleGherkin;

public static class SupportedKeywords
{
	private static readonly string[] Values =
	{
		"Given",
		"When",
		"Then",
		"And",
	};

	public static bool IsSupportedKeyword(this string str) =>
		Values.Contains(str);
}
