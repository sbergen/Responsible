using System;
using System.Collections;
using NUnit.Framework;
using Responsible.Tests.Runtime.Utilities;
using UniRx;
using UnityEngine.TestTools;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests.Runtime
{
	public class UntilReadyToTests : ResponsibleTestBase
	{
		private ConditionResponder<int> first;
		private ConditionResponder<int> second;
		private bool completed;

		[SetUp]
		public void SetUp()
		{
			this.first = new ConditionResponder<int>(1, 1);
			this.second = new ConditionResponder<int>(1, 2);
			this.completed = false;

			this.first.Responder.Optionally()
				.UntilReadyTo(this.second.Responder)
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(_ => this.completed = true, this.StoreError);
		}

		[UnityTest]
		public IEnumerator UntilReadyToRespond_DoesNotExecuteFirst_WhenSecondIsFirstToBeReady()
		{
			this.second.MayRespond = true;

			yield return null;

			this.first.AllowFullCompletion();

			// Yield a few times to be safe
			yield return null;
			yield return null;

			Assert.IsFalse(this.first.StartedToRespond, "Second should not have started to respond");
			Assert.IsFalse(this.completed, "Full operation should not be completed before response is complete");

			this.second.AllowFullCompletion();
			yield return null;
			Assert.IsTrue(this.second.CompletedRespond, "Second should have completed");
			Assert.IsTrue(this.completed, "Full operation should have completed");
		}

		[UnityTest]
		public IEnumerator UntilReadyToRespond_CompletesResponderExecution_IfStarted()
		{
			this.first.MayRespond = true;
			yield return null;
			this.second.MayRespond = true;
			yield return null;

			Assert.IsFalse(
				this.second.StartedToRespond,
				"Second should not have started to respond before first completed");

			this.first.AllowFullCompletion();
			yield return null;

			Assert.IsTrue(this.first.CompletedRespond, "First should be completed");
			Assert.IsTrue(this.second.StartedToRespond, "Second should have started to respond");

			this.second.AllowFullCompletion();
			yield return null;
			Assert.IsTrue(this.second.CompletedRespond, "Second should have completed response");
		}

		[UnityTest]
		public IEnumerator UntilReadyToRespond_PublishesError_FromAnyResponder([Values] bool errorInFirst)
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

			yield return null;
			Assert.IsInstanceOf<AssertionException>(this.Error);
		}

		[UnityTest]
		public IEnumerator UntilReadyToRespond_TimesOut_AsExpected()
		{
			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;

			Assert.IsFalse(this.completed);
			Assert.IsInstanceOf<AssertionException>(this.Error);
		}
	}
}