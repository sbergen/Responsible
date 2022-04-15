using System.Text;
using Xunit;

namespace ResponsibleGherkin.Tests;

public class LineTests
{
	[Theory]
	[InlineData(1, 2)]
	[InlineData(2, 3)]
	public void IndentBy_IncreasesIndent_WhenOriginalIndentIsOne(int amount, int expected) =>
		Assert.Equal(
			expected,
			new Line("some content", 1).IndentBy(amount).Indent);

	[Theory]
	[InlineData("",  "")]
	[InlineData(" \t ", "")]
	[InlineData("foo\t", "\tfoo")]
	[InlineData("\tfoo", "\t\tfoo")]
	public void AppendToBuilder_IndentsCorrectly_WithTabs(string lineContent, string expected)
	{
		var builder = new StringBuilder();
		new Line(lineContent, 1).AppendToBuilder(builder, IndentInfo.Tabs);

		Assert.Equal(expected, builder.ToString().TrimEnd('\n', '\r'));
	}

	[Theory]
	[InlineData("", "")]
	[InlineData(" \t ", "")]
	[InlineData("foo\t", "    foo")]
	[InlineData("\tfoo", "    \tfoo")] // we don't touch existing tabs
	public void AppendToBuilder_IndentsCorrectly_WithSpaces(string lineContent, string expected)
	{
		var builder = new StringBuilder();
		new Line(lineContent, 1).AppendToBuilder(builder, IndentInfo.Spaces);
		Assert.Equal(expected, builder.ToString().TrimEnd('\n', '\r'));
	}
}
