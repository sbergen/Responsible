namespace ResponsibleGherkin;

public record Configuration(
	FlavorType Flavor,
	IndentInfo IndentInfo,
	string Namespace,
	string BaseClass,
	string ExecutorName)
{
	public static Configuration FromPartial(PartialConfiguration config)
	{
		var missingProperties = typeof(PartialConfiguration)
			.GetProperties()
			.Where(p => p.GetValue(config) == null)
			.Select(p => p.Name)
			.ToList();

		if (missingProperties.Any())
		{
			throw InvalidConfigurationException.ForIncomplete(missingProperties);
		}

		return new Configuration(
			config.Flavor!.Value,
			config.IndentInfo!.Value,
			config.Namespace!,
			config.BaseClass!,
			config.ExecutorName!);
	}

	/// <summary>
	/// Merge partial configurations in decreasing priority order.
	/// </summary>
	public static Configuration Merge(params PartialConfiguration[] configs)
	{
		FlavorType? flavor = null;
		IndentInfo? indentInfo = null;
		string? @namespace = null;
		string? baseClass = null;
		string? executorName = null;

		foreach (var config in configs)
		{
			flavor ??= config.Flavor;
			indentInfo ??= config.IndentInfo;
			@namespace ??= config.Namespace;
			baseClass ??= config.BaseClass;
			executorName ??= config.ExecutorName;
		}

		return FromPartial(new PartialConfiguration(
			flavor,
			indentInfo,
			@namespace,
			baseClass,
			executorName));
	}
}
