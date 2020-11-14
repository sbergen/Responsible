using System;
using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class ContinueWithTests : ResponsibleTestBase
	{
		private bool mayCompleteFirst;
		private bool mayCompleteSecond;

		private ITestWaitCondition<Unit> waitForFirst;
		private ITestWaitCondition<Unit> waitForSecond;

		private ITestInstruction<Unit> errorInstruction = Do("throw", () => { throw new Exception("Test"); });

		public enum Strategy
		{
			Continuation,
			Instruction,
		}

		[SetUp]
		public void SetUp()
		{
			this.mayCompleteFirst = false;
			this.mayCompleteSecond = false;

			this.waitForFirst = WaitForCondition("first", () => this.mayCompleteFirst);
			this.waitForSecond = WaitForCondition("second", () => this.mayCompleteSecond);
		}

		[Test]
		public void ContinueWith_PropagatesErrorFromFirst(
			[Values] bool shouldCompleteSecond,
			[Values] Strategy strategy)
		{
			this.mayCompleteSecond = shouldCompleteSecond;

			ContinueWithUnit(this.errorInstruction, strategy)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			Assert.IsInstanceOf<AssertionException>(this.Error);
		}

		[UnityTest]
		public IEnumerator ContinueWith_PropagatesErrorFromSecond_AfterFirstCompleted(
			[Values] Strategy strategy)
		{
			this.ContinueWithError(this.waitForFirst.ExpectWithinSeconds(1), strategy)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			yield return null;
			Assert.IsNull(this.Error);

			this.mayCompleteFirst = true;
			yield return null;
			Assert.IsInstanceOf<AssertionException>(this.Error);
		}

		[UnityTest]
		public IEnumerator ContinueWith_Completes_AfterBothComplete([Values] Strategy strategy)
		{
			var completed = false;
			this.ContinueWithError(this.waitForFirst.ExpectWithinSeconds(1), strategy)
				.ToObservable(this.Executor)
				.Subscribe(_ => completed = true);

			yield return null;
			Assert.IsFalse(completed);

			this.mayCompleteFirst = true;
			yield return null;
			Assert.IsFalse(completed);

			this.mayCompleteSecond = true;
			yield return null;
			Assert.IsTrue(completed);
		}

		private ITestInstruction<Unit> ContinueWithUnit<T>(
			ITestInstruction<T> first,
			Strategy strategy)
		{
			var second = this.waitForSecond.ExpectWithinSeconds(1);
			switch (strategy)
			{
				case Strategy.Continuation:
					return first.ContinueWith(_ => second);
				case Strategy.Instruction:
					return first.ContinueWith(second);
				default:
					throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unhandled strategy");
			}
		}

		private ITestInstruction<Unit> ContinueWithError<T>(
			ITestInstruction<T> first,
			Strategy strategy)
		{
			switch (strategy)
			{
				case Strategy.Continuation:
					return first.ContinueWith(_ => this.errorInstruction);
				case Strategy.Instruction:
					return first.ContinueWith(this.errorInstruction);
				default:
					throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unhandled strategy");
			}
		}
	}
}