using FluentAssertions;
using Xunit;

namespace ResponsibleGherkin.Tests;

public class CodeGeneratorTests
{
	[Fact]
	public void CodeGeneration_Fails_WithUnsupportedKeywords()
	{
		var document = TestData.LoadFeature(TestData.UnsupportedKeyword);

		var codeGeneration = () => CodeGenerator.GenerateClass(
			document.Feature,
			TestData.DefaultConfiguration);

		// I'm tightly coupling the test data to this assertion, yes.
		// Without some kind of test asset tagging, I'm not sure what would be cleaner.
		codeGeneration.Should()
			.Throw<UnsupportedKeywordException>()
			.And.Message.Should().ContainAll("Unsupported step keyword", "*", "(at 4:5)");
	}
}
