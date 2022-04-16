using System.Text;
using System.Text.RegularExpressions;

namespace ResponsibleGherkin.Utilities;

public static class PascalCaseConverter
{
	private static readonly char[] SplitChars = { ' ', '\n', '\t', '\n' };

	// I couldn't find a way to kill this mutant, as it's essentially just an optimization
	// Stryker disable once Bitwise
	public static string ConvertToPascalCase(string str) => string.Join("", str
		.Split(SplitChars, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
		.Select(WordToPascalCse));


	private static string WordToPascalCse(string word)
	{
		// Copied from https://stackoverflow.com/a/23346095

		// Find word parts using the following rules:
		// 1. all lowercase starting at the beginning is a word
		// 2. all caps is a word.
		// 3. first letter caps, followed by all lowercase is a word
		// 4. the entire string must decompose into words according to 1,2,3.
		// Note that 2&3 together ensure MPSUser is parsed as "MPS" + "User".

		var match = Regex.Match(word, "^(?<word>^[a-z]+|[A-Z]+|[A-Z][a-z]+)+$");
		var matchGroup = match.Groups["word"];

		// Take each word and convert individually to TitleCase
		// to generate the final output.  Note the use of ToLower
		// before ToTitleCase because all caps is treated as an abbreviation.
		var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
		var result = new StringBuilder();
		foreach (var capture in matchGroup.Captures.Cast<Capture>())
		{
			result.Append(textInfo.ToTitleCase(capture.Value.ToLower()));
		}

		return result.ToString();
	}
}
