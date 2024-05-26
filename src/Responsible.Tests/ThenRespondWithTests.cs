using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests
{
	public class ThenRespondWithTests : ResponsibleTestBase
	{
		public enum ConstructionStrategy
		{
			Continuation,
			Instruction,
			Func,
			Action
		}

		[Test]
		public void ThenRespondWith_WaitsForCondition([Values] ConstructionStrategy strategy)
		{
			var conditionFulfilled = false;
			var condition = WaitForCondition(
				"Ready to execute",
				() => conditionFulfilled);

			var task = MakeObjectResponder(strategy, condition)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			task.IsCompleted.Should().BeFalse();

			conditionFulfilled = true;
			this.AdvanceDefaultFrame();
			task.IsCompleted.Should().BeTrue();
		}

		[Test]
		public async Task ThenRespondWith_PropagatesErrorFromWait([Values] ConstructionStrategy strategy)
		{
			var condition = WaitForCondition(
				"Ready to execute",
				() => throw new Exception("Test"));

			var task = MakeObjectResponder(strategy, condition)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			(await AwaitFailureExceptionForUnity(task)).Should().NotBeNull();
		}

		[Test]
		public async Task ThenRespondWith_PropagatesErrorFromInstruction([Values] ConstructionStrategy strategy)
		{
			var task = MakeErrorResponder(strategy, ImmediateTrue)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			(await AwaitFailureExceptionForUnity(task)).Should().NotBeNull();
		}

		[Test]
		public async Task Status_IsWaiting_WhenWaitHasCompletedButInstructionNotStarted()
		{
			var state = ImmediateTrue
				.ThenRespondWith("Responder", _ => WaitForSeconds(1))
				.CreateState();

			// This is lower-level than I'd want,
			// but still the most direct way to start the instruction.
			var instructionState = await state.ToTask(this.Executor);
			_ = instructionState.ToTask(this.Executor); // Start, but don't wait

			StateAssert.StringContainsInOrder(state.ToString())
				.Waiting("Responder")
				.Completed("True")
				.Waiting("WAIT FOR");
		}

		[Test]
		public void Status_IsWaiting_WhenWaitHasNotCompleted()
		{
			var state = Never
				.ThenRespondWith(
					"Responder",
					_ => Do("Specific failure", () => throw new Exception()))
				.CreateState();

			state.ToTask(this.Executor); // Start execution

			var stateString = state.ToString();
			StateAssert.StringContainsInOrder(stateString)
				.Waiting("Responder")
				.Waiting("Never");

			stateString.Should().NotContain("Specific failure");
		}

		private static ITestResponder<object> MakeObjectResponder<TWait>(
			ConstructionStrategy strategy,
			ITestWaitCondition<TWait> waitCondition)
		{
			var obj = new object();

			switch (strategy)
			{
				case ConstructionStrategy.Continuation:
					return waitCondition.ThenRespondWith("Respond", _ => Return(obj));
				case ConstructionStrategy.Instruction:
					return waitCondition.ThenRespondWith("Respond", Return(obj));
				case ConstructionStrategy.Func:
					return waitCondition.ThenRespondWithFunc("Respond", _ => obj);
				case ConstructionStrategy.Action:
					return waitCondition.ThenRespondWithAction("Respond", _ => { });
				default:
					throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unhandled strategy");
			}
		}

		private static ITestResponder<object> MakeErrorResponder<TWait>(
			ConstructionStrategy strategy,
			ITestWaitCondition<TWait> waitCondition)
		{
			object ThrowException(TWait _) => throw new Exception("Test");
			ITestInstruction<object> throwInstruction = DoAndReturn("Throw", () => ThrowException(default));
			switch (strategy)
			{
				case ConstructionStrategy.Continuation:
					return waitCondition.ThenRespondWith("Respond", _ => throwInstruction);
				case ConstructionStrategy.Instruction:
					return waitCondition.ThenRespondWith("Respond", throwInstruction);
				case ConstructionStrategy.Func:
					return waitCondition.ThenRespondWithFunc("Respond", ThrowException);
				case ConstructionStrategy.Action:
					return waitCondition.ThenRespondWithAction("Respond", _ => { ThrowException(_); });
				default:
					throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unhandled strategy");
			}
		}
	}
}
