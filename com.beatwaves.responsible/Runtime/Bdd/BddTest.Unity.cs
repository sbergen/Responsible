using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using Responsible.Unity;
using static Responsible.Bdd.Keywords;

namespace Responsible.Bdd
{
	/// <summary>
	/// Base class for BDD-style tests using attributes.
	/// </summary>
	/// <remarks>
	/// This is Unity-only, because we depend on NUnit in the attributes,
	/// and don't depend on NUnit for the vanilla C# version of Responsible.
	/// A non-Unity version might be implemented later.
	/// </remarks>
	/// <seealso cref="FeatureAttribute"/>
	/// <seealso cref="ScenarioAttribute"/>
	public abstract class BddTest
	{
		/// <summary>
		/// A test instruction executor that is automatically set up and torn down between tests.
		/// See <see cref="MakeExecutor"/> for customization options.
		/// </summary>
		protected TestInstructionExecutor Executor { get; private set; }

		/// <summary>
		/// Set-up method for NUnit, should not be used manually.
		/// </summary>
		[SetUp]
		public void BddTestSetUp() => this.Executor = this.MakeExecutor();

		/// <summary>
		/// Tear-down method for NUnit, should not be used manually.
		/// </summary>
		[TearDown]
		public void BddTestTearDown() => this.Executor.Dispose();

		/// <summary>
		/// Helper method for executing BDD steps, should not be directly used.
		/// </summary>
		/// <param name="scenarioAttribute">The attribute this scenario is built from.</param>
		/// <param name="testMethod">Test method that returns the test steps.</param>
		/// <returns>An IEnumerator for Unity to run</returns>
		/// <remarks>This is public only because NUnit requires it to be!</remarks>
		public IEnumerator ExecuteScenario(ScenarioAttribute scenarioAttribute, IMethodInfo testMethod)
		{
			var steps = (IEnumerable<IBddStep>)testMethod.Invoke(this, scenarioAttribute.Parameters);
			return Scenario(scenarioAttribute.Description)
				.WithSteps(steps.ToArray()).ToYieldInstruction(this.Executor);
		}

		/// <summary>
		/// Creates a test instruction executor for a test run. May be customized in deriving classes.
		/// </summary>
		/// <returns>A new test instruction executor to be used for the next test.</returns>
		[PublicAPI]
		protected virtual TestInstructionExecutor MakeExecutor() => new UnityTestInstructionExecutor();

		/// <summary>
		/// Gets the scenario execution method for a deriving type.
		/// The exact type matters, because NUnit will use it to resolve set-up and tear-down methods.
		/// </summary>
		internal static IMethodInfo GetExecuteScenarioMethod(ITypeInfo derivingType) =>
			new MethodWrapper(derivingType.Type, nameof(ExecuteScenario));
	}
}
