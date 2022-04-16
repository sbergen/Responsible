using FluentAssertions;
using Xunit;

namespace ResponsibleGherkin.Tests;

public class SupportedKeywordsTests
{
	[Theory]
	[InlineData("When", true)]
	[InlineData("Ultimately", false)]
	public void IsSupportedKeyword_ReturnsExpectedValue(string str, bool expected) =>
		str.IsSupportedKeyword().Should().Be(expected);
}
