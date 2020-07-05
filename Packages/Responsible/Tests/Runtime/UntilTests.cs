using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.RF;

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

			Assert.IsTrue(completed);
		}
	}
}