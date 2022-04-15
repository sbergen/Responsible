namespace ResponsibleGherkin.Tests;

internal static class TestFeatures
{
	public static readonly GenerationContext DefaultContext = new(
		"MyNamespace",
		"MyTestBase",
		IndentInfo.Tabs);

	public static string FeatureFileName(string featureName) =>
		$"TestFeatures/{featureName}.feature";
}
