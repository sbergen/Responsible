using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using static Responsible.Bdd.Keywords;

namespace Responsible.Bdd
{
	public abstract class BddTest
	{
		protected abstract TestInstructionExecutor Executor { get; }

		// Has to be public for NUnit
		public IEnumerator ExecuteScenario(string scenario, IMethodInfo testMethod)
		{
			var steps = (IEnumerable<IBddStep>)testMethod.Invoke(this);
			return Scenario(scenario).WithSteps(steps.ToArray()).ToYieldInstruction(this.Executor);
		}
	}

	public class FeatureAttribute : NUnitAttribute, IFixtureBuilder
	{
		private static readonly NUnitTestCaseBuilder TestCaseBuilder = new NUnitTestCaseBuilder();
		private static readonly NUnitTestFixtureBuilder DefaultFixtureBuilder = new NUnitTestFixtureBuilder();

		private readonly string description;

		public FeatureAttribute(string description)
		{
			this.description = $"Feature: {description}";
		}

		public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
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

	[JetBrains.Annotations.MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class ScenarioAttribute : Attribute
	{
		public readonly string Description;

		public ScenarioAttribute(string description)
		{
			this.Description = description;
		}
	}
}
