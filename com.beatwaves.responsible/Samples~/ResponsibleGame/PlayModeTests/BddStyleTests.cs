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
		public IEnumerator TriggerKey_Restarts_AfterFailing()
		{
			var miss = this
				.TriggerHit(false)
				.ExpectWithinSeconds(2)
				.ContinueWith(WaitForFrames(1));

			var fail = Enumerable.Repeat(miss, Status.StartingLives).Sequence();

			var assertRestarted = Do(
				"Assert restarted",
				() => Assert.IsTrue(ExpectStatusInstance().IsAlive));

			return Scenario("The game should be restartable by pressing the trigger key")
				.WithSteps(
					Given("the player has failed", fail),
					When("the user presses the trigger key", this.MockTriggerInput()),
					Then("the game should be restarted", assertRestarted))
				.ToYieldInstruction(this.TestInstructionExecutor);
		}
	}
}
