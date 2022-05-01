using System;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class ResponderTests : ResponsibleTestBase
	{
		private bool readyToReact;
		private bool readyToComplete;

		private bool startedToReact;

		private ITestResponder<object> respondToConditions = null!;

		[SetUp]
		public void SetUp()
		{
			this.readyToReact = this.readyToComplete = this.startedToReact = false;

			var react =
				DoAndReturn("Set started to react", () => this.startedToReact = true)
					.ContinueWith(_ => WaitForCondition(
							"Ready to complete",
							() => this.readyToComplete)
						.ExpectWithinSeconds(3));

			this.respondToConditions =
				WaitForCondition("To be ready", () => this.readyToReact)
					.ThenRespondWith("React", react);
		}

		[Test]
		public void BasicResponder_CompletesAtExpectedTimes()
		{
			var task = this.respondToConditions
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(this.startedToReact);
			Assert.IsFalse(task.IsCompleted);

			this.readyToReact = true;

			this.AdvanceDefaultFrame();
			Assert.IsTrue(this.startedToReact);
			Assert.IsFalse(task.IsCompleted);

			this.readyToComplete = true;
			this.AdvanceDefaultFrame();
			Assert.IsTrue(this.startedToReact);
			Assert.IsTrue(task.IsCompleted);
		}

		[Test]
		public void BasicResponder_Fails_WhenContinuationThrows()
		{
			var task = ImmediateTrue
				.ThenRespondWith<bool, object>(
					"Throw exception in selector",
					_ => throw new Exception("Test exception"))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			var exception = GetFailureException(task);
			StateAssert.StringContainsInOrder(exception.Message)
				.Failed("Throw exception in selector")
				.Completed("True")
				.Details("THEN RESPOND WITH ...")
				.Details("Test exception");
		}

		[Test]
		[TaskExceptionTest]
		public void BasicResponder_RespectsIndividualTimeouts()
		{
			var task = this.respondToConditions
				.ExpectWithinSeconds(3)
				.ToTask(this.Executor);

			// Timeout for condition is 3 seconds
			this.Scheduler.AdvanceFrame(TimeSpan.FromSeconds(2));
			this.readyToReact = true;
			this.Scheduler.AdvanceFrame(TimeSpan.Zero);
			Assert.IsFalse(task.IsCompleted);

			// After this we have waited 4 seconds, which is more than either of the individual timeouts
			this.Scheduler.AdvanceFrame(TimeSpan.FromSeconds(2));
			this.readyToComplete = true;
			this.Scheduler.AdvanceFrame(TimeSpan.Zero);

			Assert.IsTrue(task.IsCompleted);
		}

		[Test]
		[TaskExceptionTest]
		public void BasicResponder_FailureDescription_IsAsExpected()
		{
			var task = WaitForCondition("Condition", () => false)
				.ThenRespondWith("Response", _ => Return(0))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);
			this.Scheduler.AdvanceFrame(OneSecond);

			var message = GetFailureException(task).Message;
			StateAssert.StringContainsInOrder(message)
				.Failed("Response CONDITION EXPECTED WITHIN")
				.Details("WAIT FOR")
				.Canceled("Condition")
				.Details("THEN RESPOND WITH ...");
		}
	}
}
