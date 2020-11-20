using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UniRx;
using static Responsible.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests.Runtime
{
	public class WaitForAllOfTests : ResponsibleTestBase
	{
		[UnityTest]
		public IEnumerator WaitForAllOf_Completes_AfterAllComplete()
		{
			var fulfilled1 = false;
			var fulfilled2 = false;
			var fulfilled3 = false;
			bool[] results = null;

			using (WaitForAllOf(
					WaitForCondition("cond 1", () => fulfilled1, () => true),
					WaitForCondition("cond 2", () => fulfilled2, () => false),
					WaitForCondition("cond 3", () => fulfilled3, () => false))
				.ExpectWithinSeconds(10)
				.ToObservable(this.Executor)
				.Subscribe(val => results = val))
			{
				Assert.IsNull(results);
				yield return null;

				fulfilled1 = true;
				Assert.IsNull(results);
				yield return null;

				fulfilled2 = true;
				Assert.IsNull(results);
				yield return null;

				fulfilled3 = true;
				Assert.IsNull(results);
				yield return null;

				// Completes on next frame
				yield return null;
				Assert.AreEqual(
					new[] { true, false, false },
					results);
			}
		}

		[Test]
		public void WaitForAllOf_Completes_WhenSynchronouslyMet()
		{
			var result = WaitForAllOf(ImmediateTrue, ImmediateTrue, ImmediateTrue)
				.ExpectWithinSeconds(10)
				.ToObservable(this.Executor)
				.Wait(TimeSpan.Zero);

			Assert.AreEqual(
				new[] { true, true, true },
				result);
		}
	}
}