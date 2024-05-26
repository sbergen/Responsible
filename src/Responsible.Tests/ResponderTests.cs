using System;
using System.Threading.Tasks;
using FluentAssertions;
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

		private ITestResponder<object> respondToConditions;

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
			this.startedToReact.Should().BeFalse();
			task.IsCompleted.Should().BeFalse();

			this.readyToReact = true;

			this.AdvanceDefaultFrame();
			this.startedToReact.Should().BeTrue();
			task.IsCompleted.Should().BeFalse();

			this.readyToComplete = true;
			this.AdvanceDefaultFrame();
			this.startedToReact.Should().BeTrue();
			task.IsCompleted.Should().BeTrue();
		}

		[Test]
		public async Task BasicResponder_Fails_WhenContinuationThrows()
		{
			var task = ImmediateTrue
				.ThenRespondWith<bool, object>(
					"Throw exception in selector",
					_ => throw new Exception("Test exception"))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			var exception = await AwaitFailureExceptionForUnity(task);
			StateAssert.StringContainsInOrder(exception.Message)
				.Failed("Throw exception in selector")
				.Completed("True")
				.Details("THEN RESPOND WITH ...")
				.Details("Test exception");
		}

		[Test]
		public void BasicResponder_RespectsIndividualTimeouts()
		{
			var task = this.respondToConditions
				.ExpectWithinSeconds(3)
				.ToTask(this.Executor);

			// Timeout for condition is 3 seconds
			this.Scheduler.AdvanceFrame(TimeSpan.FromSeconds(2));
			this.readyToReact = true;
			this.Scheduler.AdvanceFrame(TimeSpan.Zero);
			task.IsCompleted.Should().BeFalse();

			// After this we have waited 4 seconds, which is more than either of the individual timeouts
			this.Scheduler.AdvanceFrame(TimeSpan.FromSeconds(2));
			this.readyToComplete = true;
			this.Scheduler.AdvanceFrame(TimeSpan.Zero);

			task.IsCompleted.Should().BeTrue();
		}

		[Test]
		public async Task BasicResponder_FailureDescription_IsAsExpected()
		{
			var task = WaitForCondition("Condition", () => false)
				.ThenRespondWith("Response", _ => Return(0))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);
			this.Scheduler.AdvanceFrame(OneSecond);

			var message = (await AwaitFailureExceptionForUnity(task)).Message;

			StateAssert.StringContainsInOrder(message)
				.Failed("Response CONDITION EXPECTED WITHIN")
				.Details("WAIT FOR")
				.JustCanceled("Condition")
				.Details("THEN RESPOND WITH ...");
		}
	}
}
