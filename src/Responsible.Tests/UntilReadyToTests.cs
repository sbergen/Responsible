using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Responsible.Tests.Utilities;

// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests
{
	public class UntilReadyToTests : ResponsibleTestBase
	{
		private ConditionResponder<int> first;
		private ConditionResponder<int> second;
		private Task task;

		[SetUp]
		public void SetUp()
		{
			this.first = new ConditionResponder<int>(1, 1);
			this.second = new ConditionResponder<int>(1, 2);

			this.task = this.first.Responder.Optionally()
				.UntilReadyTo(this.second.Responder)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);
		}

		[Test]
		public async Task UntilReadyToRespond_DoesNotExecuteFirst_WhenSecondIsFirstToBeReady()
		{
			this.second.MayRespond = true;

			this.AdvanceDefaultFrame();

			this.first.AllowFullCompletion();

			// Adcvnce a few times to be safe
			this.AdvanceDefaultFrame();
			this.AdvanceDefaultFrame();
			this.first.StartedToRespond.Should()
				.BeFalse("second should not have started to respond");
			this.task.IsCompleted.Should()
				.BeFalse("full operation should not be completed before response is complete");

			this.second.AllowFullCompletion();
			this.AdvanceDefaultFrame();

			await AwaitTaskCompletionForUnity(this.task);

			this.second.CompletedRespond.Should()
				.BeTrue("second should have completed");
			this.task.IsCompleted.Should()
				.BeTrue("full operation should have completed");
		}

		[Test]
		public void UntilReadyToRespond_CompletesResponderExecution_IfStarted()
		{
			this.first.MayRespond = true;
			this.AdvanceDefaultFrame();
			this.second.MayRespond = true;
			this.AdvanceDefaultFrame();

			this.second.StartedToRespond.Should().BeFalse(
				"second should not have started to respond before first completed");

			this.first.AllowFullCompletion();
			this.AdvanceDefaultFrame();

			this.first.CompletedRespond.Should().BeTrue("first should be completed");
			this.second.StartedToRespond.Should().BeTrue("second should have started to respond");

			this.second.AllowFullCompletion();
			this.AdvanceDefaultFrame();
			this.second.CompletedRespond.Should().BeTrue("second should have completed response");
		}

		[Test]
		public async Task UntilReadyToRespond_PublishesError_FromAnyResponder([Values] bool errorInFirst)
		{
			var exception = new Exception("Fail!");
			if (errorInFirst)
			{
				this.first.AllowCompletionWithError(exception);
			}
			else
			{
				this.second.AllowCompletionWithError(exception);
			}

			this.AdvanceDefaultFrame();
			(await AwaitFailureExceptionForUnity(this.task)).Should().NotBeNull();
		}

		[Test]
		public async Task UntilReadyToRespond_TimesOut_AsExpected()
		{
			this.Scheduler.AdvanceFrame(OneSecond);
			(await AwaitFailureExceptionForUnity(this.task)).Should().NotBeNull();
		}

		[Test]
		public void UntilReadyToRespond_Description_MatchesExpected()
		{
			var state = this.first.Responder.Optionally()
				.UntilReadyTo(this.second.Responder)
				.CreateState();

			StateAssert.StringContainsInOrder(state.ToString())
				.Details("UNTIL READY TO")
				.NotStarted("Respond")
				.Details("RESPOND TO ANY OF")
				.NotStarted("Respond");
		}
	}
}
