using Xunit;

namespace ResponsibleGherkin.Tests;

public class SupportedKeywordsTests
{
	[Theory]
	[InlineData("When", true)]
	[InlineData("Ultimately", false)]
	public void IsSupportedKeyword_ReturnsExpectedValue(string str, bool expected) =>
		Assert.Equal(expected, str.IsSupportedKeyword());
}
