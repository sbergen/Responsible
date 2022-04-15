using System;
using NUnit.Framework;

namespace ResponsibleGherkin.Tests;

public class FlavorTests
{
	[Test]
	public void FromType_ThrowsMeaningfulException_ForInvalidType()
	{
		var exception = Assert.Throws<ArgumentOutOfRangeException>(
			() => Flavor.FromType((FlavorType)42));

		StringAssert.Contains("flavor", exception!.Message);
		StringAssert.Contains("42", exception.Message);
	}

	[Test]
	public void FromType_DoesNotThrow_ForValidType(
		[Values] FlavorType type)
	{
		Assert.DoesNotThrow(() => Flavor.FromType(type));
	}
}
