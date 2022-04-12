using System.Linq;
using NUnit.Framework;
using Responsible;
using Responsible.Bdd;
using static Responsible.Bdd.Keywords;
using static Responsible.Responsibly;

namespace ResponsibleGame.PlayModeTests
{
	// Example of BDD-style tests
	[Feature("Restarting the game")]
	public class BddStyleTests : SystemTest
	{
		[Scenario("The game should be restartable easily after failure")]
		public IBddStep[] Restart() => new[]
		{
			Given("the player has failed", this.FailTheGame()),
			When("the player presses the trigger key", this.SimulateTriggerInput()),
			Then("the game should be restarted", AssertTheGameHasBeenRestarted())
		};

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
