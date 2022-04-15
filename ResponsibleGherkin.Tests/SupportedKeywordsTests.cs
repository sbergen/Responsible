using NUnit.Framework;

namespace ResponsibleGherkin.Tests;

public class SupportedKeywordsTests
{
	[TestCase("When", ExpectedResult = true)]
	[TestCase("Ultimately", ExpectedResult = false)]
	public bool IsSupportedKeyword_ReturnsExpectedValue(string str) =>
		str.IsSupportedKeyword();
}
