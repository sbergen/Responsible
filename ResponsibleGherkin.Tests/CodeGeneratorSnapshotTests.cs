using System.Threading.Tasks;
using NUnit.Framework;
using static VerifyNUnit.Verifier;
using static ResponsibleGherkin.Tests.TestFeatures;

namespace ResponsibleGherkin.Tests;

public class CodeGeneratorSnapshotTests
{
	[Test]
	public Task VerifyBasicFeatureFile() => Verify(GenerateCode("BasicFeature"));

	[Test]
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
