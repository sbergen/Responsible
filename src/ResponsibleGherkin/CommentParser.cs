using Gherkin.Ast;

namespace ResponsibleGherkin;

public static class CommentParser
{
	public static PartialConfiguration Parse(IEnumerable<Comment> comments)
	{
		FlavorType? flavor = null;
		IndentInfo? indentInfo = null;
		string? @namespace = null;
		string? baseClass = null;
		string? executorName = null;

		bool ParseFlavor(Comment comment, string content) =>
			TryParse("rg-flavor:", comment, content, ref flavor, TryParseEnumNullable<FlavorType>);

		var parsers = new Func<Comment, string, bool>[]
		{
			ParseFlavor,
		};

		foreach (var (comment, content) in comments
			.Select(comment => (comment, ExtractContent(comment))))
		{
			var alreadyParsed = false;
			foreach (var parser in parsers)
			{
				alreadyParsed = alreadyParsed || parser(comment, content);
			}
		}

		return new PartialConfiguration(
			flavor,
			indentInfo,
			@namespace,
			baseClass,
			executorName);
	}

	private static bool TryParse<T>(
		string identifier,
		Comment comment,
		string content,
		ref T? value,
		Func<string, T?> parser)
	{
		var valueStr = content.GetValueFor(identifier);
		if (valueStr != null)
		{
			value = (value, parser(valueStr)) switch
			{
				(_, null) => throw InvalidConfigurationException.ForInvalidComment(comment),
				({ }, { }) => throw InvalidConfigurationException.ForDuplicateComment(comment),
				(null, { } newValue) => newValue,
			};

			return true;
		}
		else
		{
			return false;
		}
	}

	private static T? TryParseEnumNullable<T>(string input)
		where T : struct, Enum =>
		Enum.TryParse<T>(input, ignoreCase: true, out var value) ? value : null;

	private static string ExtractContent(Comment comment) => comment.Text.Trim('#', ' ', '\t');

	private static string? GetValueFor(this string input, string identifier) =>
		input.StartsWith(identifier)
			? input[Math.Min(identifier.Length + 1, input.Length)..]
			: null;
}
