using FluentAssertions;
using Gherkin.Ast;
using ResponsibleGherkin.Utilities;
using Xunit;

namespace ResponsibleGherkin.Tests;

public class SupportedKeywordsTests
{
	[Theory]
	[InlineData("When", "When")]
	[InlineData("*", "And")]
	public void MapStepKeywordToValid_ReturnsExpectedValue_WhenKeywordIsValid(string str, string? expected) =>
		MakeStep(str).MapKeywordToValid().Should().Be(expected);

	[Fact]
	public void MapStepKeywordToValid_Throws_WhenKeywordIsInvalid()
	{
		var mapToValid = () => MakeStep("Ultimately").MapKeywordToValid();
		mapToValid.Should().Throw<UnsupportedKeywordException>()
			.And.Message.Should().ContainAll("Unsupported", "step", "Ultimately", "(at 1:2)");
	}

	[Fact]
	public void MapScenarioKeywordToValid_Succeeds_WithValidKeyword() =>
		MakeScenario("Example").MapKeywordToValid().Should().Be("Example");

	[Fact]
	public void MapScenarioKeywordToValid_Throws_WhenKeywordIsInvalid()
	{
		var mapToValid = () => MakeScenario("Scenario Outline").MapKeywordToValid();
		mapToValid.Should().Throw<UnsupportedKeywordException>()
			.And.Message.Should().ContainAll("Unsupported", "scenario", "Scenario Outline", "(at 3:4)");
	}

	private static Step MakeStep(string keyword) => new(
		new Location(1, 2),
		keyword,
		default,
		default);

	private static Scenario MakeScenario(string keyword) => new(
		default,
		new Location(3, 4),
		keyword,
		default,
		default,
		default,
		default);
}
