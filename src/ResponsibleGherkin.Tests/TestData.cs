using System.IO;
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

	public const string DefaultConfigurationAsComments = @"
# rg-indent: 1 tab
# rg-namespace: MyNamespace
# rg-flavor: xunit
# rg-executor: Executor
# rg-base-class: MyTestBase";

	public const string MinimalFeature = "MinimalFeature";
	public const string BackgroundFeature = "BackgroundFeature";
	public const string BasicFeature = "BasicFeature";
	public const string UnsupportedKeyword = "UnsupportedKeyword";
	public const string Rules = "Rules";
	public const string PartialConfigFeature = "FeatureWithPartialConfig";

	public static readonly string MinimalFeatureContent = File.ReadAllText(FeatureFileName(MinimalFeature));
	public static readonly string PartialConfigFeatureContent = File.ReadAllText(FeatureFileName(PartialConfigFeature));

    public static readonly string UnsupportedKeywordFeatureContent =
	    File.ReadAllText(FeatureFileName(UnsupportedKeyword));

	public static GherkinDocument LoadFeature(string featureName) =>
		new Parser().Parse(FeatureFileName(featureName));

	private static string FeatureFileName(string featureName) =>
		$"TestFeatures/{featureName}.feature";
}
