using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using static VerifyXunit.Verifier;
using static ResponsibleGherkin.Tests.TestFeatures;

namespace ResponsibleGherkin.Tests;

[UsesVerify]
public class CodeGeneratorSnapshotTests
{
	[Fact]
	public Task VerifyBasicFeatureFile() => Verify(GenerateCode("BasicFeature"));

	[Fact]
	public Task VerifyBasicFeatureFile_WithUnity() => Verify(GenerateCode(
		"BasicFeature", FlavorType.Unity));

	private static string GenerateCode(
		string name,
		FlavorType flavor = FlavorType.NUnit,
		GenerationContext? context = null) =>
		CodeGenerator.GenerateFile(
			LoadFeature(name).Feature,
			flavor,
			context ?? DefaultContext);
}
