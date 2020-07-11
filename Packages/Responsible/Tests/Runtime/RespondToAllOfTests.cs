using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Responsible.Tests.Runtime.Utilities;
using UnityEngine.TestTools;
using UniRx;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class RespondToAllOfTests : ResponsibleTestBase
	{
		private ConditionResponder<TestDataBase> responder1;
		private ConditionResponder<TestDataDerived> responder2;

		TestDataBase[] result;
		private bool Completed => this.result != null;

		[SetUp]
		public void SetUp()
		{
			this.responder1 = new ConditionResponder<TestDataBase>(1, new TestDataBase(1));
			this.responder2 = new ConditionResponder<TestDataDerived>(1, new TestDataDerived(2));
			this.result = null;

			RespondToAllOf(this.responder1.Responder, this.responder2.Responder)
				.ExpectWithinSeconds(1)
				.ToObservable()
				.Subscribe(r => this.result = r, this.StoreError);
		}

		[UnityTest]
		public IEnumerator RespondToAllOf_Completes_WhenAllCompleted()
		{
			RespondToAllOf(this.responder1.Responder, this.responder2.Responder)
				.ExpectWithinSeconds(1)
				.ToObservable()
				.Subscribe(r => this.result = r, this.StoreError);

			Assert.AreEqual(
				(false, false, false),
				(this.responder1.CompletedRespond, this.responder2.CompletedRespond, this.Completed),
				"None should have completed");

			this.responder1.AllowFullCompletion();
			this.responder2.AllowFullCompletion();
			yield return null;
			Assert.AreEqual(
				(true, true, true),
				(this.responder1.CompletedRespond,
					this.responder2.CompletedRespond,
					this.Completed),
				"Everything should have completed");
		}

		[UnityTest]
		public IEnumerator RespondToAllOf_ExecutesOnlyOneResponderAtOnce([Values] bool reverseOrder)
		{
			var first = reverseOrder ? (IConditionResponder)this.responder2 : this.responder1;
			var second = reverseOrder ? (IConditionResponder)this.responder1 : this.responder2;

			first.MayRespond = true;
			yield return null;

			second.MayRespond = true;
			yield return null;

			Assert.AreEqual(
				(true, false, false),
				(first.StartedToRespond, second.StartedToRespond, this.Completed),
				"Second should not have started to respond");

			first.MayComplete = true;
			yield return null;

			Assert.IsTrue(second.StartedToRespond, "Second should have started to respond");
			yield return null;

			second.MayComplete = true;
			yield return null;

			Assert.AreEqual(
				(true, true, true),
				(first.CompletedRespond, second.CompletedRespond, this.Completed),
				"Everything should have completed");

			this.AssertResult();
		}

		[UnityTest]
		public IEnumerator RespondToAllOf_PublishesError_OnTimeout([Values] bool completeFirst)
		{
			if (completeFirst)
			{
				this.responder1.AllowFullCompletion();
			}
			else
			{
				this.responder2.AllowFullCompletion();
			}

			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;

			Assert.IsInstanceOf<AssertionException>(this.Error);
		}


		[UnityTest]
		public IEnumerator RespondToAllOf_PublishesError_OnAnyResponderError([Values] bool throwFromFirst)
		{
			var error = new Exception("Fail!");
			if (throwFromFirst)
			{
				this.responder1.AllowCompletionWithError(error);
				this.responder2.AllowFullCompletion();
			}
			else
			{
				this.responder1.AllowFullCompletion();
				this.responder2.AllowCompletionWithError(error);
			}

			yield return null;

			Assert.IsInstanceOf<AssertionException>(this.Error);
		}

		private void AssertResult()
		{
			Assert.IsNull(this.Error);
			Assert.IsNotNull(this.result);

			Assert.AreEqual(
				new[] { typeof(TestDataBase), typeof(TestDataDerived)},
				this.result.Select(r => r.GetType()).ToArray());

			Assert.AreEqual(
				new[] { 1, 2 },
				this.result.Select(r => r.Value).ToArray());
		}

	}
}