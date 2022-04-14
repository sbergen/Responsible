using System.Collections;
using NUnit.Framework;
using Responsible.Unity;
using UnityEngine.TestTools;
using static Responsible.Bdd.Keywords;
using static Responsible.Responsibly;

namespace Responsible.UnityTests
{
	public class UnityBddTests
	{
		private TestInstructionExecutor executor;

		private bool givenExecuted;
		private bool whenExecuted;

		[SetUp]
		public void SetUp()
		{
			this.givenExecuted = false;
			this.whenExecuted = false;
			this.executor = new UnityTestInstructionExecutor();
		}

		[TearDown]
		public void TearDown()
		{
			this.executor.Dispose();
		}

		[UnityTest]
		public IEnumerator YieldScenario_RunsFullScenario() => this.executor.YieldScenario(
			Scenario("A basic BDD-style test runs without error"),
			Given(
				"the test is set up properly",
				Do("Execute Given", () => this.givenExecuted = true)),
			When(
				"we execute the when step",
				Do("Execute When", () => this.whenExecuted = true)),
			Then(
				"the state of the test class should be in the final expected state",
				Do(
					"Assert state",
					() => Assert.AreEqual(
						(true, true),
						(this.givenExecuted, this.whenExecuted)))));
	}
}
