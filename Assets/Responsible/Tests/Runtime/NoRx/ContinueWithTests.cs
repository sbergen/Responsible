using System;
using System.Collections;
using NUnit.Framework;
using Responsible.NoRx;
using static Responsible.NoRx.Responsibly;

namespace Responsible.Tests.Runtime.NoRx
{
	public class ContinueWithTests : ResponsibleTestBase
	{
		private bool mayCompleteFirst;
		private bool mayCompleteSecond;

		private ITestWaitCondition<Nothing> waitForFirst;
		private ITestWaitCondition<Nothing> waitForSecond;

		private ITestInstruction<Nothing> errorInstruction = Do("throw", () => { throw new Exception("Test"); });

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

			var task = ContinueWithNothing(this.errorInstruction, strategy)
				.ToTask(this.Executor);

			Assert.IsNotNull(GetAssertionException(task));
		}

		[Test]
		public void ContinueWith_PropagatesErrorFromSecond_AfterFirstCompleted(
			[Values] Strategy strategy)
		{
			var task = this.ContinueWithError(this.waitForFirst.ExpectWithinSeconds(1), strategy)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsFaulted);

			this.mayCompleteFirst = true;
			this.AdvanceDefaultFrame();
			Assert.IsNotNull(GetAssertionException(task));
		}

		[Test]
		public void ContinueWith_Completes_AfterBothComplete([Values] Strategy strategy)
		{
			var task = this.ContinueWithNothing(this.waitForFirst.ExpectWithinSeconds(1), strategy)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsCompleted);

			this.mayCompleteFirst = true;
			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsCompleted);

			this.mayCompleteSecond = true;
			this.AdvanceDefaultFrame();
			Assert.IsTrue(task.IsCompleted);
		}

		private ITestInstruction<Nothing> ContinueWithNothing<T>(
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

		private ITestInstruction<Nothing> ContinueWithError<T>(
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
