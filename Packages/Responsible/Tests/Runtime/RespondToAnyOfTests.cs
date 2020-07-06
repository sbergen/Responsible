using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UniRx;
using UniRx.Diagnostics;
using UnityEngine;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class RespondToAnyOfTests : ResponsibleTestBase
	{
		private bool cond1;
		private bool cond2;

		private bool reacted1;
		private bool reacted2;

		private bool mayComplete1;

		private bool mayComplete;
		bool completed;

		[SetUp]
		public void SetUp()
		{
			this.cond1 = this.cond2 =
				this.reacted1 = this.reacted2 =
					this.mayComplete = this.completed =
						this.mayComplete1 = false;

			var firstResponder = WaitForCondition(
					"May complete first",
					() => this.mayComplete1)
				.ExpectWithinSeconds(1)
				.ContinueWith(Do(() => this.reacted1 = true));

			RespondToAnyOf(
					WaitForCondition("First", () => cond1)
						.ThenRespondWith("Complete first", firstResponder),
					WaitForCondition("Second", () => this.cond2)
						.ThenRespondWith(
							"Complete second",
							_ => Do(() => this.reacted2 = true)))
				.Until(WaitForCondition("Complete", () => this.mayComplete))
				.ExpectWithinSeconds(1)
				.Execute()
				.Subscribe(_ => this.completed = true);
		}

		[UnityTest]
		public IEnumerator RespondToAnyOf_Completes_WhenEitherCompleted([Values] bool completeFirst)
		{
			this.mayComplete1 = true;

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

			this.mayComplete = true;

			yield return null;
			yield return null;

			Assert.AreEqual(
				(true, completeFirst, !completeFirst),
				(this.completed, this.reacted1, this.reacted2));
		}

		[UnityTest]
		public IEnumerator RespondToAnyOf_ConditionsDoNotExecute_WhenUntilConditionCompletesFirst()
		{
			this.mayComplete1 = true;

			yield return null;

			this.mayComplete = true;

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

		[UnityTest]
		public IEnumerator RespondToAnyOf_ExecutesRespondersSequentially_WhenMultipleReadyToRespond()
		{
			// Allow both to start. The first one should take precedence.
			this.cond1 = true;
			this.cond2 = true;

			// Yield a few times to be safe
			yield return null;
			yield return null;
			Assert.IsFalse(this.reacted2, "Second responder should not start while first is executing");

			// Complete everything
			this.mayComplete = true;
			this.mayComplete1 = true;
			yield return null;
			yield return null;

			Assert.AreEqual(
				(true, true, true),
				(this.completed, this.reacted1, this.reacted2));
		}
	}
}