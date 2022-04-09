using System.Collections;
using System.Linq;
using NUnit.Framework;
using Responsible;
using UnityEngine.TestTools;
using static Responsible.Responsibly;

namespace ResponsibleGame.PlayModeTests
{
	/// <summary>
	/// BDD-style example, which should be about the same as <see cref="BasicSystemTests"/>
	/// </summary>
	public class BddStyleTests : SystemTest
	{
		[UnityTest]
		public IEnumerator TriggerKey_Restarts_AfterFailing() => BDD(
			given: TheUserHasFailed,
			when: TheTriggerButtonIsPressed,
			then: TheGameIsRestarted);

		private ITestInstruction<object> TheUserHasFailed()
		{
			var miss = this
				.TriggerHit(false)
				.ExpectWithinSeconds(2)
				.ContinueWith(WaitForFrames(1));

			return Enumerable.Repeat(miss, Status.StartingLives).Sequence();
		}

		private ITestInstruction<object> TheTriggerButtonIsPressed() => this.MockTriggerInput();

		private void TheGameIsRestarted() => Assert.IsTrue(ExpectStatusInstance().IsAlive);
	}
}
