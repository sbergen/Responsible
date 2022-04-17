using FluentAssertions;
using Xunit;

namespace ResponsibleGherkin.Tests;

public class ConfigurationParserTests
{
	[Fact]
	public void FromPartial_ReturnsEquivalent_WhenAllPropsPresent()
	{
		var partial = new PartialConfiguration(
			FlavorType.Xunit,
			new IndentInfo(1, IndentType.Tabs),
			"MyNamespace",
			"MyTestBase",
			"MyExecutor");

		var complete = Configuration.FromPartial(partial);

		complete.Should().BeEquivalentTo(partial);
	}

	[Fact]
	public void FromPartial_Throws_WhenPropertiesMissing()
	{
		var partial = new PartialConfiguration(
			null,
			new IndentInfo(1, IndentType.Tabs),
			null,
			"MyTestBase",
			"MyExecutor");

		var fromPartial = () => Configuration.FromPartial(partial);

		fromPartial.Should()
			.Throw<IncompleteConfigurationException>()
			.WithMessage("Incomplete configuration, missing 2 properties: Flavor, Namespace");
	}

	[Fact]
	public void Merge_Throws_WhenNoConfigurationPassed()
	{
		var mergeEmpty = () => Configuration.Merge();
		mergeEmpty.Should()
			.Throw<IncompleteConfigurationException>()
			.WithMessage("*5*");
	}

	[Fact]
	public void Merge_ReturnsExpectedValue_WhenMergingPartials()
	{
		var partial1 = new PartialConfiguration(
			FlavorType.Xunit, // This should be prioritized
			null,
			null,
			null,
			null);

		var partial2 = new PartialConfiguration(
			FlavorType.NUnit,
			new IndentInfo(1, IndentType.Tabs), // This should be prioritized
			"MyNamespace",
			null,
			null);

		var partial3 = new PartialConfiguration(
			FlavorType.Unity,
			new IndentInfo(4, IndentType.Spaces),
			null,
			"MyTestBase",
			"MyExecutor");

		var merged = Configuration.Merge(partial1, partial2, partial3);
		var expected = new Configuration(
			FlavorType.Xunit,
			new IndentInfo(1, IndentType.Tabs),
			"MyNamespace",
			"MyTestBase",
			"MyExecutor");

		merged.Should().BeEquivalentTo(expected);
	}
}
