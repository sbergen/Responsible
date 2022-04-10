using System;
using System.Collections.Generic;
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
		private static readonly NUnitTestCaseBuilder TestCaseBuilder = new NUnitTestCaseBuilder();
		private static readonly NUnitTestFixtureBuilder DefaultFixtureBuilder = new NUnitTestFixtureBuilder();

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
				suite.RunState = RunState.NotRunnable;
				suite.Properties.Set(PropertyNames.SkipReason, $"Feature class must inherit from {nameof(BddTest)}");
				yield return suite;
				yield break;
			}

			var executeMethod = typeInfo
				.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Single(m => m.Name == nameof(BddTest.ExecuteScenario));

			foreach (var method in typeInfo.GetMethods(BindingFlags.Public | BindingFlags.Instance))
			{
				var scenarioAttributes = method.GetCustomAttributes<ScenarioAttribute>(true);

				if (scenarioAttributes.Any())
				{
					var scenarioDescription = scenarioAttributes.Single().Description;
					var parameters = new TestCaseParameters(new object[] { scenarioDescription, method })
					{
						ExpectedResult = null,
						HasExpectedResult = true,
					};

					var test = TestCaseBuilder.BuildTestMethod(executeMethod, suite, parameters);
					test.Name = $"Scenario: {scenarioDescription}";

					suite.Add(test);
				}
			}

			yield return suite;

			var defaultFixture = DefaultFixtureBuilder.BuildFrom(typeInfo);
			if (defaultFixture.Tests.Any())
			{
				yield return defaultFixture;
			}
		}
	}
}
