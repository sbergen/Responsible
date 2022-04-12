using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
			[Scenario("Valid scenario in invalid class")]
			public IBddStep[] TestScenario1() => Array.Empty<IBddStep>();
		}

		private class InvalidReturnTypeBddTest : BddTest
		{
			[Scenario("Invalid scenario")]
			public void InvalidScenario()
			{
			}
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

		private class BddTestWithInvalidArgumentCounts : BddTest
		{
			[Scenario("1 != 0", 1)]
			[Scenario("2 != 0", 1, 2)]
			public IBddStep[] ZeroArguments() => Array.Empty<IBddStep>();

			[Scenario("0 != 1")]
			[Scenario("2 != 1", 1, 2)]
			public IBddStep[] OneArgument(object arg1) => Array.Empty<IBddStep>();

			[Scenario("0 != 2")]
			[Scenario("1 != 2", 1)]
			public IBddStep[] TwoArguments(object arg1, object arg2) => Array.Empty<IBddStep>();
		}

		private static readonly IDictionary<string, ITest> InvalidArgumentCountTests =
			BuildSuites<BddTestWithInvalidArgumentCounts>()
				.SelectMany(suite => suite.Tests)
				.ToDictionary(test => test.Name, test => test);

		public static IEnumerable<string> InvalidArgumentCountTestKeys() => InvalidArgumentCountTests.Keys;

		[Test]
		public void BuildingTest_ShouldMarkSuiteAsNotRunnable_WhenClassDoesNotInheritBddTest()
		{
			var suite = BuildSuites<NonBddTest>().Single();
			AssertTestNotRunnableWithReasonContaining(suite, "must inherit from BddTest");
		}

		[Test]
		public void BuildingTest_ShouldMarkScenarioAsNotRunnable_WhenReturnTypeIsIncorrect()
		{
			var test = BuildSuites<InvalidReturnTypeBddTest>()
				.Single()
				.Tests
				.Single();
			AssertTestNotRunnableWithReasonContaining(
				test,
				"Scenario return type must be convertible to IEnumerable<IBddStep>, got System.Void");
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

		[Test]
		public void Scenario_ShouldNotBeRunnable_WhenArgumentCountsMismatch(
			[ValueSource(nameof(InvalidArgumentCountTestKeys))] string testName)
		{
			AssertTestNotRunnableWithReasonContaining(
				InvalidArgumentCountTests[testName],
				"same amount of parameters");
		}

		private static IEnumerable<TestSuite> BuildSuites<T>()
		{
			var fakeAttribute = new FeatureAttribute("Test feature");
			var fixtureBuilder = (IFixtureBuilder)fakeAttribute;
			return fixtureBuilder.BuildFrom(new TypeWrapper(typeof(T)));
		}

		[AssertionMethod]
		private static void AssertTestNotRunnableWithReasonContaining(ITest test, string reason)
		{
			Assert.AreEqual(RunState.NotRunnable, test.RunState);
			StringAssert.Contains(
				reason,
				(string)test.Properties.Get(PropertyNames.SkipReason));
		}
	}
}
