using System.Text;
using Gherkin.Ast;

namespace ResponsibleGherkin;

public static class CodeGenerator
{
	public static string GenerateFile(
		Feature feature,
		FlavorType flavorType,
		GenerationContext context)
	{
		var builder = new StringBuilder();
		var flavor = Flavor.FromType(flavorType);
		var indentInfo = context.IndentInfo;

		UsingDirectivesGenerator.Generate(flavor)
			.AppendToBuilder(builder, indentInfo);

		builder.AppendLine();

		// TODO: support file-level namespaces?
		builder.AppendLine($"namespace {context.Namespace}");
		builder.AppendLine("{");

		FeatureGenerator.Generate(feature, flavor, context)
			.AppendToBuilder(builder, indentInfo, indentBy: 1);

		builder.AppendLine("}");

		return builder.ToString();
	}
}
