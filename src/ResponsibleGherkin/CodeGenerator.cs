using Gherkin.Ast;

namespace ResponsibleGherkin;

public static class CodeGenerator
{
	public static GeneratedClass GenerateClass(
		Feature feature,
		Configuration configuration)
	{
		List<string> lines = new();
		var indentInfo = configuration.IndentInfo;
		var flavor = Flavor.FromType(configuration.Flavor);

		UsingDirectivesGenerator.Generate(flavor)
			.AppendToList(lines, indentInfo);

		lines.Add("");

		// TODO: support file-level namespaces?
		lines.Add($"namespace {configuration.Namespace}");
		lines.Add("{");

		FeatureGenerator.Generate(feature, flavor, configuration)
			.AppendToList(lines, indentInfo, indentBy: 1);

		lines.Add("}");

		return new GeneratedClass(feature.Name.ToPascalCase(), lines);
	}
}
