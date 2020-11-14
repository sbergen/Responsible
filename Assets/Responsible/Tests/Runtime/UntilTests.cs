using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests.Runtime
{
	public class UntilTests : ResponsibleTestBase
	{
		[UnityTest]
		public IEnumerator Until_CompletesResponder_WhenReadyBeforeCompletion()
		{
			var readyToReact = false;
			var startedToReact = false;
			var mayCompleteReaction = false;
			var reactionCompleted = false;
			var shouldContinue = false;
			var completed = false;

			var react = DoAndReturn("Set started to react", () => startedToReact = true)
				.ContinueWith(WaitForCondition("May complete", () => mayCompleteReaction)
					.ExpectWithinSeconds(1)
					.ContinueWith(DoAndReturn("Set reaction completed", () => reactionCompleted = true)));

			var respondUntil = WaitForCondition("Ready", () => readyToReact)
				.ThenRespondWith("React", react)
				.Optionally()
				.Until(WaitForCondition("Continue", () => shouldContinue))
				.ExpectWithinSeconds(1);

			respondUntil.ToObservable(this.Executor).Subscribe(_ => completed = true);

			readyToReact = true;

			yield return null;
			Assert.IsTrue(startedToReact);

			shouldContinue = true;

			// Yield a few times just to be safe
			yield return null;
			yield return null;

			mayCompleteReaction = true;
			yield return null;

			Assert.IsTrue(reactionCompleted);
			Assert.IsTrue(completed);
		}

		[UnityTest]
		public IEnumerator Until_DoesNotExecute_IfConditionIsMetFirst()
		{
			var cond1 = false;
			var untilCond = false;
			var cond2 = false;

			var firstCompleted = false;
			var secondCompleted = false;

			var state = WaitForCondition("Wait for first cond", () => cond1)
				.ThenRespondWith("First response", _ => firstCompleted = true)
				.Optionally()
				.Until(WaitForCondition("Until cond", () => untilCond))
				.ThenRespondWith("Second response", WaitForCondition("Second cond", () => cond2)
					.ExpectWithinSeconds(1)
					.ContinueWith(_ => DoAndReturn("Set second completed", () => secondCompleted = true)))
				.ExpectWithinSeconds(1)
				.CreateState();

			state.ToObservable(this.Executor)
				.Subscribe(_ => secondCompleted = true);

			untilCond = true;
			yield return null;
			yield return null;
			yield return null;

			cond1 = true;

			// A couple of extra yields to be safe
			yield return null;
			yield return null;
			yield return null;
			Assert.IsFalse(firstCompleted);
			Assert.IsFalse(secondCompleted);

			cond2 = true;
			yield return null;

			Assert.AreEqual(
				(false, true),
				(firstCompleted, secondCompleted));
		}

		[UnityTest]
		public IEnumerator Until_TimesOut_AsExpected()
		{
			var completed = false;

			Never
				.ThenRespondWith("complete", _ => completed = true)
				.Optionally()
				.Until(Never)
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			yield return null;
			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;

			Assert.IsFalse(completed);
			Assert.IsInstanceOf<AssertionException>(this.Error);
		}
	}
}