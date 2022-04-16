using FluentAssertions;
using ResponsibleGherkin.Utilities;
using Xunit;

namespace ResponsibleGherkin.Tests;

public class LineTests
{
	[Theory]
	[InlineData(1, 2)]
	[InlineData(2, 3)]
	public void IndentBy_IncreasesIndent_WhenOriginalIndentIsOne(int amount, int expected) =>
		new Line("some content", 1).IndentBy(amount).Indent
			.Should().Be(expected);

	[Theory]
	[InlineData("", "")]
	[InlineData(" \t ", "")]
	[InlineData("foo\t", "\tfoo")]
	[InlineData("\tfoo", "\t\tfoo")]
	public void BuildString_IndentsCorrectly_WithTabs(string lineContent, string expected) =>
		new Line(lineContent, 1).BuildString(IndentInfo.Tabs).Should().Be(expected);

	[Theory]
	[InlineData("", "")]
	[InlineData(" \t ", "")]
	[InlineData("foo\t", "    foo")]
	[InlineData("\tfoo", "    \tfoo")] // we don't touch existing tabs
	public void BuildString_IndentsCorrectly_WithSpaces(string lineContent, string expected) =>
		new Line(lineContent, 1).BuildString(IndentInfo.Spaces).Should().Be(expected);
}
