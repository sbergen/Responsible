using NUnit.Framework;

namespace ResponsibleGherkin.Tests;

public class PascalCaseConverterTests
{
	[TestCase("", ExpectedResult = "")]
	[TestCase(" ", ExpectedResult = "")]
	[TestCase(" foo ", ExpectedResult = "Foo")]
	[TestCase(" lots of food ", ExpectedResult = "LotsOfFood")]
	[TestCase("DSP scheduler", ExpectedResult = "DspScheduler")]
	[TestCase("DspScheduler can do", ExpectedResult = "DspSchedulerCanDo")]
	[TestCase("UI handler", ExpectedResult = "UIHandler", Ignore = "two-letter acronyms not handled")]
	public string ConvertToPascalCase_ReturnsExceptedValue(string input)
		=> PascalCaseConverter.ConvertToPascalCase(input);
}
