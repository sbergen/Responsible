using System;
using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.RF;
using static Responsible.Tests.Runtime.TestInstances;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests.Runtime
{
	public class WaitForTests
	{
		[UnityTest]
		public IEnumerator WaitForCondition_Completes_WhenConditionMet()
		{
			var fulfilled = false;
			var completed = false;
			using (WaitForCondition("Wait for fulfilled", () => fulfilled)
				.ExpectWithinSeconds(10)
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
			var result = ImmediateTrue
				.ExpectWithinSeconds(10)
				.Execute()
				.Wait(TimeSpan.Zero);

			Assert.IsTrue(result);
		}

		[Test]
		public void WaitForSeconds_Completes_AfterTimeout()
		{
			var scheduler = new TestScheduler();
			var completed = false;
			using (TestInstructionExtensions.OverrideExecutor(scheduler))
			using (WaitForSeconds(2).Execute().Subscribe(_ => completed = true))
			{
				Assert.IsFalse(completed);
				scheduler.AdvanceBy(OneSecond);
				Assert.IsFalse(completed);
				scheduler.AdvanceBy(OneSecond);
				Assert.IsTrue(completed);
			}
		}

		[UnityTest]
		public IEnumerator WaitForAllOf_Completes_AfterAllComplete()
		{
			var fulfilled1 = false;
			var fulfilled2 = false;
			var fulfilled3 = false;
			bool? completed = null;

			using (WaitForAllOf(
				WaitForCondition("cond 1", () => fulfilled1, () => true),
				WaitForCondition("cond 2", () => fulfilled2, () => false),
					WaitForCondition("cond 3", () => fulfilled3, () => false))
				.ExpectWithinSeconds(10)
				.Execute()
				.Subscribe(val => completed = val))
			{
				Assert.IsNull(completed);
				yield return null;

				fulfilled1 = true;
				Assert.IsNull(completed);
				yield return null;

				fulfilled2 = true;
				Assert.IsNull(completed);
				yield return null;

				fulfilled3 = true;
				Assert.IsNull(completed);
				yield return null;

				// Completes on next frame
				yield return null;
				Assert.IsTrue(completed);
			}
		}

		[Test]
		public void WaitForAllOf_Completes_WhenSynchronouslyMet()
		{
			var result = WaitForAllOf(ImmediateTrue, ImmediateTrue, ImmediateTrue)
				.ExpectWithinSeconds(10)
				.Execute()
				.Wait(TimeSpan.Zero);

			Assert.IsTrue(result);
		}
	}
}
