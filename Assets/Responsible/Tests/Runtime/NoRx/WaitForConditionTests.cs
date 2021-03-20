using System;
using NUnit.Framework;
using Responsible.NoRx;
using static Responsible.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests.Runtime.NoRx
{
	public class WaitForConditionTests : ResponsibleTestBase
	{
		[Test]
		public void WaitForConditionOn_Completes_OnlyWhenConditionIsTrueOnReturnedObject()
		{
			object boxedBool = null;

			var task = WaitForConditionOn(
					"Wait for boxedBool to be true",
					() => boxedBool,
					obj => obj is bool asBool && asBool)
				.ExpectWithinSeconds(10)
				.ToTask(this.Executor);

			Assert.IsFalse(task.IsCompleted, "Should not be completed before condition is met");
			this.AdvanceDefaultFrame();

			// Completes on next frame
			boxedBool = true;
			Assert.IsFalse(task.IsCompleted, "Should not be completed before poller is run");
			this.AdvanceDefaultFrame();

			Assert.IsTrue(task.IsCompleted, "Should be completed after polling");
		}

		[Test]
		public void WaitForCondition_Completes_WhenConditionMet()
		{
			var fulfilled = false;
			var task = WaitForCondition("Wait for fulfilled", () => fulfilled)
				.ExpectWithinSeconds(10)
				.ToTask(this.Executor);

			Assert.IsFalse(task.IsCompleted, "Should not be completed before condition is met");
			this.AdvanceDefaultFrame();

			// Completes on next frame
			fulfilled = true;
			Assert.IsFalse(task.IsCompleted, "Should not be completed before poller is run");
			this.AdvanceDefaultFrame();

			Assert.IsTrue(task.IsCompleted, "Should be completed after polling");
		}

		[Test]
		public void WaitForCondition_CompletesImmediately_WhenSynchronouslyMet()
		{
			var result = ImmediateTrue
				.ExpectWithinSeconds(10)
				.ToTask(this.Executor)
				.Wait(TimeSpan.Zero);

			Assert.IsTrue(result);
		}

		[Test]
		public void WaitForCondition_ContainsDetails_WhenTimedOut()
		{
			var task = WaitForCondition(
					"Never",
					() => false,
					builder => builder.AddDetails("Should be in output"))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.TimeProvider.AdvanceFrame(OneSecond);

			var exception = GetAssertionException(task);
			StringAssert.Contains("Should be in output", exception.Message);
		}

		/*
		[UnityTest]
		public IEnumerator WaitForCondition_ContainsCorrectDetails_WhenCanceled()
		{
			var extraContextRequested = false;

			var respond = WaitForCondition(
					"Should be canceled",
					() => false,
					_ => extraContextRequested = true)
				.ThenRespondWithAction("Do nothing", Nop);

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
				Assert.That(this.Error.Message, Does.Match(@"\[-\].*Should be canceled.*[Cc]anceled"));
			}
		}*/
	}
}
