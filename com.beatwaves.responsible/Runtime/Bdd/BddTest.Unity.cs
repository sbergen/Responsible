using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
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
		/// Helper method for executing BDD steps, should not be used manually,
		/// but must be public to make NUnit happy.
		/// </summary>
		/// <param name="scenario">Name of the test scenario.</param>
		/// <param name="testMethod">Test method that returns the test steps.</param>
		/// <returns></returns>
		public IEnumerator ExecuteScenario(string scenario, IMethodInfo testMethod)
		{
			var steps = (IEnumerable<IBddStep>)testMethod.Invoke(this);
			return Scenario(scenario).WithSteps(steps.ToArray()).ToYieldInstruction(this.Executor);
		}

		/// <summary>
		/// Creates a test instruction executor for a test run. May be customized in deriving classes.
		/// </summary>
		/// <returns>A new test instruction executor to be used for the next test.</returns>
		[PublicAPI]
		protected virtual TestInstructionExecutor MakeExecutor() => new UnityTestInstructionExecutor();
	}
}
