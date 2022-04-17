using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Gherkin.Ast;
using Xunit;

namespace ResponsibleGherkin.Tests;

public class CommentParserTests
{
	[Fact]
	public void Parse_ReturnsEmptyConfiguration_WhenCommentsEmpty() =>
		ParseLines().Should().BeEquivalentTo(PartialConfiguration.Empty);

	[Theory]
	[InlineData("# rg-flavor: foobar")]
	[InlineData("# rg-indent: 4spaces")]
	[InlineData("# rg-indent: 1 foo")]
	[InlineData("# rg-indent: foo tabs")]
	[InlineData("# rg-namespace: ...")]
	[InlineData("# rg-base-class: base class")]
	[InlineData("# rg-executor: 123")]
	public void Parse_ThrowsError_OnInvalidValues(string comment)
	{
		var parse = () => ParseSingleLine(comment);
		parse.Should()
			.Throw<InvalidConfigurationException>()
			.WithMessage($"Invalid configuration comment *'{comment}'*(at 1:0)");
	}


	[Fact]
	public void Parse_ThrowsError_WhenDuplicateValue()
	{
		const string comment1 = "# rg-flavor: xunit";
		const string comment2 = "# rg-flavor: NUnit";
		var parse = () => ParseLines(comment1, comment2);
		parse.Should()
			.Throw<InvalidConfigurationException>()
			.WithMessage($"Duplicate configuration value *'{comment2}'*(at 2:0)");
	}

	[Theory]
	[InlineData("# rg-flavor: Xunit", FlavorType.Xunit)]
	[InlineData("# rg-flavor: nunit ", FlavorType.NUnit)]
	[InlineData("#  \t rg-flavor: \t UNITY\t", FlavorType.Unity)]
	public void Parse_ParsesFlavor_WhenValid(string input, FlavorType expected) =>
		ParseSingleLine(input)
			.Flavor.Should().Be(expected);

	[Theory]
	[InlineData("# rg-indent: 1 tab", 1, IndentType.Tabs)]
	[InlineData("# rg-indent: 2 tabs", 2, IndentType.Tabs)]
	[InlineData("# rg-indent: 1  space", 1, IndentType.Spaces)]
	[InlineData("# rg-indent: 4\tspaces", 4, IndentType.Spaces)]
	public void Parse_ParsesIndentInfo_WhenValid(string input, int expectedAmount, IndentType expectedTye) =>
		ParseSingleLine(input)
			.IndentInfo.Should().BeEquivalentTo(new IndentInfo(expectedAmount, expectedTye));

	[Fact]
	public void FullConfiguration_CanBeBuiltFromComments()
	{
		var comments = new[]
		{
			"# rg-indent: 4 spaces",
			"# rg-namespace: MyNamespace",
			"# rg-flavor: xunit",
			"# rg-executor: Executor",
			"# rg-base-class: MyTestBase",
		};

		var expected = new Configuration(
			FlavorType.Xunit,
			new IndentInfo(4, IndentType.Spaces),
			"MyNamespace",
			"MyTestBase",
			"Executor");

		var parsed = CommentParser.ParseLines(comments);

		Configuration.FromPartial(parsed).Should().BeEquivalentTo(expected);
	}

	private static PartialConfiguration ParseLines(params string[] lines) => CommentParser.ParseLines(lines);
	private static PartialConfiguration ParseSingleLine(string line) => CommentParser.ParseLines(new[] { line });
}
