namespace ResponsibleGherkin;

public record PartialConfiguration(
	FlavorType? Flavor = default,
	IndentInfo? IndentInfo = default,
	string? Namespace = default,
	string? BaseClass = default,
	string? ExecutorName = default)
{
	public static readonly PartialConfiguration Empty = new();
}
