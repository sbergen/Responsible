using NUnit.Framework;
using Responsible.Bdd;
using static Responsible.Bdd.Keywords;
using static Responsible.Responsibly;

namespace Responsible.UnityTests
{
	[Feature("BDD-style test")]
	public class BddTests : BddTest
	{
		private bool givenExecuted;
		private bool whenExecuted;

		// It's really hard to know for sure if SetUp and TearDown get run with custom attributes,
		// so log to the console from these.
		// If anyone happens to look at this code and has ideas, please hit me up!

		[SetUp]
		public void SetUp()
		{
			UnityEngine.Debug.Log("Running SetUp");

			this.givenExecuted = false;
			this.whenExecuted = false;
		}

		[TearDown]
		public void TearDown()
		{
			UnityEngine.Debug.Log("Running TearDown");
		}

		[Scenario("A basic BDD-style test runs without error")]
		public IBddStep[] BasicTest() => new[]
		{
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
					() => Assert.AreEqual((true, true), (this.givenExecuted, this.whenExecuted)))),
		};
	}
}
