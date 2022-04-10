using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using Responsible.Bdd;

namespace Responsible.UnityTests
{
	public class FeatureAttributeTests
	{
		private class NonBddTest
		{
		}

		private class ValidBddTest : BddTest
		{
			[Scenario("Test scenario 1")]
			public IBddStep[] TestScenario1() => Array.Empty<IBddStep>();

			[Scenario("Test scenario 2")]
			public IBddStep[] TestScenario2() => Array.Empty<IBddStep>();
		}

		private class MixedBddTest : BddTest
		{
			[Scenario("Test scenario")]
			public IBddStep[] TestScenario() => Array.Empty<IBddStep>();

			[Ignore("Just used for attribute testing")]
			[Test]
			public void NormalTest()
			{
			}
		}

		[Test]
		public void BuildingTest_ShouldMarkItAsNotRunnable_WhenClassDoesNotInheritBddTest()
		{
			var suite = BuildSuites<NonBddTest>().Single();

			Assert.AreEqual(RunState.NotRunnable, suite.RunState);
			StringAssert.Contains(
				"must inherit from BddTest",
				(string)suite.Properties.Get(PropertyNames.SkipReason));
		}

		[Test]
		public void BuildingTest_ShouldHaveGivenName_WhenClassIsSetUpCorrectly()
		{
			var suite = BuildSuites<ValidBddTest>().Single();
			Assert.AreEqual("Feature: Test feature", suite.Name);
		}

		[Test]
		public void BuildingTest_ShouldContainAllScenarios_WhenClassIsSetUpCorrectly()
		{
			var testNames = BuildSuites<ValidBddTest>()
				.Single()
				.Tests.Select(t => t.Name)
				.ToArray();

			CollectionAssert.AreEquivalent(
				new[] { "Scenario: Test scenario 1", "Scenario: Test scenario 2" },
				testNames);
		}

		[Test]
		public void BuildingTests_ShouldReturnTwoSuites_WhenMixingStyles()
		{
			var suiteNames = BuildSuites<MixedBddTest>()
				.Select(s => s.Name)
				.ToArray();

			CollectionAssert.AreEquivalent(
				new[]
				{
					"Feature: Test feature",
					$"{nameof(FeatureAttributeTests)}+{nameof(MixedBddTest)}"
				},
				suiteNames);
		}

		[Test]
		public void BuildingTests_ShouldReturnAllTests_WhenMixingStyles()
		{
			var testNames = BuildSuites<MixedBddTest>()
				.SelectMany(suite => suite.Tests)
				.Select(test => test.Name)
				.ToArray();

			CollectionAssert.AreEquivalent(
				new[]
				{
					"Scenario: Test scenario",
					"NormalTest",
				},
				testNames);
		}

		private static IEnumerable<TestSuite> BuildSuites<T>()
		{
			var fakeAttribute = new FeatureAttribute("Test feature");
			var fixtureBuilder = (IFixtureBuilder)fakeAttribute;
			return fixtureBuilder.BuildFrom(new TypeWrapper(typeof(T)));
		}
	}
}
