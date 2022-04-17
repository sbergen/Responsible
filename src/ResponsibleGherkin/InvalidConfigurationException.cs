using Gherkin.Ast;
using ResponsibleGherkin.Utilities;

namespace ResponsibleGherkin;

public class InvalidConfigurationException : Exception
{
	private InvalidConfigurationException(string message)
		: base(message)
	{
	}

	public static InvalidConfigurationException ForInvalidComment(Comment comment) =>
		new($"Invalid configuration comment '{comment.Text}' {comment.Location.Diagnostics()}");

	public static InvalidConfigurationException ForDuplicateComment(Comment comment) =>
		new($"Duplicate configuration value provided in comment '{comment.Text}' {comment.Location.Diagnostics()}");

	public static InvalidConfigurationException ForIncomplete(IReadOnlyList<string> missingProperties)
	{
		var count = missingProperties.Count;
		var props = string.Join(", ", missingProperties);
		return new InvalidConfigurationException(
			$"Incomplete configuration, missing {count} properties: {props}");
	}
}
