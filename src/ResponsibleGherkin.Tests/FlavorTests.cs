using System;
using Xunit;

namespace ResponsibleGherkin.Tests;

public class FlavorTests
{
	[Fact]
	public void FromType_ThrowsMeaningfulException_ForInvalidType()
	{
		var exception = Assert.Throws<ArgumentOutOfRangeException>(
			() => Flavor.FromType((FlavorType)42));

		Assert.Contains("flavor", exception.Message);
		Assert.Contains("42", exception.Message);
	}

	[Theory]
	[ClassData(typeof(EnumValues<FlavorType>))]
	public void FromType_DoesNotThrow_ForValidType(FlavorType type)
	{
		Assert.NotNull(Flavor.FromType(type));
	}
}
