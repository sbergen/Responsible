using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UniRx;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class RespondToAnyOfTests : ResponsibleTestBase
	{
		private bool cond1;
		private bool cond2;

		private bool reacted1;
		private bool reacted2;

		private bool complete;
		bool completed;

		[SetUp]
		public void SetUp()
		{
			this.cond1 = this.cond2 = this.reacted1 = this.reacted2 = this.complete = this.completed = false;

			RespondToAnyOf(
					WaitForCondition("First", () => cond1)
						.ThenRespondWith(
							"Complete first",
							_ => Do(() => this.reacted1 = true)),
					WaitForCondition("Second", () => this.cond2)
						.ThenRespondWith(
							"Complete second",
							_ => Do(() => this.reacted2 = true)))
				.Until(WaitForCondition("Complete", () => this.complete))
				.ExpectWithinSeconds(1)
				.Execute()
				.Subscribe(_ => this.completed = true);
		}

		[UnityTest]
		public IEnumerator RespondToAnyOf_Completes_WhenEitherCompleted([Values] bool completeFirst)
		{
			if (completeFirst)
			{
				this.cond1 = true;
			}
			else
			{
				this.cond2 = true;
			}

			// TODO, check why multiple yields are needed

			yield return null;
			yield return null;

			this.complete = true;

			yield return null;
			yield return null;

			Assert.AreEqual(
				(true, completeFirst, !completeFirst),
				(this.completed, this.reacted1, this.reacted2));
		}

		[UnityTest]
		public IEnumerator RespondToAnyOf_ConditionsDoNotExecute_WhenUntilConditionCompletesFirst()
		{
			yield return null;

			this.complete = true;

			yield return null;
			yield return null;

			this.cond1 = true;
			this.cond2 = true;

			yield return null;
			yield return null;

			Assert.AreEqual(
				(true, false, false),
				(this.completed, this.reacted1, this.reacted2));
		}
	}
}