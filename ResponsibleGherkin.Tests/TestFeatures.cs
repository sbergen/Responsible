namespace ResponsibleGherkin.Tests;

internal static class TestFeatures
{
	public static readonly CodeGenerator.UserContext DefaultContext = new(
		"MyNamespace",
		"MyTestBase");

	public static string FeatureFileName(string featureName) =>
		$"TestFeatures/{featureName}.feature";
}
