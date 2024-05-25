using FluentAssertions;
using Xunit;

namespace ResponsibleGherkin.Tests;

public class CommentParserTests
{
	[Fact]
	public void Parse_ReturnsEmptyConfiguration_WhenCommentsEmpty() =>
		ParseLines().Should().BeEquivalentTo(PartialConfiguration.Empty);

	[Theory]
	[InlineData("# responsible-flavor: foobar")]
	[InlineData("# responsible-indent: 4spaces")]
	[InlineData("# responsible-indent: 1 foo")]
	[InlineData("# responsible-indent: foo tabs")]
	[InlineData("# responsible-namespace:    ")]
	[InlineData("# responsible-base-class: \t ")]
	[InlineData("# responsible-executor:")]
	public void Parse_ThrowsError_OnInvalidValues(string comment)
	{
		var parse = () => ParseSingleLine(comment);
		parse.Should()
			.Throw<InvalidConfigurationException>()
			.WithMessage($"Invalid configuration comment *'{comment}'*(at 1:0)");
	}


	[Theory]
	[InlineData("# responsible-flavor: xunit", "# responsible-flavor: NUnit")]
	[InlineData("# responsible-base-class: C1", "# responsible-base-class: C1")]
	[InlineData("# responsible-executor: E1", "# responsible-executor: E2")]
	public void Parse_ThrowsError_WhenDuplicateValue(
		string comment1, string comment2)
	{
		var parse = () => ParseLines(comment1, comment2);
		parse.Should()
			.Throw<InvalidConfigurationException>()
			.WithMessage($"Duplicate configuration value *'{comment2}'*(at 2:0)");
	}

	[Theory]
	[InlineData("# responsible-flavor: Xunit", FlavorType.Xunit)]
	[InlineData("# responsible-flavor: nunit ", FlavorType.NUnit)]
	[InlineData("#  \t responsible-flavor: \t UNITY\t", FlavorType.Unity)]
	public void Parse_ParsesFlavor_WhenValid(string input, FlavorType expected) =>
		ParseSingleLine(input)
			.Flavor.Should().Be(expected);

	[Theory]
	[InlineData("# responsible-indent: 1 tab", 1, '\t')]
	[InlineData("# responsible-indent: 2 tabs", 2, '\t')]
	[InlineData("# responsible-indent: 1  space", 1, ' ')]
	[InlineData("# responsible-indent: 4\tspaces", 4, ' ')]
	public void Parse_ParsesIndentInfo_WhenValid(string input, int expectedAmount, char expectedChar) =>
		ParseSingleLine(input)
			.IndentInfo.Should().BeEquivalentTo(new IndentInfo(expectedAmount, expectedChar));

	[Theory]
	[InlineData("# responsible-namespace: My.Nested. Namespace")]
	[InlineData("# responsible-executor: GetContext().Executor")]
	[InlineData("# responsible-base-class: Some.Other.Namespace.BaseClass")]
	public void Parse_DoesNotThrow_WhenNamesAreValid(string input)
	{
		var parse = () => ParseSingleLine(input);
		parse.Should().NotThrow("the input is valid");
	}

	[Fact]
	public void FullConfiguration_CanBeBuiltFromComments()
	{
		var comments = new[]
		{
			"# responsible-indent: 4 spaces",
			"# responsible-namespace: MyNamespace",
			"# responsible-flavor: xunit",
			"# responsible-executor: Executor",
			"# responsible-base-class: MyTestBase",
		};

		var expected = new Configuration(
			FlavorType.Xunit,
			new IndentInfo(4, ' '),
			"MyNamespace",
			"MyTestBase",
			"Executor");

		var parsed = CommentParser.ParseLines(comments);

		Configuration.FromPartial(parsed).Should().BeEquivalentTo(expected);
	}

	private static PartialConfiguration ParseLines(params string[] lines) => CommentParser.ParseLines(lines);
	private static PartialConfiguration ParseSingleLine(string line) => CommentParser.ParseLines(new[] { line });
}
