using Xunit;

namespace ResponsibleGherkin.Tests;

public class CodeGeneratorTests
{
	[Fact]
	public void CodeGeneration_Fails_WithUnsupportedKeywords()
	{
		var document = TestFeatures.LoadFeature("UnsupportedKeyword");
		var exception = Assert.Throws<UnsupportedKeywordException>(() => CodeGenerator
			.GenerateClass(document.Feature, TestFeatures.DefaultConfiguration));

		// I'm tightly coupling the test data to this assertion, yes.
		// Without some kind of test asset tagging, I'm not sure what would be cleaner.
		Assert.Contains("Unsupported step keyword: '*'", exception!.Message);
	}
}
