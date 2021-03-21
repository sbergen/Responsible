using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.Tests.Runtime.NoRx.Utilities;
using Responsible.NoRx;
using static Responsible.NoRx.Responsibly;

namespace Responsible.Tests.Runtime.NoRx
{
	public class RespondToAllOfTests : ResponsibleTestBase
	{
		private ConditionResponder<TestDataBase> responder1;
		private ConditionResponder<TestDataBase> responder2;

		private Task<TestDataBase[]> task;
		private bool Completed => this.task.IsCompleted;

		[SetUp]
		public void SetUp()
		{
			this.responder1 = new ConditionResponder<TestDataBase>(1, new TestDataBase(1));
			this.responder2 = new ConditionResponder<TestDataBase>(1, new TestDataDerived(2));
			this.task = null;

			this.task = RespondToAllOf(this.responder1.Responder, this.responder2.Responder)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);
		}

		[Test]
		public void RespondToAllOf_Completes_WhenAllCompleted()
		{
			Assert.AreEqual(
				(false, false, false),
				(this.responder1.CompletedRespond, this.responder2.CompletedRespond, this.Completed),
				"None should have completed");

			this.responder1.AllowFullCompletion();
			this.responder2.AllowFullCompletion();
			this.AdvanceDefaultFrame();

			Assert.AreEqual(
				(true, true, true),
				(this.responder1.CompletedRespond,
					this.responder2.CompletedRespond,
					this.Completed),
				"Everything should have completed");
		}

		[Test]
		public void RespondToAllOf_ExecutesOnlyOneResponderAtOnce([Values] bool reverseOrder)
		{
			var first = reverseOrder ? (IConditionResponder)this.responder2 : this.responder1;
			var second = reverseOrder ? (IConditionResponder)this.responder1 : this.responder2;

			first.MayRespond = true;
			this.AdvanceDefaultFrame();

			second.MayRespond = true;
			this.AdvanceDefaultFrame();

			Assert.AreEqual(
				(true, false, false),
				(first.StartedToRespond, second.StartedToRespond, this.Completed),
				"Second should not have started to respond");

			first.MayComplete = true;
			this.AdvanceDefaultFrame();

			Assert.IsTrue(second.StartedToRespond, "Second should have started to respond");
			this.AdvanceDefaultFrame();

			second.MayComplete = true;
			this.AdvanceDefaultFrame();

			Assert.AreEqual(
				(true, true, true),
				(first.CompletedRespond, second.CompletedRespond, this.Completed),
				"Everything should have completed");

			this.AssertResult();
		}

		[Test]
		public void RespondToAllOf_CompletesWithError_OnTimeout([Values] bool completeFirst)
		{
			if (completeFirst)
			{
				this.responder1.AllowFullCompletion();
			}
			else
			{
				this.responder2.AllowFullCompletion();
			}

			this.TimeProvider.AdvanceFrame(OneSecond);

			Assert.IsNotNull(GetAssertionException(this.task));
		}


		[Test]
		public void RespondToAllOf_CompletesWithError_OnAnyResponderError([Values] bool throwFromFirst)
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

			this.AdvanceDefaultFrame();

			Assert.IsNotNull(GetAssertionException(this.task));
		}

		private void AssertResult()
		{
			Assert.IsFalse(this.task.IsFaulted);
			Assert.IsTrue(this.task.IsCompleted);

			Assert.AreEqual(
				new[] { typeof(TestDataBase), typeof(TestDataDerived)},
				this.task.AssertSynchronousResult().Select(r => r.GetType()).ToArray());

			Assert.AreEqual(
				new[] { 1, 2 },
				this.task.AssertSynchronousResult().Select(r => r.Value).ToArray());
		}
	}
}
