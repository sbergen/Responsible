using NUnit.Framework;

namespace ResponsibleGherkin.Tests;

public class CodeGeneratorTests
{
	[Test]
	public void CodeGeneration_Fails_WithUnsupportedKeywords()
	{
		var document = TestFeatures.LoadFeature("UnsupportedKeyword");
		var exception = Assert.Throws<UnsupportedKeywordException>(() => CodeGenerator
			.GenerateFile(document.Feature, FlavorType.NUnit, TestFeatures.DefaultContext));

		// "*" is the unsupported keyword, assumed here, yeah I know, not great...
		StringAssert.Contains("*", exception!.Message);
		StringAssert.Contains("Unsupported", exception.Message);
		StringAssert.Contains("keyword", exception.Message);
	}
}
