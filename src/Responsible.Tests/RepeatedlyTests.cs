using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class RepeatedlyTests : ResponsibleTestBase
	{
		private bool complete;
		private bool condition;
		private int executionCount;
		private ITestResponder<object> responder;
		private ITestWaitCondition<object> waitForCompletion;

		[SetUp]
		public void SetUp()
		{
			this.complete = false;
			this.condition = false;
			this.executionCount = 0;

			this.responder = Responsibly
				.WaitForCondition(
					"Wait for condition",
					() => this.condition)
				.ThenRespondWithAction(
					"Reset condition and increment count",
					_ =>
					{
						++this.executionCount;
						this.condition = false;
					});

			this.waitForCompletion = WaitForCondition(
				"Wait for completion",
				// ReSharper disable once AccessToModifiedClosure
				() => complete);
		}

		[Test]
		public void RepeatedlyUntil_CompletesSynchronously_WhenUntilConditionAlreadyMet()
		{
			var task = this.responder
				.Repeatedly()
				.Until(ImmediateTrue)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			Assert.IsNull(task.Exception);
			Assert.IsTrue(task.IsCompleted);
			Assert.AreEqual(0, this.executionCount);
		}

		[Test]
		public void RepeatedlyUntil_DoesNotRequireResponderToExecute()
		{
			var task = this.responder
				.Repeatedly()
				.Until(this.waitForCompletion)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.complete = true;
			this.AdvanceDefaultFrame();

			Assert.IsNull(task.Exception);
			Assert.IsTrue(task.IsCompleted);
			Assert.AreEqual(0, this.executionCount);
		}

		[Test]
		public void RepeatedlyUntil_Completes_WhenResponderExecutesOnce()
		{
			var task = this.responder
				.Repeatedly()
				.Until(this.waitForCompletion)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.condition = true;
			this.AdvanceDefaultFrame();
			this.complete = true;
			this.AdvanceDefaultFrame();

			Assert.IsNull(task.Exception);
			Assert.IsTrue(task.IsCompleted);
			Assert.AreEqual(1, this.executionCount);
		}

		[Test]
		public void RepeatedlyUntil_Completes_WhenResponderExecutesTwice()
		{
			var task = this.responder
				.Repeatedly()
				.Until(this.waitForCompletion)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.condition = true;
			this.AdvanceDefaultFrame();
			this.condition = true;
			this.AdvanceDefaultFrame();
			this.complete = true;
			this.AdvanceDefaultFrame();

			Assert.IsNull(task.Exception);
			Assert.IsTrue(task.IsCompleted);
			Assert.AreEqual(2, this.executionCount);
		}

		[Test]
		public void Repeatedly_CanBeCanceled()
		{
			var cts = new CancellationTokenSource();
			var task = this.responder
				.Repeatedly()
				.Until(Never)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor, cts.Token);

			cts.Cancel();

			var error = GetFailureException(task);
			Assert.IsInstanceOf<TaskCanceledException>(error.InnerException);
		}

		[Test]
		public void RepeatedlyStateString_MatchesExpected_WhenNoExecutions()
		{
			var state = this.responder
				.Repeatedly()
				.Until(Never)
				.ExpectWithinSeconds(1)
				.CreateState();

			StateAssert.StringContainsInOrder(state.ToString())
				.Details("REPEATEDLY")
				.Details("never started");
		}

		[Test]
		public void RepeatedlyStateString_MatchesExpected_WhenNoConditionCompletions()
		{
			var state = this.responder
				.Repeatedly()
				.Until(Never)
				.ExpectWithinSeconds(1)
				.CreateState();

			state.ToTask(this.Executor);

			StateAssert.StringContainsInOrder(state.ToString())
				.Details("RESPOND TO REPEATEDLY")
				.Waiting("Wait for condition");
		}

		[Test]
		public void RepeatedlyStateString_MatchesExpected_WhenSomeCompletions()
		{
			var state = this.responder
				.Repeatedly()
				.Until(Never)
				.ExpectWithinSeconds(1)
				.CreateState();

			state.ToTask(this.Executor);

			this.condition = true;
			this.AdvanceDefaultFrame();
			this.condition = true;
			this.AdvanceDefaultFrame();

			StateAssert.StringContainsInOrder(state.ToString())
				.Details("RESPOND TO REPEATEDLY")
				.Completed("Reset condition and increment count")
				.Completed("Reset condition and increment count")
				.Waiting("Wait for condition");
		}
	}
}
