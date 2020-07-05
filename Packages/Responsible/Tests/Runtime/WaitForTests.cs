using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UniRx;

namespace Responsible.Tests.Editor
{
	public class WaitForTests
	{
		[UnityTest]
		public IEnumerator WaitForCondition_Completes_WhenConditionMet()
		{
			var fulfilled = false;
			var completed = false;
			using (WaitFor
				.Condition("Wait for fulfilled", () => fulfilled)
				.ExpectWithinSeconds(1)
				.Execute()
				.Subscribe(_ => completed = true))
			{
				Assert.IsFalse(completed);
				yield return null;

				// Completes on next frame
				fulfilled = true;
				Assert.IsFalse(completed);
				yield return null;
				Assert.IsTrue(completed);
			}
		}

		[Test]
		public void WaitForCondition_CompletesImmediately_WhenSynchronouslyMet()
		{
			var result = WaitFor
				.Condition("Wait for true", () => true, () => true)
				.ExpectWithinSeconds(1)
				.Execute()
				.Wait(TimeSpan.Zero);

			Assert.IsTrue(result);
		}
	}
}
