using System.Diagnostics.CodeAnalysis;
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
		public async Task RepeatedlyUntil_CompletesSynchronously_WhenUntilConditionAlreadyMet()
		{
			await this.responder
				.Repeatedly(1)
				.Until(ImmediateTrue)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			Assert.AreEqual(0, this.executionCount);
		}

		[Test]
		public async Task RepeatedlyUntil_DoesNotRequireResponderToExecute()
		{
			var task = this.responder
				.Repeatedly(1)
				.Until(this.waitForCompletion)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.complete = true;
			this.AdvanceDefaultFrame();

			await task;
			Assert.AreEqual(0, this.executionCount);
		}

		[Test]
		public async Task RepeatedlyUntil_Completes_WhenResponderExecutesOnce()
		{
			var task = this.responder
				.Repeatedly(1)
				.Until(this.waitForCompletion)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.condition = true;
			this.AdvanceDefaultFrame();
			this.complete = true;
			this.AdvanceDefaultFrame();

			await task;
			Assert.AreEqual(1, this.executionCount);
		}

		[Test]
		public async Task RepeatedlyUntil_Completes_WhenResponderExecutesTwice()
		{
			var task = this.responder
				.Repeatedly(2)
				.Until(this.waitForCompletion)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.condition = true;
			this.AdvanceDefaultFrame();
			this.condition = true;
			this.AdvanceDefaultFrame();
			this.complete = true;
			this.AdvanceDefaultFrame();

			await task;
			Assert.AreEqual(2, this.executionCount);
		}

		[Test]
		[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
		public async Task MultipleRepeatedlyResponders_FunctionAsExpected()
		{
			bool? ping = null;
			var count = 0;

			var incrementCount = Do(
				"Increment count and reset",
				() =>
				{
					++count;
					ping = null;
				});

			var countIsFive = WaitForCondition("Count is five", () => count == 5);

			var task = Responsibly
				.WaitForAllOf(
					WaitForCondition("Ping", () => ping == true)
						.ThenRespondWith("Ping the count", incrementCount)
						.Repeatedly(3)
						.Until(countIsFive),
					WaitForCondition("Pong", () => ping == false)
						.ThenRespondWith("Pong the count", incrementCount)
						.Repeatedly(3)
						.Until(countIsFive))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			for (var i = 0; i < 10; i++)
			{
				ping = i % 2 == 0;
				this.AdvanceDefaultFrame();
			}

			await task;
			Assert.AreEqual(5, count);
		}

		[Test]
		public async Task Repeatedly_CanBeCanceled()
		{
			var cts = new CancellationTokenSource();
			var task = this.responder
				.Repeatedly(1)
				.Until(Never)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor, cts.Token);

			cts.Cancel();

			var error = await AwaitFailureExceptionForUnity(task);
			Assert.IsInstanceOf<TaskCanceledException>(error.InnerException);
		}

		[Test]
		public async Task Repeatedly_Fails_WhenMaximumCountReached()
		{
			var execCount = 0;
			var task = ImmediateTrue
				.ThenRespondWithAction("Increment execution count", _ => ++execCount)
				.Repeatedly(10)
				.Until(Never)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			Assert.AreEqual(10, execCount);

			var error = await AwaitFailureExceptionForUnity(task);
			Assert.IsInstanceOf<RepetitionLimitExceededException>(error.InnerException);
			// ReSharper disable once PossibleNullReferenceException, checked above
			StringAssert.Contains("10", error.InnerException.Message);
		}

		[Test]
		public void RepeatedlyStateString_MatchesExpected_WhenNoExecutions()
		{
			var state = this.responder
				.Repeatedly(0)
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
				.Repeatedly(1)
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
				.Repeatedly(2)
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
