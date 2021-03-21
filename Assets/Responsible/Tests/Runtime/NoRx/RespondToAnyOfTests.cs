using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.NoRx;
using Responsible.Tests.Runtime.NoRx.Utilities;
using static Responsible.NoRx.Responsibly;

namespace Responsible.Tests.Runtime.NoRx
{
	public class RespondToAnyOfTests : ResponsibleTestBase
	{
		private ConditionResponder responder1;
		private ConditionResponder responder2;

		private bool mayComplete;
		private Task<Nothing> task;

		[SetUp]
		public void SetUp()
		{
			this.mayComplete = false;
			this.responder1 = new ConditionResponder(1);
			this.responder2 = new ConditionResponder(1);

			this.task = RespondToAnyOf(
					this.responder1.Responder,
					this.responder2.Responder)
				.Until(WaitForCondition("Complete", () => this.mayComplete))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);
		}

		[Test]
		public void RespondToAnyOf_Completes_WhenEitherCompleted([Values] bool completeFirst)
		{
			this.responder1.MayComplete = true;
			this.responder2.MayComplete = true;

			if (completeFirst)
			{
				this.responder1.MayRespond = true;
			}
			else
			{
				this.responder2.MayRespond = true;
			}

			this.AdvanceDefaultFrame();

			this.mayComplete = true;

			this.AdvanceDefaultFrame();

			this.task.Wait(TimeSpan.Zero);

			Assert.AreEqual(
				(true, completeFirst, !completeFirst),
				(this.task.IsCompleted, this.responder1.CompletedRespond, this.responder2.CompletedRespond));
		}

		[Test]
		public void RespondToAnyOf_ConditionsDoNotExecute_WhenUntilConditionCompletesFirst()
		{
			this.responder1.MayComplete = true;
			this.responder2.MayComplete = true;

			this.AdvanceDefaultFrame();

			this.mayComplete = true;

			this.AdvanceDefaultFrame();

			this.responder1.MayRespond = true;
			this.responder2.MayRespond = true;

			this.AdvanceDefaultFrame();

			Assert.AreEqual(
				(true, false, false),
				(this.task.IsCompleted, this.responder1.StartedToRespond, this.responder2.StartedToRespond));
		}

		[Test]
		public void RespondToAnyOf_ExecutesRespondersSequentially_WhenMultipleReadyToRespond()
		{
			this.responder1.MayRespond = true;
			this.responder2.MayRespond = true;

			this.AdvanceDefaultFrame();
			Assert.IsFalse(
				this.responder1.StartedToRespond && this.responder2.StartedToRespond,
				"Only one responder should start while the other is executing");

			// Complete everything
			this.mayComplete = true;
			this.responder1.MayComplete = true;
			this.responder2.MayComplete = true;
			this.AdvanceDefaultFrame();

			Assert.AreEqual(
				(true, true, true),
				(this.task.IsCompleted, this.responder1.CompletedRespond, this.responder2.CompletedRespond));
		}
	}
}