using System.Collections;
using System.Linq;
using NUnit.Framework;
using Responsible;
using UnityEngine.TestTools;
using static Responsible.Bdd.Keywords;
using static Responsible.Responsibly;

namespace ResponsibleGame.PlayModeTests
{
	// Example of BDD-style tests
	public class BddStyleTests : SystemTest
	{
		[UnityTest]
		public IEnumerator ShouldBeEasilyRestartableAfter_Failure() => this.Executor.YieldScenario(
			Scenario("The game should be restartable easily after failure"),
			Given("the player has failed", this.FailTheGame()),
			When("the player presses the trigger key", this.SimulateTriggerInput()),
			Then("the game should be restarted", AssertTheGameHasBeenRestarted()));

		private ITestInstruction<object> FailTheGame()
		{
			var miss = this
				.TriggerHit(false)
				.ExpectWithinSeconds(2)
				.ContinueWith(WaitForFrames(1));

			return Enumerable.Repeat(miss, Status.StartingLives).Sequence();
		}

		private static ITestInstruction<object> AssertTheGameHasBeenRestarted() => Do(
			"Assert the game has been restarted",
			() => Assert.IsTrue(ExpectStatusInstance().IsAlive));
	}
}
