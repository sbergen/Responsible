using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using NUnit.Framework;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class ExpectWithinSecondsTests : ResponsibleTestBase
	{
		[Test]
		public void ExpectCondition_TerminatesWithError_AfterTimeout()
		{
			var task = Never
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			Assert.IsFalse(task.IsFaulted);
			this.TimeProvider.AdvanceFrame(OneSecond);
			Assert.IsNotNull(GetFailureException(task));
		}

		[Test]
		public void ExpectCondition_ContainsErrorDetails_WhenTimedOut()
		{
			this.AssertErrorDetailsAfterOneSecond(
				Never.ExpectWithinSeconds(1).BoxResult(),
				@"\[!\] Never EXPECTED WITHIN.*
\s+Failed with.*
\s+Test operation stack");
		}

		[Test]
		public void ExpectCondition_ContainsErrorDetails_WhenExceptionThrown()
		{
			this.AssertErrorDetailsAfterOneSecond(
				WaitForCondition(
						"Throwing condition",
						() => throw new Exception("Test"))
					.ExpectWithinSeconds(1),
				@"\[!\] Throwing condition EXPECTED WITHIN.*
\s+Failed with.*
\s+Test operation stack");
		}

		[TestCase(1, @"1\.00 s")]
		[TestCase(61, @"0:01:01")]
		public void ExpectConditionDescription_ContainsSubConditions_WithCompoundCondition(
			double withinSeconds, string expectedTimeFormat)
		{
			var description = WaitForCondition("First", () => false)
				.AndThen(WaitForCondition("Second", () => false))
				.ExpectWithinSeconds(withinSeconds)
				.CreateState()
				.ToString();

			Assert.That(description, Does.Match($@".*{expectedTimeFormat}.*
\s*\[ \] First.*
\s*\[ \] Second.*"));
		}

		[TestCase(59, @"59\.00 s")]
		[TestCase(3670, @"1:01:10")]
		public void ExpectConditionDescription_Inlined_WithDiscreteCondition(
			double withinSeconds, string expectedTimeFormat)
		{
			var description = WaitForCondition("Only", () => false)
				.ExpectWithinSeconds(withinSeconds)
				.CreateState()
				.ToString();

			Assert.That(
				description,
				Does.Match($@"\s*\[ \] Only.*WITHIN.*{expectedTimeFormat}"));
		}

		[Test]
		public void ExpectResponder_TerminatesWithError_IfWaitNotFulfilled()
		{
			var task = Never
				.ThenRespondWithAction("NOP", Nop)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			Assert.IsFalse(task.IsFaulted);
			this.TimeProvider.AdvanceFrame(OneSecond);
			Assert.IsNotNull(GetFailureException(task));
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

			this.TimeProvider.AdvanceFrame(OneSecond);
			Assert.IsFalse(task.IsFaulted);
			Assert.IsFalse(task.IsCompleted);

			this.TimeProvider.AdvanceFrame(OneSecond);
			Assert.IsFalse(task.IsFaulted);
			Assert.IsTrue(task.IsCompleted);
		}

		[Test]
		public void ExpectResponder_ContainsErrorDetails_WhenConditionTimedOut()
		{
			var responder = Never.ThenRespondWithAction("Nop", Nop);
			this.AssertErrorDetailsAfterOneSecond(
				responder.ExpectWithinSeconds(1),
				@"timed out.*
\[!\] Nop EXPECTED WITHIN [^!]*
Failed with.*
Test operation stack");
		}

		[Test]
		public void ExpectResponder_ContainsErrorDetails_WhenInstructionTimedOut()
		{
			var responder = ImmediateTrue.ThenRespondWith("Response", Never.ExpectWithinSeconds(0.5));
			this.AssertErrorDetailsAfterOneSecond(
				responder.BoxResult().ExpectWithinSeconds(1),
				@"timed out.*
\[!\] Response EXPECTED WITHIN.*
\s+\[!\] Never EXPECTED WITHIN.*
\s+Failed with.*
\s+Test operation stack");
		}

		[Test]
		public void ExpectResponder_ContainsErrorDetails_WhenExceptionThrown()
		{
			var responder = ImmediateTrue.ThenRespondWithAction("Throw error", _ => throw new Exception("Test"));
			this.AssertErrorDetailsAfterOneSecond(
				responder.ExpectWithinSeconds(1),
				@"failed.*
\[!\] Throw error EXPECTED WITHIN.*
\s+\[!\] Throw error .*
\s+Failed with.*
\s+Test operation stack");
		}

		private void AssertErrorDetailsAfterOneSecond(
			ITestInstruction<object> instruction,
			[RegexPattern] string regex)
		{
			var task = instruction.ToTask(this.Executor);

			this.TimeProvider.AdvanceFrame(OneSecond);

			var exception = GetFailureException(task);
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
