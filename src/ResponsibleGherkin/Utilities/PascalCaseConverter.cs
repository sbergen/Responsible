using System.Text;
using static ResponsibleGherkin.Utilities.PascalCaseConverter.CharType;

namespace ResponsibleGherkin.Utilities;

public static class PascalCaseConverter
{
	internal enum CharType
	{
		NonAlphaNumeric = 1,
		Digit,
		Lowercase,
		Uppercase,
	}

	/// <summary>
	/// Strips every non-alphanumeric character, and converts to pascal case.
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public static string ConvertToPascalCase(string str)
	{
		// Assume length is about the same, prepare for leading underscore.
		// Disable Stryker, as this is just optimization.
		// Stryker disable all once
		var builder = new StringBuilder(str.Length + 1);

		// previous is "before the start of the string" here
		var previousType = NonAlphaNumeric;
		foreach (var ch in str)
		{
			var currentType = ResolveType(ch);

			switch (currentType)
			{
				case Lowercase when previousType is NonAlphaNumeric or Digit:
					builder.Append(char.ToUpper(ch));
					break;
				case Uppercase when previousType is Uppercase:
					builder.Append(char.ToLower(ch));
					break;
				case Lowercase or Uppercase:
					builder.Append(ch);
					break;
				case Digit:
				{
					if (builder.Length == 0)
					{
						builder.Append('_');
					}

					builder.Append(ch);
					break;
				}
			}

			previousType = currentType;
		}

		return builder.ToString();
	}

	private static CharType ResolveType(char ch)
	{
		if (char.IsLetter(ch))
		{
			return char.IsUpper(ch) ? Uppercase : Lowercase;
		}
		else if (char.IsNumber(ch))
		{
			return Digit;
		}
		else
		{
			return NonAlphaNumeric;
		}
	}
}
