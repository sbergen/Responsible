using System;
using System.Threading;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests
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

			this.Scheduler.AdvanceFrame(OneSecond);

			var exception = GetFailureException(task);
			StringAssert.Contains("Should be in output", exception.Message);
		}

		[Test]
		public void WaitForCondition_CleansUpSuccessfully_AfterCompletion()
		{
			var polled = false;
			var unused = WaitForCondition(
					"Complete immediately",
					() =>
					{
						polled = true;
						return true;
					})
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			polled = false;

			this.AdvanceDefaultFrame();
			Assert.IsFalse(polled, "Should not be polled after completion");
		}

		[Test]
		[TaskExceptionTest]
		public void WaitForCondition_CleansUpSuccessfully_AfterCancellation()
		{
			using (var tokenSource = new CancellationTokenSource())
			{
				var polled = false;
				var unused = WaitForCondition(
						"Never complete",
						() =>
						{
							polled = true;
							return false;
						})
					.ExpectWithinSeconds(1)
					.ToTask(this.Executor, tokenSource.Token);

				polled = false; // Reset after initial check
				tokenSource.Cancel();
				this.AdvanceDefaultFrame();
				Assert.IsFalse(polled, "Should not be polled after cancellation");
			}
		}

		[Test]
		public void WaitForCondition_IsNotPolled_WhenCanceledBeforeExecution()
		{
			using (var tokenSource = new CancellationTokenSource())
			{
				var polled = false;
				tokenSource.Cancel();
				var unused = WaitForCondition(
						"Never complete",
						() =>
						{
							polled = true;
							return false;
						})
					.ExpectWithinSeconds(1)
					.ToTask(this.Executor, tokenSource.Token);

				this.AdvanceDefaultFrame();
				Assert.IsFalse(polled, "Should not be polled if canceled before execution");
			}
		}

		[Test]
		public void WaitForCondition_ContainsCorrectDetails_WhenCanceled()
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
			var task = respond.Optionally()
				.Until(ImmediateTrue)
				.AndThen(Never)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.Scheduler.AdvanceFrame(TimeSpan.FromSeconds(2));


			Assert.IsFalse(extraContextRequested, "Should not request extra context when canceled");

			var error = GetFailureException(task);
			StateAssert.StringContainsInOrder(error.Message)
				.Canceled("Should be canceled")
				.FailureDetails();
		}


		[Test]
		public void InlinedOutput_IsGeneratedForInitialState_WhenExpected()
		{
			var state = WaitForCondition("Never", () => false)
				.ExpectWithinSeconds(1)
				.CreateState();

			var stateString = state.ToString();
			StateAssert.StringContainsInOrder(stateString)
				.NotStarted("Never EXPECTED WITHIN");
		}

		[Test]
		public void InlinedOutput_IsGeneratedForCompletedState_WhenExpected()
		{
			var state = WaitForCondition("Immediately", () => true)
				.ExpectWithinSeconds(1)
				.CreateState();
			state.ToTask(this.Executor); // Trigger completion

			var stateString = state.ToString();
			StateAssert.StringContainsInOrder(stateString)
				.Completed("Immediately EXPECTED WITHIN");
		}
	}
}
