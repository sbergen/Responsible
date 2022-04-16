using Gherkin.Ast;

namespace ResponsibleGherkin;

public class UnsupportedKeywordException : Exception
{
	public UnsupportedKeywordException(string context, string keyword, Location location)
		: base($"Unsupported {context} keyword: '{keyword}' (at {location.Line}:{location.Column})")
	{
	}
}
