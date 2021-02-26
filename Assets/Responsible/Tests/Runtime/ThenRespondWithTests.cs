using System;
using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests.Runtime
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

		[UnityTest]
		public IEnumerator ThenRespondWith_WaitsForCondition([Values] ConstructionStrategy strategy)
		{
			var conditionFulfilled = false;
			var condition = WaitForCondition(
				"Ready to execute",
				() => conditionFulfilled);
			var completed = false;

			MakeUnitResponder(strategy, condition)
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(_ => completed = true);

			yield return null;
			Assert.IsFalse(completed);

			conditionFulfilled = true;
			yield return null;
			Assert.IsTrue(completed);
		}

		[Test]
		public void ThenRespondWith_PropagatesErrorFromWait([Values] ConstructionStrategy strategy)
		{
			var condition = WaitForCondition(
				"Ready to execute",
				() => throw new Exception("Test"));

			MakeUnitResponder(strategy, condition)
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			Assert.IsInstanceOf<AssertionException>(this.Error);
		}

		[Test]
		public void ThenRespondWith_PropagatesErrorFromInstruction([Values] ConstructionStrategy strategy)
		{
			MakeErrorResponder(strategy, ImmediateTrue)
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			Assert.IsInstanceOf<AssertionException>(this.Error);
		}

		private static ITestResponder<Unit> MakeUnitResponder<TWait>(
			ConstructionStrategy strategy,
			ITestWaitCondition<TWait> waitCondition)
		{
			switch (strategy)
			{
				case ConstructionStrategy.Continuation:
					return waitCondition.ThenRespondWith("Respond", x => Return(Unit.Default));
				case ConstructionStrategy.Instruction:
					return waitCondition.ThenRespondWith("Respond", Return(Unit.Default));
				case ConstructionStrategy.Func:
					return waitCondition.ThenRespondWithFunc("Respond", _ => Unit.Default);
				case ConstructionStrategy.Action:
					return waitCondition.ThenRespondWithAction("Respond", _ => { });
				default:
					throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unhandled strategy");
			}
		}

		private static ITestResponder<Unit> MakeErrorResponder<TWait>(
			ConstructionStrategy strategy,
			ITestWaitCondition<TWait> waitCondition)
		{
			Unit ThrowException(TWait _) => throw new Exception("Test");
			ITestInstruction<Unit> throwInstruction = DoAndReturn("Throw", () => ThrowException(default));
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