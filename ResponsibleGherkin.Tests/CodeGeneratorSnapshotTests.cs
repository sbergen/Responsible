using System.Threading.Tasks;
using Gherkin;
using NUnit.Framework;
using VerifyNUnit;
using static VerifyNUnit.Verifier;

namespace ResponsibleGherkin.Tests;

public class CodeGeneratorSnapshotTests
{
	private static readonly CodeGenerator.UserContext DefaultContext = new(
		"MyNamespace",
		"MyTestBase");

	[Test]
	public Task VerifyBasicFeatureFile() => Verify(GenerateCode("BasicFeature"));

	private static string GenerateCode(
		string name,
		CodeGenerator.FlavorType flavor = CodeGenerator.FlavorType.NUnit,
		CodeGenerator.UserContext? context = null)
	{
		var document = new Parser
		{
			StopAtFirstError = true,
		}.Parse($"TestFeatures/{name}.feature");

		return CodeGenerator.GenerateFile(
			document.Feature,
			flavor,
			context ?? DefaultContext);
	}
}
