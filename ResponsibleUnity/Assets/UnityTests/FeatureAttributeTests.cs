using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using Responsible.Bdd;

namespace Responsible.UnityTests
{
	public class FeatureAttributeTests
	{
		[Test]
		public void FeatureAttribute_AppliesNameToTest()
		{
			var test = new TestMethod(new MethodWrapper(
				typeof(FeatureAttributeTests),
				nameof(this.FeatureAttribute_AppliesNameToTest)));
			var applyToTest = (IApplyToTest)new FeatureAttribute("Expected Name");

			applyToTest.ApplyToTest(test);

			Assert.AreEqual(
				"Feature: Expected Name",
				test.Name);
		}

	}
}
