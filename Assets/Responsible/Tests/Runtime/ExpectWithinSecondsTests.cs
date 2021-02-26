using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using NUnit.Framework;
using UniRx;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class ExpectWithinSecondsTests : ResponsibleTestBase
	{
		[Test]
		public void ExpectCondition_TerminatesWithError_AfterTimeout()
		{
			Never
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			Assert.IsNull(this.Error);
			this.Scheduler.AdvanceBy(OneSecond);
			Assert.IsInstanceOf<AssertionException>(this.Error);
		}

		[Test]
		public void ExpectCondition_ContainsErrorDetails_WhenTimedOut()
		{
			this.AssertErrorDetailsAfterOneSecond(
				Never.ExpectWithinSeconds(1),
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
			Never
				.ThenRespondWithAction("NOP", Nop)
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			Assert.IsNull(this.Error);
			this.Scheduler.AdvanceBy(OneSecond);
			Assert.IsInstanceOf<AssertionException>(this.Error);
		}

		[Test]
		public void ExpectResponder_DoesNotTerminateWithTimeout_IfWaitFulfilled()
		{
			var completed = false;

			// The instruction takes longer than the timeout
			// => The timeout applies only to the wait!
			ImmediateTrue
				.ThenRespondWith("Wait for two seconds", WaitForSeconds(2))
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(_ => completed = true, this.StoreError);

			this.Scheduler.AdvanceBy(OneSecond);
			Assert.IsNull(this.Error);
			Assert.IsFalse(completed);

			this.Scheduler.AdvanceBy(OneSecond);
			Assert.IsNull(this.Error);
			Assert.IsTrue(completed);
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
				responder.ExpectWithinSeconds(1),
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
			ITestInstruction<Unit> instruction,
			[RegexPattern] string regex)
		{
			using (instruction
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError))
			{
				this.Scheduler.AdvanceBy(OneSecond);

				Assert.IsInstanceOf<AssertionException>(this.Error);
				Assert.That(this.Error.Message, Does.Match($"(?s:{regex})"));

				var multipleFailuresDescription =
					$"Should contain only singe failure details, but was:\n{this.Error.Message}";

				Assert.AreEqual(
					1,
					Regex.Matches(this.Error.Message, "Failed with").Count,
					multipleFailuresDescription);

				Assert.AreEqual(
					1,
					Regex.Matches(this.Error.Message, "Test operation stack").Count,
					multipleFailuresDescription);
			}
		}
	}
}