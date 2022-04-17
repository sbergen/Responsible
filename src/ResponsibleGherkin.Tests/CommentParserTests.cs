using System;
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
		CommentParser.Parse(MakeComments())
			.Should().BeEquivalentTo(PartialConfiguration.Empty);

	[Theory]
	[InlineData("# rg-flavor: Xunit", FlavorType.Xunit)]
	[InlineData("# rg-flavor: nunit ", FlavorType.NUnit)]
	[InlineData("#  \t rg-flavor: \t UNITY\t", FlavorType.Unity)]
	public void Parse_ParsesFlavor_WhenValid(string input, FlavorType expected) =>
		CommentParser.Parse(MakeComments(input))
			.Flavor.Should().Be(expected);

	[Fact]
	public void Parse_ThrowsError_WhenInvalidFormat()
	{
		const string comment = "# rg-flavor: foobar";
		var parse = () => CommentParser.Parse(MakeComments(comment));
		parse.Should()
			.Throw<InvalidConfigurationException>()
			.WithMessage($"Invalid configuration comment *'{comment}'*(at 1:0)");
	}

	[Fact]
	public void Parse_ThrowsError_WhenDuplicateValue()
	{
		const string comment1 = "# rg-flavor: xunit";
		const string comment2 = "# rg-flavor: NUnit";
		var parse = () => CommentParser.Parse(MakeComments(comment1, comment2));
		parse.Should()
			.Throw<InvalidConfigurationException>()
			.WithMessage($"Duplicate configuration value *'{comment2}'*(at 2:0)");
	}

	private static IEnumerable<Comment> MakeComments(params string[] content) => content
		.Select((c, i) => new Comment(new Location(i + 1), c));
}
