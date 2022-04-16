using System.IO;
using System.Text.Json;
using Gherkin;
using Gherkin.Ast;

namespace ResponsibleGherkin.Tests;

internal static class TestData
{
	public static readonly Configuration DefaultConfiguration = new(
		FlavorType.Xunit,
		IndentInfo.Tabs,
		"MyNamespace",
		"MyTestBase",
		"Executor");

	public static readonly string DefaultConfigurationJson = JsonSerializer.Serialize(DefaultConfiguration);

	public const string MinimalFeature = "MinimalFeature";
	public const string BackgroundFeature = "BackgroundFeature";
	public const string BasicFeature = "BasicFeature";
	public const string UnsupportedKeyword = "UnsupportedKeyword";
	public const string Rules = "Rules";

	public static readonly string MinimalFeatureContent = File.ReadAllText(FeatureFileName(MinimalFeature));

    public static readonly string UnsupportedKeywordFeatureContent =
	    File.ReadAllText(FeatureFileName(UnsupportedKeyword));

	public static GherkinDocument LoadFeature(string featureName) =>
		new Parser().Parse(FeatureFileName(featureName));

	private static string FeatureFileName(string featureName) =>
		$"TestFeatures/{featureName}.feature";
}
