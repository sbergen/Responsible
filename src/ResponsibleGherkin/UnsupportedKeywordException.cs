namespace ResponsibleGherkin;

public class UnsupportedKeywordException : Exception
{
	public UnsupportedKeywordException(string context, string keyword)
		: base($"Unsupported {context} keyword: '{keyword}'")
	{
	}
}
