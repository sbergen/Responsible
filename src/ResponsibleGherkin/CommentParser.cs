using System.Globalization;
using Gherkin.Ast;

namespace ResponsibleGherkin;

public static class CommentParser
{
	private static readonly char[] IntraLineSeparators = { ' ', '\t' };
	private static readonly char[] LineTrimChars = { '#', ' ', '\t' };

	public static PartialConfiguration ParseLines(IEnumerable<string> lines) =>
		Parse(lines.Select((line, i) =>
			new Comment(new Location(i + 1), line)));

	public static PartialConfiguration Parse(IEnumerable<Comment> comments)
	{
		FlavorType? flavor = null;
		IndentInfo? indentInfo = null;
		string? @namespace = null;
		string? baseClass = null;
		string? executorName = null;

		void ParseFlavor(Comment comment, string content) =>
			TryParse("responsible-flavor:", comment, content, ref flavor, TryParseEnumNullable<FlavorType>);

		void ParseIndentInfo(Comment comment, string content) =>
			TryParse("responsible-indent:", comment, content, ref indentInfo, TryParseIndentInfo);

		void ParseNamespace(Comment comment, string content) =>
			TryParse("responsible-namespace:", comment, content, ref @namespace, TryParseIdentifierOrExpression);

		void ParseBaseClass(Comment comment, string content) =>
			TryParse("responsible-base-class:", comment, content, ref baseClass, TryParseIdentifierOrExpression);

		void ParseExecutorName(Comment comment, string content) =>
			TryParse("responsible-executor:", comment, content, ref executorName, TryParseIdentifierOrExpression);

		var parsers = new Action<Comment, string>[]
		{
			ParseFlavor,
			ParseIndentInfo,
			ParseNamespace,
			ParseBaseClass,
			ParseExecutorName,
		};

		foreach (var (comment, content) in comments
			.Select(comment => (comment, ExtractContent(comment))))
		{
			foreach (var parser in parsers)
			{
				parser(comment, content);
			}
		}

		return new PartialConfiguration(
			flavor,
			indentInfo,
			@namespace,
			baseClass,
			executorName);
	}

	// This could be optimized by returning a boolean of whether or not something was parsed.
	// However, Stryker is telling me that if I'm not explicitly testing it,
	// the optimization is probably not worth it now :)
	private static void TryParse<T>(
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
		}
	}

	private static T? TryParseEnumNullable<T>(string input)
		where T : struct, Enum =>
		Enum.TryParse<T>(input, ignoreCase: true, out var value) ? value : null;

	private static IndentInfo? TryParseIndentInfo(string input)
	{
		static IndentInfo? ParseValues(string amountStr, string typeStr) =>
			(amountStr.TryParseIntNullable(), typeStr.TryParseIndentChar()) switch
			{
				({} amount, {} type) => new IndentInfo(amount, type),
				_ => null,
			};

		var parts = input.Split(
			IntraLineSeparators,
			StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		return parts.Length == 2 ? ParseValues(parts[0], parts[1]) : null;
	}

	private static char? TryParseIndentChar(this string input) => input switch
	{
		"tab" => '\t',
		"tabs" => '\t',
		"space" => ' ',
		"spaces" => ' ',
		_ => null,
	};

	private static int? TryParseIntNullable(this string input) =>
		int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
			? result
			: null;

	// Let's not try to be too smart. The compiler will reveal if it's wrong sooner or later.
	// Just checking for empty is good enough for now.
	private static string? TryParseIdentifierOrExpression(this string input) =>
		string.IsNullOrEmpty(input) ? null : input;

	private static string ExtractContent(Comment comment) => comment.Text.Trim(LineTrimChars);

	private static string? GetValueFor(this string input, string identifier) =>
		input.StartsWith(identifier)
			? input[Math.Min(identifier.Length + 1, input.Length)..]
			: null;
}
