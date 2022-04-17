namespace ResponsibleGherkin;

public class IncompleteConfigurationException : Exception
{
	public IncompleteConfigurationException(IReadOnlyList<string> missingProperties)
		: base(BuildMessage(missingProperties))
	{
	}

	private static string BuildMessage(IReadOnlyList<string> missingProperties)
	{
		var count = missingProperties.Count;
		var props = string.Join(", ", missingProperties);
		return $"Incomplete configuration, missing {count} properties: {props}";
	}
}
