using Gherkin.Ast;
using ResponsibleGherkin.Utilities;

namespace ResponsibleGherkin;

public class UnsupportedKeywordException : Exception
{
	public UnsupportedKeywordException(string context, string keyword, Location location)
		: base($"Unsupported {context} keyword: '{keyword}' {location.Diagnostics()}")
	{
	}
}
