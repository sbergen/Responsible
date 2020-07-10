using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Responsible.Tests.Runtime.Utilities;
using UnityEngine.TestTools;
using UniRx;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class RespondToAllOfTests : ResponsibleTestBase
	{
		private ConditionResponder<int> responder1;
		private ConditionResponder<bool> responder2;
		private ConditionResponder<float> responder3;

		private bool completed;

		[SetUp]
		public void SetUp()
		{
			this.responder1 = new ConditionResponder<int>(1, 1);
			this.responder2 = new ConditionResponder<bool>(1, true);
			this.responder3 = new ConditionResponder<float>(1, 0);

			this.completed = false;

			RespondToAllOf(this.responder1.Responder, this.responder2.Responder, this.responder3.Responder)
				.ExpectWithinSeconds(1)
				.ToObservable()
				.Subscribe(_ => this.completed = true, this.StoreError);
		}

		[UnityTest]
		public IEnumerator RespondToAllOf_Completes_WhenAllCompleted()
		{
			Assert.AreEqual(
				(false, false, false, false),
				(this.responder1.CompletedRespond,
					this.responder2.CompletedRespond,
					this.responder3.CompletedRespond,
					this.completed),
				"None should have completed");

			this.responder1.AllowFullCompletion();
			this.responder2.AllowFullCompletion();
			this.responder3.AllowFullCompletion();
			yield return null;
			Assert.AreEqual(
				(true, true, true, true),
				(this.responder1.CompletedRespond,
					this.responder2.CompletedRespond,
					this.responder3.CompletedRespond,
					this.completed),
				"Everything should have completed");
		}

		[UnityTest]
		public IEnumerator RespondToAllOf_ExecutesOnlyOneResponderAtOnce([Values(0, 1, 2)] int firstToRespond)
		{
			var first = this.SelectResponder(firstToRespond);
			var second = this.SelectResponder(firstToRespond + 1);
			var third = this.SelectResponder(firstToRespond + 2);

			first.MayRespond = true;
			yield return null;

			second.MayRespond = true;
			third.MayRespond = true;
			yield return null;

			Assert.AreEqual(
				(firstToRespond == 0, firstToRespond == 1, firstToRespond == 2),
				(this.responder1.StartedToRespond, this.responder2.StartedToRespond, this.responder3.StartedToRespond),
				"Others should not have started to respond");

			first.MayComplete = true;
			yield return null;

			// Order from here is undetermined
			var next = second.StartedToRespond ? second : third;
			var last = second.StartedToRespond ? third : second;
			Assert.IsTrue(next.StartedToRespond, "Next should have started to respond");
			Assert.IsFalse(last.StartedToRespond, "Last should not yet respond");
			next.MayComplete = true;

			yield return null;

			Assert.IsTrue(next.CompletedRespond, "Next should have completed");
			Assert.IsTrue(last.StartedToRespond, "Last should have started to respond");
			last.MayComplete = true;

			yield return null;

			Assert.AreEqual(
				(true, true, true, true),
				(this.responder1.CompletedRespond,
					this.responder2.CompletedRespond,
					this.responder3.CompletedRespond,
					this.completed),
				"Everything should have completed");
		}

		private IConditionResponder SelectResponder(int i)
		{
			switch (i % 3)
			{
				case 0:
					return this.responder1;
				case 1:
					return this.responder2;
				case 2:
					return this.responder3;
				default:
					throw new Exception("Invalid responder index");
			}
		}

	}
}