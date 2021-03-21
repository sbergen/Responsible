using System;
using NUnit.Framework;
using Responsible.Tests.Runtime.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class AsNothingTests : ResponsibleTestBase
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
		public void UnitTestInstruction_DoesNotThrow_WhenSuccessful()
		{
			Assert.AreEqual(
				Nothing.Default,
				this.returnTrue.AsNothingInstruction().ToTask(this.Executor).AssertSynchronousResult());
		}

		[Test]
		public void UnitTestInstruction_HasError_WhenInstructionHasError()
		{
			var task = this.throwError.AsNothingInstruction().ToTask(this.Executor);
			Assert.NotNull(GetAssertionException(task));
		}

		[Test]
		public void AsNothingInstruction_ReturnsSelf_WhenAlreadyUnit()
		{
			var instruction = Return(Nothing.Default);
			Assert.AreSame(instruction, instruction.AsNothingInstruction());
		}

		[Test]
		public void AsNothingCondition_Completes_WhenCompleted()
		{
			var task = this.waitForComplete
				.AsNothingCondition()
				.ExpectWithinSeconds(10)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsCompleted);

			this.complete = true;
			this.AdvanceDefaultFrame();
			Assert.IsTrue(task.IsCompleted);
		}

		[Test]
		public void AsNothingCondition_PublishesError_WhenTimedOut()
		{
			var task = Never
				.AsNothingCondition()
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsFaulted);

			this.TimeProvider.AdvanceFrame(OneSecond);
			Assert.IsNotNull(GetAssertionException(task));
		}

		[Test]
		public void AsNothingCondition_ReturnsSelf_WhenAlreadyUnit()
		{
			var condition = WaitForCondition("Unit", () => false);
			Assert.AreSame(condition, condition.AsNothingCondition());
		}

		[Test]
		public void UnitTestResponder_Completes_WhenCompleted()
		{
			var task = this.waitForComplete
				.ThenRespondWith("Respond", this.returnTrue)
				.AsNothingResponder()
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsCompleted);

			this.complete = true;
			this.AdvanceDefaultFrame();
			Assert.IsTrue(task.IsCompleted);
		}

		[Test]
		public void UnitTestResponder_PublishesError_OnError()
		{
			var task = this.waitForComplete
				.ThenRespondWith("Respond", this.throwError)
				.AsNothingResponder()
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsFaulted);

			this.complete = true;
			this.AdvanceDefaultFrame();
			Assert.IsNotNull(GetAssertionException(task));
		}

		[Test]
		public void UnitTestResponder_PublishesError_OnTimeout()
		{
			var task = this.waitForComplete
				.ThenRespondWith("Respond", this.returnTrue)
				.AsNothingResponder()
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsFaulted);

			this.TimeProvider.AdvanceFrame(OneSecond);
			Assert.IsNotNull(GetAssertionException(task));
		}

		[Test]
		public void AsNothingResponder_ReturnsSelf_WhenAlreadyUnit()
		{
			var responder = Never.ThenRespondWith("Return Unit", Return(Nothing.Default));
			Assert.AreSame(responder, responder.AsNothingResponder());
		}
	}
}
