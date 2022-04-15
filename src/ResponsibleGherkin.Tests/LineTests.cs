using System.Text;
using NUnit.Framework;

namespace ResponsibleGherkin.Tests;

public class LineTests
{
	[TestCase(1, ExpectedResult = 2)]
	[TestCase(2, ExpectedResult = 3)]
	public int IndentBy_IncreasesIndent_WhenOriginalIndentIsOne(int amount) =>
		new Line("some content", 1).IndentBy(amount).Indent;

	[TestCase("",  ExpectedResult = "")]
	[TestCase(" \t ",  ExpectedResult = "")]
	[TestCase("foo\t",  ExpectedResult = "\tfoo")]
	[TestCase("\tfoo",  ExpectedResult = "\t\tfoo")]
	public string AppendToBuilder_IndentsCorrectly_WithTabs(string lineContent)
	{
		var builder = new StringBuilder();
		new Line(lineContent, 1).AppendToBuilder(builder, IndentInfo.Tabs);
		return builder.ToString().TrimEnd('\n', '\r');
	}

	[TestCase("",  ExpectedResult = "")]
	[TestCase(" \t ",  ExpectedResult = "")]
	[TestCase("foo\t",  ExpectedResult = "    foo")]
	[TestCase("\tfoo",  ExpectedResult = "    \tfoo")] // we don't touch existing tabs
	public string AppendToBuilder_IndentsCorrectly_WithSpaces(string lineContent)
	{
		var builder = new StringBuilder();
		new Line(lineContent, 1).AppendToBuilder(builder, IndentInfo.Spaces);
		return builder.ToString().TrimEnd('\n', '\r');
	}
}
