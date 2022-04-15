using Microsoft.CodeAnalysis.CSharp;

namespace ResponsibleGherkin;

public static class StringExtensions
{
	public static string Quote(this string str) => SymbolDisplay.FormatLiteral(str, true);

	public static string ToPascalCase(this string str) => PascalCaseConverter.ConvertToPascalCase(str);
}
