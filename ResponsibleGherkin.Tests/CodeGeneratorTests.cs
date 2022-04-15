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

		// I'm tightly coupling the test data to this assertion, yes.
		// Without some kind of test asset tagging, I'm not sure what would be cleaner.
		StringAssert.Contains("Unsupported step keyword: '*'", exception!.Message);
	}
}
