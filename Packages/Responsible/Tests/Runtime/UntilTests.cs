using System;
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

			var react = Do(() => startedToReact = true)
				.ContinueWith(WaitForCondition("May complete", () => mayCompleteReaction)
					.ExpectWithinSeconds(1)
					.ContinueWith(Do(() => reactionCompleted = true)));

			var respondUntil = WaitForCondition("Ready", () => readyToReact)
				.ThenRespondWith("React", react)
				.Optionally()
				.Until(WaitForCondition("Continue", () => shouldContinue))
				.ExpectWithinSeconds(1);

			respondUntil.Execute().Subscribe(_ => completed = true);

			readyToReact = true;

			yield return null;
			yield return null;
			Assert.IsTrue(startedToReact);

			shouldContinue = true;

			// Yield a few times just to be safe :D
			yield return null;
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

			WaitForCondition("First cond", () => cond1)
				.ThenRespondWith("Complete first", Do(() => firstCompleted = true))
				.Optionally()
				.Until(WaitForCondition("Until cond", () => untilCond))
				.ThenRespondWith("Second", WaitForCondition("Second cond", () => cond2)
					.ExpectWithinSeconds(1)
					.ContinueWith(_ => Do(() => secondCompleted = true)))
				.ExpectWithinSeconds(1)
				.Execute()
				.Subscribe(_ => secondCompleted = true);

			untilCond = true;
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
			Exception error = null;

			Never
				.ThenRespondWith("complete", Do(() => completed = true))
				.Optionally()
				.Until(Never)
				.ExpectWithinSeconds(1)
				.Execute()
				.Subscribe(_ => { }, e => error = e);

			yield return null;
			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;

			Assert.IsFalse(completed);
			Assert.IsInstanceOf<AssertionException>(error);
		}
	}
}