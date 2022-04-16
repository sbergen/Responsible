using System;
using FluentAssertions;
using Xunit;

namespace ResponsibleGherkin.Tests;

public class FlavorTests
{
	[Fact]
	public void FromType_ThrowsMeaningfulException_ForInvalidType()
	{
		var fromType = () => Flavor.FromType((FlavorType)42);

		fromType.Should()
			.Throw<ArgumentOutOfRangeException>("an invalid flavor was used")
			.WithMessage("*flavor*42*", "the message should be informative");
	}

	[Theory]
	[ClassData(typeof(EnumValues<FlavorType>))]
	public void FromType_DoesNotThrow_ForValidType(FlavorType type)
	{
		var fromType = () => Flavor.FromType(type);
		fromType.Should().NotThrow("valid inputs should be accepted");
	}
}
