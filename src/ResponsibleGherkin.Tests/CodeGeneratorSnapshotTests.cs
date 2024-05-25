using System.Threading.Tasks;
using ResponsibleGherkin.Generators;
using Xunit;
using static VerifyXunit.Verifier;
using static ResponsibleGherkin.Tests.TestData;

namespace ResponsibleGherkin.Tests;

public class CodeGeneratorSnapshotTests
{
	[Theory]
	[ClassData(typeof(EnumValues<FlavorType>))]
	public async Task VerifyMinimalFeatureFile(FlavorType flavor) =>
		await Verify(GenerateCode(MinimalFeature, flavor)).UseParameters(flavor);

	[Theory]
	[ClassData(typeof(EnumValues<FlavorType>))]
	public async Task VerifyBackgroundFeatureFile(FlavorType flavor) =>
		await Verify(GenerateCode(BackgroundFeature, flavor)).UseParameters(flavor);

	[Theory]
	[InlineData(BasicFeature)]
	[InlineData(Rules)]
	public async Task VerifyXunitFeatureFile(string feature) =>
		await Verify(GenerateCode(feature, FlavorType.Xunit)).UseParameters(feature);

	private static string GenerateCode(string name, FlavorType flavor) =>
		CodeGenerator
			.GenerateClass(
				LoadFeature(name).Feature,
				DefaultConfiguration with { Flavor = flavor })
			.BuildFileContent();
}
