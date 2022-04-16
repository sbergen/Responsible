using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using static VerifyXunit.Verifier;
using static ResponsibleGherkin.Tests.TestData;

namespace ResponsibleGherkin.Tests;

[UsesVerify]
public class CodeGeneratorSnapshotTests
{
	[Theory]
	[InlineData(FlavorType.Unity)]
	[InlineData(FlavorType.NUnit)]
	[InlineData(FlavorType.Xunit)]
	public async Task VerifyBasicFeatureFile(FlavorType flavor) =>
		await Verify(GenerateCode(BasicFeature, flavor)).UseParameters(flavor);

	private static string GenerateCode(string name, FlavorType flavor) =>
		CodeGenerator
			.GenerateClass(
				LoadFeature(name).Feature,
				DefaultConfiguration with { Flavor = flavor })
			.BuildFileContent();
}
