using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class RepeatUntilTests : ResponsibleTestBase
	{
		[Test]
		public void RepeatUntil_ShouldRepeat_UntilConditionIsFilled()
		{
			var executionCount = 0;
			var task = Responsibly
				.Do("increment execution count", () => ++executionCount)
				.ContinueWith(WaitForFrames(0))
				.RepeatUntil(
					WaitForCondition("execution count to be 3", () => executionCount == 3),
					100)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsCompleted, "condition should not yet be completed");

			this.AdvanceDefaultFrame();
			Assert.IsTrue(task.IsCompleted, "condition should be completed");
		}

		[Test]
		public void RepeatUntil_DoesNotExecuteInstruction_IfConditionIsAlreadyFilled()
		{
			var executionCount = 0;
			var task = Responsibly
				.Do("increment execution count", () => ++executionCount)
				.RepeatUntil(ImmediateTrue, 100)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			Assert.IsTrue(task.IsCompleted, "condition should be immediately");
			Assert.AreEqual(0, executionCount, "no instruction executions should happen");
		}

		[Test]
		public async Task Repeatedly_CanBeCanceled()
		{
			var cts = new CancellationTokenSource();
			var task = Do("nothing", () => {})
				.ContinueWith(WaitForFrames(0))
				.RepeatUntil(Never, 100)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor, cts.Token);

			cts.Cancel();

			var error = await AwaitFailureExceptionForUnity(task);
			Assert.IsInstanceOf<TaskCanceledException>(error.InnerException);
		}

		[Test]
		public async Task ReachingMaximumRepeatCount_CausesFailure()
		{
			var executionCount = 0;
			var task = Responsibly
				.Do("increment execution count", () => ++executionCount)
				.RepeatUntil(Never, 3)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			var error = await AwaitFailureExceptionForUnity(task);
			Assert.AreEqual(3, executionCount);
			Assert.IsInstanceOf<RepetitionLimitExceededException>(error.InnerException);
			// ReSharper disable once PossibleNullReferenceException, checked above
			StringAssert.Contains("3", error.InnerException.Message);
		}

		[Test]
		public void RepeatUntilStateString_MatchesExpected_WhenNoExecutions()
		{
			var state = Do("nothing", () => {})
				.RepeatUntil(Never, 1)
				.ExpectWithinSeconds(1)
				.CreateState();

			StateAssert.StringContainsInOrder(state.ToString())
				.Details("UNTIL")
				.NotStarted("Never")
				.Details("REPEATEDLY EXECUTING")
				.Details("...");
		}

		[Test]
		public void RepeatUntilStateString_MatchesExpected_WhenSomeExecutions()
		{
			var state = Do("do nothing", () => {})
				.ContinueWith(WaitForFrames(0))
				.RepeatUntil(Never, 10)
				.ExpectWithinSeconds(1)
				.CreateState();

			// Execute first instruction
			state.ToTask(this.Executor);

			StateAssert.StringContainsInOrder(state.ToString())
				.Details("UNTIL")
				.Waiting("Never")
				.Details("REPEATEDLY EXECUTING")
				.Completed("do nothing")
				.Details("...");
		}

		[Test]
		public void RepeatUntilStateString_MatchesExpected_WhenCompleted()
		{
			var executionCount = 0;
			var state = Do("increment count", () => ++executionCount)
				.ContinueWith(WaitForFrames(0))
				.RepeatUntil(WaitForCondition("one execution", () => executionCount == 1), 10)
				.ExpectWithinSeconds(1)
				.CreateState();

			// Execute the whole thing
			state.ToTask(this.Executor);
			this.AdvanceDefaultFrame();

			var stateString = state.ToString();
			StateAssert.StringContainsInOrder(stateString)
				.Details("UNTIL")
				.Completed("one execution")
				.Details("REPEATEDLY EXECUTING")
				.Completed("increment count");

			StringAssert.DoesNotContain("...", stateString);
		}
	}
}