using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace Responsible.Bdd
{
	/// <summary>
	/// Attribute for annotating a class as BDD-style tests for a feature.
	/// The class must derive from <see cref="BddTest"/>.
	/// </summary>
	/// <seealso cref="ScenarioAttribute"/>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class FeatureAttribute : NUnitAttribute, IFixtureBuilder
	{
		private static readonly NUnitTestCaseBuilder TestCaseBuilder;
		private static readonly NUnitTestFixtureBuilder DefaultFixtureBuilder;

		// A bit of a hack, but if initialized inline, they'll not be in coverage
		// (because NUnit initializes them before coverage is collected).
		[ExcludeFromCodeCoverage]
		static FeatureAttribute()
		{
			TestCaseBuilder = new NUnitTestCaseBuilder();
			DefaultFixtureBuilder = new NUnitTestFixtureBuilder();
		}

		private readonly string description;

		/// <summary>
		/// Annotates a class as a feature test suite with the given description.
		/// All methods in the class that have the <see cref="ScenarioAttribute"/>
		/// will be included as test cases in this test suite.
		/// </summary>
		/// <param name="description">Description of the feature</param>
		public FeatureAttribute(string description)
		{
			this.description = $"Feature: {description}";
		}

		IEnumerable<TestSuite> IFixtureBuilder.BuildFrom(ITypeInfo typeInfo)
		{
			var suite = new TestSuite(typeInfo)
			{
				Name = this.description
			};

			if (!typeof(BddTest).IsAssignableFrom(typeInfo.Type))
			{
				SetNotRunnable(suite, $"Feature class must inherit from {nameof(BddTest)}");
			}

			foreach (var method in typeInfo
				.GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.Where(method => method.GetCustomAttributes<ScenarioAttribute>(true).Any()))
			{
				var test = MakeTestFromScenario(suite, method);
				suite.Add(test);
			}

			yield return suite;

			var defaultFixture = DefaultFixtureBuilder.BuildFrom(typeInfo);
			if (defaultFixture.Tests.Any())
			{
				yield return defaultFixture;
			}
		}

		private static void SetNotRunnable(Test test, string reason)
		{
			test.RunState = RunState.NotRunnable;
			test.Properties.Set(PropertyNames.SkipReason, reason);
		}

		private static Test MakeTestFromScenario(TestSuite suite, IMethodInfo scenarioMethod)
		{
			var scenarioDescription = scenarioMethod
				.GetCustomAttributes<ScenarioAttribute>(true)
				.Single()
				.Description;

			var parameters = new TestCaseParameters(new object[] { scenarioDescription, scenarioMethod })
			{
				ExpectedResult = null,
				HasExpectedResult = true,
			};

			var test = TestCaseBuilder.BuildTestMethod(BddTest.ExecuteScenarioMethod, suite, parameters);
			test.Name = $"Scenario: {scenarioDescription}";

			if (!typeof(IEnumerable<IBddStep>).IsAssignableFrom(scenarioMethod.ReturnType.Type))
			{
				SetNotRunnable(
					test,
					$"Scenario return type must be convertible to IEnumerable<{nameof(IBddStep)}>, got {scenarioMethod.ReturnType}");
			}

			return test;
		}
	}
}
