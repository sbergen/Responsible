namespace ResponsibleGherkin;

public record struct Configuration(
	FlavorType Flavor,
	IndentInfo IndentInfo,
	string Namespace,
	string BaseClass,
	string ExecutorName);
