using System;
using NUnit.Framework;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class ResponderTests : ResponsibleTestBase
	{
		private bool readyToReact;
		private bool readyToComplete;

		private bool startedToReact;

		private ITestResponder<Nothing> respondToConditions;

		[SetUp]
		public void SetUp()
		{
			this.readyToReact = this.readyToComplete = this.startedToReact = false;

			var react =
				DoAndReturn("Set started to react", () => this.startedToReact = true)
					.ContinueWith(_ => WaitForCondition(
							"Ready to complete",
							() => this.readyToComplete)
						.ExpectWithinSeconds(3));

			this.respondToConditions =
				WaitForCondition("To be ready", () => this.readyToReact)
					.ThenRespondWith("React", react);
		}

		[Test]
		public void BasicResponder_CompletesAtExpectedTimes()
		{
			var task = this.respondToConditions
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(this.startedToReact);
			Assert.IsFalse(task.IsCompleted);

			this.readyToReact = true;

			this.AdvanceDefaultFrame();
			Assert.IsTrue(this.startedToReact);
			Assert.IsFalse(task.IsCompleted);

			this.readyToComplete = true;
			this.AdvanceDefaultFrame();
			Assert.IsTrue(this.startedToReact);
			Assert.IsTrue(task.IsCompleted);
		}

		[Test]
		public void BasicResponder_RespectsIndividualTimeouts()
		{
			var task = this.respondToConditions
				.ExpectWithinSeconds(3)
				.ToTask(this.Executor);

			// Timeout for condition is 3 seconds
			this.TimeProvider.AdvanceFrame(TimeSpan.FromSeconds(2));
			this.readyToReact = true;
			this.TimeProvider.AdvanceFrame(TimeSpan.Zero);
			Assert.IsFalse(task.IsCompleted);

			// After this we have waited 4 seconds, which is more than either of the individual timeouts
			this.TimeProvider.AdvanceFrame(TimeSpan.FromSeconds(2));
			this.readyToComplete = true;
			this.TimeProvider.AdvanceFrame(TimeSpan.Zero);

			Assert.IsTrue(task.IsCompleted);
		}
	}
}
