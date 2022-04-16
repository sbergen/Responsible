using System.IO;
using System.Text.Json;
using Gherkin;
using Gherkin.Ast;

namespace ResponsibleGherkin.Tests;

internal static class TestFeatures
{
	public static readonly Configuration DefaultConfiguration = new(
		FlavorType.Xunit,
		IndentInfo.Tabs,
		"MyNamespace",
		"MyTestBase",
		"Executor");

	public static readonly string DefaultConfigurationJson = JsonSerializer.Serialize(DefaultConfiguration);

  	public static readonly string MinimalFeatureString = File.ReadAllText(FeatureFileName("MinimalFeature"));

    public static readonly string UnsupportedKeywordFeatureString =
	    File.ReadAllText(FeatureFileName("UnsupportedKeyword"));

	public static GherkinDocument LoadFeature(string featureName) =>
		new Parser().Parse(FeatureFileName(featureName));

	private static string FeatureFileName(string featureName) =>
		$"TestFeatures/{featureName}.feature";
}
