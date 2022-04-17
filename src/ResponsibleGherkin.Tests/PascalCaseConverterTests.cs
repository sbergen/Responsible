using FluentAssertions;
using ResponsibleGherkin.Utilities;
using Xunit;

namespace ResponsibleGherkin.Tests;

public class PascalCaseConverterTests
{
	[Theory]
	[InlineData("", "")]
	[InlineData(" ", "")]
	[InlineData("  foo  ", "Foo")]
	[InlineData("foo   a  bar", "FooABar")]
	[InlineData("FooBar", "FooBar")]
	[InlineData("Foo 1 bar", "Foo1Bar")]
	[InlineData("1 bar", "_1Bar")]
	[InlineData("DSP scheduler", "DspScheduler")]
	[InlineData("!this-should-be-fine!", "ThisShouldBeFine")]
	public void ConvertToPascalCase_ReturnsExceptedValue(string input, string expected) =>
		PascalCaseConverter.ConvertToPascalCase(input).Should().Be(expected);
}
