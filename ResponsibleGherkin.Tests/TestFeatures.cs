using Gherkin;
using Gherkin.Ast;

namespace ResponsibleGherkin.Tests;

internal static class TestFeatures
{
	public static readonly GenerationContext DefaultContext = new(
		"MyNamespace",
		"MyTestBase",
		IndentInfo.Tabs);

	public static GherkinDocument LoadFeature(string featureName) =>
		new Parser { StopAtFirstError = true }
			.Parse(FeatureFileName(featureName));

	private static string FeatureFileName(string featureName) =>
		$"TestFeatures/{featureName}.feature";
}
