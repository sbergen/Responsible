using System;
using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests.Runtime
{
	public class WaitForTests : ResponsibleTestBase
	{
		[UnityTest]
		public IEnumerator WaitForConditionOn_Completes_OnlyWhenConditionIsTrueOnReturnedObject()
		{
			var completed = false;
			object boxedBool = null;

			using (WaitForConditionOn(
					"Wait for boxedBool to be true",
					() => boxedBool,
					obj => obj is bool asBool && asBool)
				.ExpectWithinSeconds(10)
				.ToObservable(this.Executor)
				.Subscribe(_ => completed = true))
			{
				Assert.IsFalse(completed);
				yield return null;

				// Completes on next frame
				boxedBool = true;
				Assert.IsFalse(completed);
				yield return null;
				Assert.IsTrue(completed);
			}
		}

		[UnityTest]
		public IEnumerator WaitForConditionOn_RunsSelector_WhenResultSelectorProvided()
		{
			bool? result = null;
			object boxedBool = null;

			using (WaitForConditionOn(
					"Wait for boxedBool to be true",
					() => boxedBool,
					obj => obj is bool asBool && asBool,
					val => !(bool)val)
				.ExpectWithinSeconds(10)
				.ToObservable(this.Executor)
				.Subscribe(val => result = val))
			{
				Assert.IsNull(result);
				yield return null;

				// Completes on next frame
				boxedBool = true;
				yield return null;
				Assert.IsFalse(result, "The result should be negated by the result selector");
			}
		}

		[UnityTest]
		public IEnumerator WaitForCondition_Completes_WhenConditionMet()
		{
			var fulfilled = false;
			var completed = false;
			using (WaitForCondition("Wait for fulfilled", () => fulfilled)
				.ExpectWithinSeconds(10)
				.ToObservable(this.Executor)
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
				.ToObservable(this.Executor)
				.Wait(TimeSpan.Zero);

			Assert.IsTrue(result);
		}

		[Test]
		public void WaitForLast_Completes_WhenCompleted()
		{
			Subject<int> subject = new Subject<int>();
			int? result = null;

			using (WaitForLast("Wait for last", subject)
				.ExpectWithinSeconds(10)
				.ToObservable(this.Executor)
				.Subscribe(r => result = r))
			{
				Assert.IsNull(result);
				subject.OnNext(1);
				Assert.IsNull(result);
				subject.OnNext(2);
				Assert.IsNull(result);
				subject.OnCompleted();
				Assert.AreEqual(2, result);
			}
		}

		[Test]
		public void WaitForLast_PublishesError_OnEmpty()
		{
			using (WaitForLast("Wait for last on empty", Observable.Empty<int>())
				.ExpectWithinSeconds(10)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError))
			{
				Assert.IsInstanceOf<AssertionException>(this.Error);
			}
		}

		[UnityTest]
		public IEnumerator WaitForLast_TimesOut_WhenTimeoutExceeded()
		{
			using (WaitForLast("Wait for last on never", Observable.Never<int>())
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError))
			{
				yield return null;
				Assert.IsNull(this.Error);

				this.Scheduler.AdvanceBy(OneSecond);
				yield return null;
				Assert.IsInstanceOf<AssertionException>(this.Error);
			}
		}

		[Test]
		public void WaitForSeconds_Completes_AfterTimeout()
		{
			var completed = false;
			using (WaitForSeconds(2).ToObservable(this.Executor).Subscribe(_ => completed = true))
			{
				Assert.IsFalse(completed);
				this.Scheduler.AdvanceBy(OneSecond);
				Assert.IsFalse(completed);
				this.Scheduler.AdvanceBy(OneSecond);
				Assert.IsTrue(completed);
			}
		}

		[UnityTest]
		public IEnumerator WaitForFrames_CompletesAfterTimeout()
		{
			var completed = false;
			using (WaitForFrames(2).ToObservable(this.Executor).Subscribe(_ => completed = true))
			{
				Assert.IsFalse(completed);
				yield return null; // This frame
				Assert.IsFalse(completed);
				yield return null; // First frame
				Assert.IsFalse(completed);
				yield return null; // Second frame
				Assert.IsTrue(completed);
			}
		}

		[UnityTest]
		public IEnumerator WaitForFrames_CompletesAfterThisFrame_WithZeroFrames()
		{
			var completed = false;
			using (WaitForFrames(0).ToObservable(this.Executor).Subscribe(_ => completed = true))
			{
				Assert.IsFalse(completed);
				yield return null; // This frame
				Assert.IsTrue(completed);
			}
		}

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
