using ResponsibleGherkin.Utilities;

namespace ResponsibleGherkin.Generators;

public static class UsingDirectivesGenerator
{
	private static readonly string[] StandardNamespaces =
	{
		"Responsible",
	};

	public static IEnumerable<Line> Generate(Flavor flavor) => flavor
		.RequiredNamespaces
		.Concat(StandardNamespaces)
		.OrderBy(ns => ns)
		.Select(ns => $"using {ns};")
		.Append("using static Responsible.Bdd.Keywords;")
		.Select(str => (Line)str);
}
