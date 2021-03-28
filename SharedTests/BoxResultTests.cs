using System;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class BoxResultTests : ResponsibleTestBase
	{
		private bool complete;

		private ITestWaitCondition<bool> waitForComplete;
		private ITestInstruction<bool> returnTrue;
		private ITestInstruction<int> throwError;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			this.waitForComplete = WaitForConditionOn("Wait", () => this.complete, val => val);
			this.returnTrue = DoAndReturn("Set completed", () => true);
			this.throwError = DoAndReturn<int>("Throw error", () => throw new Exception(""));
		}

		[SetUp]
		public void SetUp()
		{
			this.complete = false;
		}

		[Test]
		public void BoxedTestInstruction_DoesNotThrow_WhenSuccessful()
		{
			Assert.AreEqual(
				true,
				this.returnTrue.BoxResult().ToTask(this.Executor).AssertSynchronousResult());
		}

		[Test]
		public void BoxedTestInstruction_HasError_WhenInstructionHasError()
		{
			var task = this.throwError.BoxResult().ToTask(this.Executor);
			Assert.NotNull(GetFailureException(task));
		}

		[Test]
		public void BoxedCondition_Completes_WhenCompleted()
		{
			var task = this.waitForComplete
				.BoxResult()
				.ExpectWithinSeconds(10)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsCompleted);

			this.complete = true;
			this.AdvanceDefaultFrame();
			Assert.IsTrue(task.IsCompleted);
		}

		[Test]
		public void BoxedCondition_PublishesError_WhenTimedOut()
		{
			var task = Never
				.BoxResult()
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsFaulted);

			this.Scheduler.AdvanceFrame(OneSecond);
			Assert.IsNotNull(GetFailureException(task));
		}

		[Test]
		public void BoxedTestResponder_Completes_WhenCompleted()
		{
			var task = this.waitForComplete
				.ThenRespondWith("Respond", this.returnTrue)
				.BoxResult()
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsCompleted);

			this.complete = true;
			this.AdvanceDefaultFrame();
			Assert.IsTrue(task.IsCompleted);
		}

		[Test]
		public void BoxedTestResponder_PublishesError_OnError()
		{
			var task = this.waitForComplete
				.ThenRespondWith("Respond", this.throwError)
				.BoxResult()
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsFaulted);

			this.complete = true;
			this.AdvanceDefaultFrame();
			Assert.IsNotNull(GetFailureException(task));
		}

		[Test]
		public void BoxedTestResponder_PublishesError_OnTimeout()
		{
			var task = this.waitForComplete
				.ThenRespondWith("Respond", this.returnTrue)
				.BoxResult()
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsFaulted);

			this.Scheduler.AdvanceFrame(OneSecond);
			Assert.IsNotNull(GetFailureException(task));
		}
	}
}
