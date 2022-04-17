namespace ResponsibleGherkin;

public record PartialConfiguration(
	FlavorType? Flavor,
	IndentInfo? IndentInfo,
	string? Namespace,
	string? BaseClass,
	string? ExecutorName);
