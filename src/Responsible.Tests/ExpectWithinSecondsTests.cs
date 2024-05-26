using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class ExpectWithinSecondsTests : ResponsibleTestBase
	{
		[Test]
		public async Task ExpectCondition_TerminatesWithError_AfterTimeout()
		{
			var task = Never
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			task.IsFaulted.Should().BeFalse();
			this.Scheduler.AdvanceFrame(OneSecond);
			var error = await AwaitFailureExceptionForUnity(task);
			error.InnerException.Should().BeOfType<TimeoutException>();
		}

		[Test]
		public async Task ExpectWithinSeconds_CancelsSuccessfully()
		{
			var cts = new CancellationTokenSource();
			var task = Never
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor, cts.Token);

			cts.Cancel();
			var error = await AwaitFailureExceptionForUnity(task);
			error.InnerException.Should().BeOfType<TaskCanceledException>();
		}

		[Test]
		public async Task ExpectCondition_ContainsErrorDetails_WhenTimedOut()
		{
			await this.AssertErrorDetailsAfterOneSecond(
				Never.ExpectWithinSeconds(1).BoxResult(),
				state => StateAssert.StringContainsInOrder(state)
					.Failed("Never EXPECTED WITHIN"));
		}

		[Test]
		public async Task ExpectCondition_ContainsErrorDetails_WhenExceptionThrown()
		{
			await this.AssertErrorDetailsAfterOneSecond(
				WaitForCondition(
						"Throwing condition",
						() => throw new Exception("Test"))
					.ExpectWithinSeconds(1),
				state => StateAssert.StringContainsInOrder(state)
					.Failed("Throwing condition EXPECTED WITHIN"));
		}

		[Test]
		public void ExpectConditionDescription_ContainsSubConditions_WithCompoundCondition()
		{
			var description = WaitForCondition("First", () => false)
				.AndThen(WaitForCondition("Second", () => false))
				.ExpectWithinSeconds(1)
				.CreateState()
				.ToString();

			StateAssert.StringContainsInOrder(description)
				.Details($"{1d:0.00} s")
				.NotStarted("First")
				.NotStarted("Second");
		}

		[Test]
		public void ExpectConditionDescription_Inlined_WithDiscreteCondition()
		{
			var description = WaitForCondition("Only", () => false)
				.ExpectWithinSeconds(1)
				.CreateState()
				.ToString();

			StateAssert.StringContainsInOrder(description)
				.NotStarted($"Only EXPECTED WITHIN {1d:0.00} s");
		}

		[Test]
		public async Task ExpectResponder_TerminatesWithError_IfWaitNotFulfilled()
		{
			var task = Never
				.ThenRespondWithAction("NOP", Nop)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			task.IsFaulted.Should().BeFalse();
			this.Scheduler.AdvanceFrame(OneSecond);
			(await AwaitFailureExceptionForUnity(task)).Should().NotBeNull();
		}

		[Test]
		public void ExpectResponder_DoesNotTerminateWithTimeout_IfWaitFulfilled()
		{
			// The instruction takes longer than the timeout
			// => The timeout applies only to the wait!
			var task = ImmediateTrue
				.ThenRespondWith("Wait for two seconds", WaitForSeconds(2))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.Scheduler.AdvanceFrame(OneSecond);
			task.IsFaulted.Should().BeFalse();
			task.IsCompleted.Should().BeFalse();

			this.Scheduler.AdvanceFrame(OneSecond);
			task.IsFaulted.Should().BeFalse();
			task.IsCompleted.Should().BeTrue();
		}

		[Test]
		public async Task ExpectResponder_ContainsErrorDetails_WhenConditionTimedOut()
		{
			var responder = Never.ThenRespondWithAction("Nop", Nop);
			await this.AssertErrorDetailsAfterOneSecond(
				responder.ExpectWithinSeconds(1),
				state => StateAssert.StringContainsInOrder(state)
					.Details("timed out")
					.Failed("Nop CONDITION EXPECTED WITHIN")
					.Details("Failed with")
					.Details("Test operation stack"));
		}

		[Test]
		public async Task ExpectResponder_ContainsErrorDetails_WhenInstructionTimedOut()
		{
			var responder = ImmediateTrue.ThenRespondWith("Response", Never.ExpectWithinSeconds(0.5));
			await this.AssertErrorDetailsAfterOneSecond(
				responder.BoxResult().ExpectWithinSeconds(1),
				state => StateAssert.StringContainsInOrder(state)
					.Details("timed out")
					.Failed("Response CONDITION EXPECTED WITHIN")
					.Failed("Never EXPECTED WITHIN")
					.Details("Failed with")
					.Details("Test operation stack"));
		}

		[Test]
		public async Task ExpectResponder_ContainsErrorDetails_WhenExceptionThrown()
		{
			var responder = ImmediateTrue.ThenRespondWithAction("Throw error", _ => throw new Exception("Test"));
			await this.AssertErrorDetailsAfterOneSecond(
				responder.ExpectWithinSeconds(1),
				state => StateAssert.StringContainsInOrder(state)
					.Details("failed")
					.Failed("Throw error CONDITION EXPECTED WITHIN")
					.Failed("Throw error")
					.Details("Failed with")
					.Details("Test operation stack"));
		}

		private async Task AssertErrorDetailsAfterOneSecond(
			ITestInstruction<object> instruction,
			Action<string> detailsAssert)
		{
			var task = instruction.ToTask(this.Executor);

			this.Scheduler.AdvanceFrame(OneSecond);

			var exception = await AwaitFailureExceptionForUnity(task);
			detailsAssert(exception.Message);

			var multipleFailuresDescription =
				$"there should only be a single failure details, but was:\n{exception.Message}";

			Regex.Matches(exception.Message, "Failed with").Count
				.Should().Be(1, multipleFailuresDescription);

			Regex.Matches(exception.Message, "Test operation stack").Count
				.Should().Be(1, multipleFailuresDescription);
			}
	}
}
