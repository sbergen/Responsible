namespace ResponsibleGherkin;

public record PartialConfiguration(
	FlavorType? Flavor,
	IndentInfo? IndentInfo,
	string? Namespace,
	string? BaseClass,
	string? ExecutorName)
{
	public static readonly PartialConfiguration Empty =
		new(default, default, default, default, default);
}
