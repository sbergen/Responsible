using System.Threading.Tasks;
using Gherkin;
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
		"BasicFeature", CodeGenerator.FlavorType.Unity));

	private static string GenerateCode(
		string name,
		CodeGenerator.FlavorType flavor = CodeGenerator.FlavorType.NUnit,
		GenerationContext? context = null)
	{
		var document = new Parser { StopAtFirstError = true }
			.Parse(FeatureFileName(name));

		return CodeGenerator.GenerateFile(
			document.Feature,
			flavor,
			context ?? DefaultContext);
	}
}
