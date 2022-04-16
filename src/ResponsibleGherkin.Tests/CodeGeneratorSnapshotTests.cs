using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using static VerifyXunit.Verifier;
using static ResponsibleGherkin.Tests.TestData;

namespace ResponsibleGherkin.Tests;

[UsesVerify]
public class CodeGeneratorSnapshotTests
{
	// Only verify the minimal version for now, to minimize the amount of snapshots
	// Differences between flavours are simple enough to not have to test anything more complex.
	[Theory]
	[InlineData(FlavorType.Unity)]
	[InlineData(FlavorType.NUnit)]
	[InlineData(FlavorType.Xunit)]
	public async Task VerifyMinimalFeatureFile(FlavorType flavor) =>
		await Verify(GenerateCode(MinimalFeature, flavor)).UseParameters(flavor);

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
