using System;
using NUnit.Framework;
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
			Assert.IsFalse(task.IsCompleted);

			conditionFulfilled = true;
			this.AdvanceDefaultFrame();
			Assert.IsTrue(task.IsCompleted);
		}

		[Test]
		public void ThenRespondWith_PropagatesErrorFromWait([Values] ConstructionStrategy strategy)
		{
			var condition = WaitForCondition(
				"Ready to execute",
				() => throw new Exception("Test"));

			var task = MakeObjectResponder(strategy, condition)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			Assert.NotNull(GetFailureException(task));
		}

		[Test]
		public void ThenRespondWith_PropagatesErrorFromInstruction([Values] ConstructionStrategy strategy)
		{
			var task = MakeErrorResponder(strategy, ImmediateTrue)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			Assert.NotNull(GetFailureException(task));
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
