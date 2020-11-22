using System;
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
			this.AssertErrorDetailsAfterOneSecond(Never.ExpectWithinSeconds(1));
		}

		[Test]
		public void ExpectCondition_ContainsErrorDetails_WhenExceptionThrown()
		{
			this.AssertErrorDetailsAfterOneSecond(
				WaitForCondition(
						"Throwing condition",
						() => throw new Exception("Test"))
					.ExpectWithinSeconds(1));
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
				.ThenRespondWith("NOP", Nop)
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
		public void ExpectResponder_ContainsErrorDetails_WhenTimedOut()
		{
			var responder = Never.ThenRespondWith("NOP", Nop);
			this.AssertErrorDetailsAfterOneSecond(responder.ExpectWithinSeconds(1));
		}

		[Test]
		public void ExpectResponder_ContainsErrorDetails_WhenExceptionThrown()
		{
			var responder = ImmediateTrue.ThenRespondWith("Throw error", _ => throw new Exception("Test"));
			this.AssertErrorDetailsAfterOneSecond(responder.ExpectWithinSeconds(1));
		}

		private void AssertErrorDetailsAfterOneSecond(ITestInstruction<Unit> instruction)
		{
			instruction
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			this.Scheduler.AdvanceBy(OneSecond);

			Assert.IsInstanceOf<AssertionException>(this.Error);
			StringAssert.Contains("[!]", this.Error.Message);
			StringAssert.Contains("Test operation stack", this.Error.Message);
			StringAssert.Contains("Failed with", this.Error.Message);
		}
	}
}