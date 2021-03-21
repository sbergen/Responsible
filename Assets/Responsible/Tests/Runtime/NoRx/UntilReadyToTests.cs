using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.NoRx;
using Responsible.Tests.Runtime.NoRx.Utilities;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests.Runtime.NoRx
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
		public void UntilReadyToRespond_DoesNotExecuteFirst_WhenSecondIsFirstToBeReady()
		{
			this.second.MayRespond = true;

			this.AdvanceDefaultFrame();

			this.first.AllowFullCompletion();

			// Adcvnce a few times to be safe
			this.AdvanceDefaultFrame();
			this.AdvanceDefaultFrame();
			Assert.IsFalse(this.first.StartedToRespond, "Second should not have started to respond");
			Assert.IsFalse(this.task.IsCompleted, "Full operation should not be completed before response is complete");

			this.second.AllowFullCompletion();
			this.AdvanceDefaultFrame();
			Assert.IsTrue(this.second.CompletedRespond, "Second should have completed");
			Assert.IsTrue(this.task.IsCompleted, "Full operation should have completed");
		}

		[Test]
		public void UntilReadyToRespond_CompletesResponderExecution_IfStarted()
		{
			this.first.MayRespond = true;
			this.AdvanceDefaultFrame();
			this.second.MayRespond = true;
			this.AdvanceDefaultFrame();

			Assert.IsFalse(
				this.second.StartedToRespond,
				"Second should not have started to respond before first completed");

			this.first.AllowFullCompletion();
			this.AdvanceDefaultFrame();

			Assert.IsTrue(this.first.CompletedRespond, "First should be completed");
			Assert.IsTrue(this.second.StartedToRespond, "Second should have started to respond");

			this.second.AllowFullCompletion();
			this.AdvanceDefaultFrame();
			Assert.IsTrue(this.second.CompletedRespond, "Second should have completed response");
		}

		[Test]
		public void UntilReadyToRespond_PublishesError_FromAnyResponder([Values] bool errorInFirst)
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
			Assert.IsNotNull(GetAssertionException(this.task));
		}

		[Test]
		public void UntilReadyToRespond_TimesOut_AsExpected()
		{
			this.TimeProvider.AdvanceFrame(OneSecond);
			Assert.IsNotNull(GetAssertionException(this.task));
		}
	}
}
