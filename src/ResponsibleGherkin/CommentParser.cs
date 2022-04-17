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

		foreach (var (comment, content) in comments
			.Select(comment => (comment, ExtractContent(comment))))
		{
			TryParseFlavor(comment, content, ref flavor);
		}

		return new PartialConfiguration(
			flavor,
			indentInfo,
			@namespace,
			baseClass,
			executorName);
	}

	private static bool TryParseFlavor(Comment comment, string content, ref FlavorType? flavor)
	{
		var valueStr = content.GetValueFor("rg-flavor:");
		if (valueStr != null)
		{
			if (Enum.TryParse<FlavorType>(valueStr, ignoreCase: true, out var value))
			{
				if (flavor.HasValue)
				{
					throw InvalidConfigurationException.ForDuplicateComment(comment);
				}
				else
				{
					flavor = value;
				}
			}
			else
			{
				throw InvalidConfigurationException.ForInvalidComment(comment);
			}

			return true;
		}
		else
		{
			return false;
		}
	}

	private static string ExtractContent(Comment comment) => comment.Text.Trim('#', ' ', '\t');

	private static string? GetValueFor(this string input, string identifier) =>
		input.StartsWith(identifier)
			? input[Math.Min(identifier.Length + 1, input.Length)..]
			: null;
}
