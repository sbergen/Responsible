using System.Collections;
using System.Linq;
using NUnit.Framework;
using Responsible;
using UnityEngine.TestTools;
using static Responsible.Responsibly;

namespace ResponsibleGame.PlayModeTests
{
	public class BasicSystemTests : SystemTest
	{
		[UnityTest]
		public IEnumerator HittingTargetArea_ProducesHitMarker()
		{
			yield return this.TriggerHit(true)
				.ExpectWithinSeconds(2)
				.ContinueWith(Do("Assert that there should be one hit marker", () =>
				{
					var markers = GetAllMarkers();
					Assert.AreEqual(1, markers.Length);
					Assert.IsInstanceOf<Hit>(markers[0]);
				}))
				.ToYieldInstruction(this.Executor);
		}

		[UnityTest]
		public IEnumerator MissingTargetArea_ProducesMissMarker()
		{
			yield return this.TriggerHit(false)
				.ExpectWithinSeconds(2)
				.ContinueWith(Do("Assert that there should be one miss marker", () =>
				{
					var markers = GetAllMarkers();
					Assert.AreEqual(1, markers.Length);
					Assert.IsInstanceOf<Miss>(markers[0]);
				}))
				.ToYieldInstruction(this.Executor);
		}

		[UnityTest]
		public IEnumerator TriggerKey_Restarts_AfterFailing()
		{
			var miss = this
				.TriggerHit(false)
				.ExpectWithinSeconds(2)
				.ContinueWith(WaitForFrames(1));

			var fail = Enumerable.Repeat(miss, Status.StartingLives).Sequence();

			yield return fail
				.ContinueWith(Do("Assert that that the player is dead", () =>
				{
					Assert.IsFalse(ExpectStatusInstance().IsAlive);
				}))
				.ContinueWith(this.SimulateTriggerInput())
				.ContinueWith(Do("Assert that the player is alive again", () =>
				{
					Assert.IsTrue(ExpectStatusInstance().IsAlive);
				}))
				.ToYieldInstruction(this.Executor);
		}

		[UnityTest]
		public IEnumerator PlayToTenPoints_ProducesExpectedEndResult()
		{
			// Yes, this test is pretty undeterministic, but it demonstrates
			// how Responsible could be used in inherently undeterministic environments.

			var hit = this
				.TriggerHit(true)
				.ExpectWithinSeconds(2)
				.ContinueWith(WaitForFrames(1));

			var miss = this
				.TriggerHit(false)
				.ExpectWithinSeconds(2)
				.ContinueWith(WaitForFrames(1));

			var threeHitsAndAMiss = new[] { hit, hit, hit, miss }.Sequence();

			yield return new[]
				{
					hit,
					threeHitsAndAMiss,
					threeHitsAndAMiss,
					threeHitsAndAMiss,
				}
				.Sequence()
				.ContinueWith(Do("Assert that there are 10 miss and 3 hit markers", () =>
				{
					var markers = GetAllMarkers();
					Assert.AreEqual(13, markers.Length);
					Assert.AreEqual(3, markers.Count(item => item is Miss));
					Assert.AreEqual(10, markers.Count(item => item is Hit));
				}))
				.ContinueWith(Do("Assert that the player is dead and has scored 10 points", () =>
				{
					var status = ExpectStatusInstance();
					Assert.IsFalse(status.IsAlive);
					Assert.AreEqual(10, status.Score);
				}))
				.ToYieldInstruction(this.Executor);
		}
	}
}
