using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class ContinueWithTests : ResponsibleTestBase
	{
		private bool mayCompleteFirst;
		private bool mayCompleteSecond;

		private ITestWaitCondition<object> waitForFirst;
		private ITestWaitCondition<object> waitForSecond;

		private static readonly Exception TestException = new Exception("Test");
		private static readonly ITestInstruction<object> ErrorInstruction =
			Do("throw", () => throw TestException);

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
		public async Task ContinueWith_PropagatesErrorFromFirst(
			[Values] bool shouldCompleteSecond,
			[Values] Strategy strategy)
		{
			this.mayCompleteSecond = shouldCompleteSecond;

			var task = this.ContinueWithNothing(ErrorInstruction, strategy)
				.ToTask(this.Executor);

			Assert.IsNotNull(await AwaitFailureException(task));
		}

		[Test]
		public async Task ContinueWith_PropagatesError_WhenContinuationThrows()
		{
			var task = Responsibly
				.Return(new object())
				.ContinueWith<object, object>(_ => throw new Exception("Test exception"))
				.ToTask(this.Executor);

			var exception = await AwaitFailureException(task);
			StateAssert.StringContainsInOrder(exception.Message)
				.Completed("Return")
				.Failed("...")
				.Details("Test exception");
		}

		[Test]
		public async Task ContinueWith_PropagatesErrorFromSecond_AfterFirstCompleted(
			[Values] Strategy strategy)
		{
			var task = this.ContinueWithError(this.waitForFirst.ExpectWithinSeconds(1), strategy)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsFaulted);

			this.mayCompleteFirst = true;
			this.AdvanceDefaultFrame();
			Assert.AreSame(TestException, (await AwaitFailureException(task)).InnerException);
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

		[Test]
		public void Description_MatchesExpected_WithInstruction()
		{
			var state = Do("First", () => { })
				.ContinueWith(Do("Second", () => { }))
				.CreateState();

			StateAssert.StringContainsInOrder(state.ToString())
				.NotStarted("First")
				.NotStarted("Second");
		}

		[Test]
		public void Description_MatchesExpected_WithContinuation()
		{
			var state = Do("First", () => { })
				.ContinueWith(_ => Do("Second", () => { }))
				.CreateState();

			StateAssert.StringContainsInOrder(state.ToString())
				.NotStarted("First")
				.NotStarted("...");
		}

		private ITestInstruction<object> ContinueWithNothing<T>(
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

		private ITestInstruction<object> ContinueWithError<T>(
			ITestInstruction<T> first,
			Strategy strategy)
		{
			switch (strategy)
			{
				case Strategy.Continuation:
					return first.ContinueWith(_ => ErrorInstruction);
				case Strategy.Instruction:
					return first.ContinueWith(ErrorInstruction);
				default:
					throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unhandled strategy");
			}
		}
	}
}
