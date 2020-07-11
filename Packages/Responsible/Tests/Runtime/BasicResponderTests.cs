using System;
using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class ResponderTests : ResponsibleTestBase
	{
		private bool readyToReact;
		private bool readyToComplete;

		private bool startedToReact;

		private ITestResponder<Unit> respondToConditions;

		[SetUp]
		public void SetUp()
		{
			this.readyToReact = this.readyToComplete = this.startedToReact = false;

			var react =
				Do("Set started to react", () => this.startedToReact = true)
					.ContinueWith(_ => WaitForCondition(
							"Ready to complete",
							() => this.readyToComplete)
						.ExpectWithinSeconds(3));

			this.respondToConditions =
				WaitForCondition("To be ready", () => this.readyToReact)
					.ThenRespondWith("React", react);
		}

		[UnityTest]
		public IEnumerator BasicResponder_CompletesAtExpectedTimes()
		{
			var completed = false;
			this.respondToConditions
				.ExpectWithinSeconds(1)
				.ToObservable()
				.Subscribe(_ => completed = true);

			yield return null;
			Assert.IsFalse(this.startedToReact);
			Assert.IsFalse(completed);

			this.readyToReact = true;

			yield return null;
			Assert.IsTrue(this.startedToReact);
			Assert.IsFalse(completed);

			readyToComplete = true;
			yield return null;
			Assert.IsTrue(this.startedToReact);
			Assert.IsTrue(completed);
		}

		[UnityTest]
		public IEnumerator BasicResponder_RespectsIndividualTimeouts()
		{
			var completed = false;
			this.respondToConditions
				.ExpectWithinSeconds(3)
				.ToObservable()
				.Subscribe(_ => completed = true);

			// Timeout for condition is 3 seconds
			this.Scheduler.AdvanceBy(TimeSpan.FromSeconds(2));
			yield return null;
			this.readyToReact = true;
			yield return null;
			Assert.IsFalse(completed);

			// After this we have waited 4 seconds, which is more than either of the individual timeouts
			this.Scheduler.AdvanceBy(TimeSpan.FromSeconds(2));
			yield return null;
			this.readyToComplete = true;
			yield return null;

			Assert.IsTrue(completed);
		}
	}
}