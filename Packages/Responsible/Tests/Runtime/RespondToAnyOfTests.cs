using System.Collections;
using NUnit.Framework;
using Responsible.Tests.Runtime.Utilities;
using UnityEngine.TestTools;
using UniRx;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class RespondToAnyOfTests : ResponsibleTestBase
	{
		private ConditionResponder responder1;
		private ConditionResponder responder2;

		private bool mayComplete;
		private bool completed;

		[SetUp]
		public void SetUp()
		{
			this.mayComplete = this.completed = false;
			this.responder1 = new ConditionResponder(1);
			this.responder2 = new ConditionResponder(1);

			RespondToAnyOf(
					this.responder1.Responder,
					this.responder2.Responder)
				.Until(WaitForCondition("Complete", () => this.mayComplete))
				.ExpectWithinSeconds(1)
				.Execute()
				.Subscribe(_ => this.completed = true);
		}

		[UnityTest]
		public IEnumerator RespondToAnyOf_Completes_WhenEitherCompleted([Values] bool completeFirst)
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

			// TODO, check why multiple yields are needed

			yield return null;
			yield return null;

			this.mayComplete = true;

			yield return null;
			yield return null;

			Assert.AreEqual(
				(true, completeFirst, !completeFirst),
				(this.completed, this.responder1.CompletedRespond, this.responder2.CompletedRespond));
		}

		[UnityTest]
		public IEnumerator RespondToAnyOf_ConditionsDoNotExecute_WhenUntilConditionCompletesFirst()
		{
			this.responder1.MayComplete = true;
			this.responder2.MayComplete = true;

			yield return null;

			this.mayComplete = true;

			yield return null;
			yield return null;

			this.responder1.MayRespond = true;
			this.responder2.MayRespond = true;

			yield return null;
			yield return null;

			Assert.AreEqual(
				(true, false, false),
				(this.completed, this.responder1.StartedToRespond, this.responder2.StartedToRespond));
		}

		[UnityTest]
		public IEnumerator RespondToAnyOf_ExecutesRespondersSequentially_WhenMultipleReadyToRespond()
		{
			// Allow both to start. The first one should take precedence.
			this.responder1.MayRespond = true;
			this.responder2.MayRespond = true;

			// Yield a few times to be safe
			yield return null;
			yield return null;
			Assert.IsFalse(this.responder2.StartedToRespond, "Second responder should not start while first is executing");

			// Complete everything
			this.mayComplete = true;
			this.responder1.MayComplete = true;
			this.responder2.MayComplete = true;
			yield return null;
			yield return null;

			Assert.AreEqual(
				(true, true, true),
				(this.completed, this.responder1.CompletedRespond, this.responder2.CompletedRespond));
		}
	}
}