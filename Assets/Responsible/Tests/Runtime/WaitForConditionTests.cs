using System;
using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests.Runtime
{
	public class WaitForConditionTests : ResponsibleTestBase
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

		[UnityTest]
		public IEnumerator WaitForCondition_LogsDetails_WhenTimedOut()
		{
			using (WaitForCondition(
					"Never",
					() => false,
					builder => builder.AddDetails("Should be in output"))
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError))
			{
				this.Scheduler.AdvanceBy(TimeSpan.FromSeconds(2));
				yield return null;

				Assert.IsInstanceOf<AssertionException>(this.Error);
				StringAssert.Contains("Should be in output", this.Error.Message);
			}
		}

		[UnityTest]
		public IEnumerator WaitForCondition_LogsCorrectDetails_WhenCanceled()
		{
			var extraContextRequested = false;

			var respond = WaitForCondition(
					"Never",
					() => false,
					_ => extraContextRequested = true)
				.ThenRespondWith("Do nothing", Nop);

			// Never execute the optional responder, leading to the wait being canceled.
			// But error out afterwards, to get a failure message.
			// We could do this in a simpler way using CreateState,
			// but that would not be as realistic.
			using (respond.Optionally()
				.Until(ImmediateTrue)
				.AndThen(Never)
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError))
			{
				this.Scheduler.AdvanceBy(TimeSpan.FromSeconds(2));
				yield return null;

				Assert.IsInstanceOf<AssertionException>(this.Error);
				Assert.IsFalse(extraContextRequested, "Should not request extra context when canceled");
				Assert.That(this.Error.Message, Does.Match(@"\[-\].*Never.*[Cc]anceled"));
			}
		}
	}
}