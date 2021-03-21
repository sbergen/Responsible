using System;
using System.Collections;
using NUnit.Framework;
using Responsible.NoRx;
using UnityEngine.TestTools;
using static Responsible.NoRx.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests.Runtime.NoRx
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

			var task = MakeBoolResponder(strategy, condition)
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

			var task = MakeBoolResponder(strategy, condition)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			Assert.NotNull(GetAssertionException(task));
		}

		[Test]
		public void ThenRespondWith_PropagatesErrorFromInstruction([Values] ConstructionStrategy strategy)
		{
			var task = MakeErrorResponder(strategy, ImmediateTrue)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			Assert.NotNull(GetAssertionException(task));
		}

		private static ITestResponder<bool> MakeBoolResponder<TWait>(
			ConstructionStrategy strategy,
			ITestWaitCondition<TWait> waitCondition)
		{
			switch (strategy)
			{
				case ConstructionStrategy.Continuation:
					return waitCondition.ThenRespondWith("Respond", x => Return(true));
				case ConstructionStrategy.Instruction:
					return waitCondition.ThenRespondWith("Respond", Return(true));
				case ConstructionStrategy.Func:
					return waitCondition.ThenRespondWithFunc("Respond", _ => true);
				case ConstructionStrategy.Action:
					return waitCondition.ThenRespondWithAction("Respond", _ => { });
				default:
					throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unhandled strategy");
			}
		}

		private static ITestResponder<bool> MakeErrorResponder<TWait>(
			ConstructionStrategy strategy,
			ITestWaitCondition<TWait> waitCondition)
		{
			bool ThrowException(TWait _) => throw new Exception("Test");
			ITestInstruction<bool> throwInstruction = DoAndReturn("Throw", () => ThrowException(default));
			switch (strategy)
			{
				case ConstructionStrategy.Continuation:
					return waitCondition.ThenRespondWith("Respond", x => throwInstruction);
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
