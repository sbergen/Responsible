using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
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

			Assert.IsFalse(task.IsFaulted);
			this.Scheduler.AdvanceFrame(OneSecond);
			var error = await AwaitFailureException(task);
			Assert.IsInstanceOf<TimeoutException>(error.InnerException);
		}

		[Test]
		public async Task ExpectWithinSeconds_CancelsSuccessfully()
		{
			var cts = new CancellationTokenSource();
			var task = Never
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor, cts.Token);

			cts.Cancel();
			var error = await AwaitFailureException(task);
			Assert.IsInstanceOf<TaskCanceledException>(error.InnerException);
		}

		[Test]
		public async Task ExpectCondition_ContainsErrorDetails_WhenTimedOut()
		{
			await this.AssertErrorDetailsAfterOneSecond(
				Never.ExpectWithinSeconds(1).BoxResult(),
				@"\[!\] Never EXPECTED WITHIN.*
\s+Failed with.*
\s+Test operation stack");
		}

		[Test]
		public async Task ExpectCondition_ContainsErrorDetails_WhenExceptionThrown()
		{
			await this.AssertErrorDetailsAfterOneSecond(
				WaitForCondition(
						"Throwing condition",
						() => throw new Exception("Test"))
					.ExpectWithinSeconds(1),
				@"\[!\] Throwing condition EXPECTED WITHIN.*
\s+Failed with.*
\s+Test operation stack");
		}

		[Test]
		public void ExpectConditionDescription_ContainsSubConditions_WithCompoundCondition()
		{
			var description = WaitForCondition("First", () => false)
				.AndThen(WaitForCondition("Second", () => false))
				.ExpectWithinSeconds(1)
				.CreateState()
				.ToString();

			Assert.That(description, Does.Match($@".*1\.00 s.*
\s*\[ \] First.*
\s*\[ \] Second.*"));
		}

		[Test]
		public void ExpectConditionDescription_Inlined_WithDiscreteCondition()
		{
			var description = WaitForCondition("Only", () => false)
				.ExpectWithinSeconds(1)
				.CreateState()
				.ToString();

			Assert.That(
				description,
				Does.Match($@"\s*\[ \] Only.*WITHIN.*1\.00 s"));
		}

		[Test]
		public async Task ExpectResponder_TerminatesWithError_IfWaitNotFulfilled()
		{
			var task = Never
				.ThenRespondWithAction("NOP", Nop)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			Assert.IsFalse(task.IsFaulted);
			this.Scheduler.AdvanceFrame(OneSecond);
			Assert.IsNotNull(await AwaitFailureException(task));
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
			Assert.IsFalse(task.IsFaulted);
			Assert.IsFalse(task.IsCompleted);

			this.Scheduler.AdvanceFrame(OneSecond);
			Assert.IsFalse(task.IsFaulted);
			Assert.IsTrue(task.IsCompleted);
		}

		[Test]
		public async Task ExpectResponder_ContainsErrorDetails_WhenConditionTimedOut()
		{
			var responder = Never.ThenRespondWithAction("Nop", Nop);
			await this.AssertErrorDetailsAfterOneSecond(
				responder.ExpectWithinSeconds(1),
				@"timed out.*
\[!\] Nop CONDITION EXPECTED WITHIN [^!]*
Failed with.*
Test operation stack");
		}

		[Test]
		public async Task ExpectResponder_ContainsErrorDetails_WhenInstructionTimedOut()
		{
			var responder = ImmediateTrue.ThenRespondWith("Response", Never.ExpectWithinSeconds(0.5));
			await this.AssertErrorDetailsAfterOneSecond(
				responder.BoxResult().ExpectWithinSeconds(1),
				@"timed out.*
\[!\] Response CONDITION EXPECTED WITHIN.*
\s+\[!\] Never EXPECTED WITHIN.*
\s+Failed with.*
\s+Test operation stack");
		}

		[Test]
		public async Task ExpectResponder_ContainsErrorDetails_WhenExceptionThrown()
		{
			var responder = ImmediateTrue.ThenRespondWithAction("Throw error", _ => throw new Exception("Test"));
			await this.AssertErrorDetailsAfterOneSecond(
				responder.ExpectWithinSeconds(1),
				@"failed.*
\[!\] Throw error CONDITION EXPECTED WITHIN.*
\s+\[!\] Throw error .*
\s+Failed with.*
\s+Test operation stack");
		}

		private async Task AssertErrorDetailsAfterOneSecond(
			ITestInstruction<object> instruction,
			[RegexPattern] string regex)
		{
			var task = instruction.ToTask(this.Executor);

			this.Scheduler.AdvanceFrame(OneSecond);

			var exception = await AwaitFailureException(task);
			Assert.That(exception.Message, Does.Match($"(?s:{regex})"));

			var multipleFailuresDescription =
				$"Should contain only singe failure details, but was:\n{exception.Message}";

			Assert.AreEqual(
				1,
				Regex.Matches(exception.Message, "Failed with").Count,
				multipleFailuresDescription);

			Assert.AreEqual(
				1,
				Regex.Matches(exception.Message, "Test operation stack").Count,
				multipleFailuresDescription);
			}
	}
}
