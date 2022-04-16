using FluentAssertions;
using Xunit;

namespace ResponsibleGherkin.Tests;

public class PascalCaseConverterTests
{
	[Theory]
	[InlineData("", "")]
	[InlineData(" ", "")]
	[InlineData(" foo ", "Foo")]
	[InlineData(" lots\nof\tfood ", "LotsOfFood")]
	[InlineData(" lots     of      food ", "LotsOfFood")]
	[InlineData("DSP scheduler", "DspScheduler")]
	[InlineData("DspScheduler can do", "DspSchedulerCanDo")]
	public void ConvertToPascalCase_ReturnsExceptedValue(string input, string expected) =>
		PascalCaseConverter.ConvertToPascalCase(input).Should().Be(expected);
}
