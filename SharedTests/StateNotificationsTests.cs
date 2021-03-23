using System;
using System.Threading;
using NUnit.Framework;
using Responsible.State;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class StateNotificationsTests : ResponsibleTestBase
	{
		private TestOperationStateNotification notification;
		private IDisposable subscription;

		[SetUp]
		public void SetUp()
		{
			this.notification = null;
			this.subscription = TestInstructionExecutor.SubscribeToStates(s => this.notification = s);
		}

		[TearDown]
		public void TearDown()
		{
			this.subscription.Dispose();
		}

		[Test]
		public void StateNotifications_PublishesStarted_WhenOperationStarted()
		{
			var wait = WaitForFrames(0);
			Assert.IsNull(this.notification);
			wait.ToTask(this.Executor);
			Assert.IsInstanceOf<TestOperationStateNotification.Started>(this.notification);
		}

		[Test]
		public void StateNotifications_PublishesFinished_WhenOperationCanceled()
		{
			var tokenSource = new CancellationTokenSource();
			WaitForFrames(0).ToTask(this.Executor, tokenSource.Token);
			tokenSource.Cancel();
			Assert.IsInstanceOf<TestOperationStateNotification.Finished>(this.notification);
		}

		[Test]
		public void StateNotifications_PublishesFinished_WhenOperationCompleted()
		{
			ImmediateTrue.ExpectWithinSeconds(1).ToTask(this.Executor);
			Assert.IsInstanceOf<TestOperationStateNotification.Finished>(this.notification);
		}

		[Test]
		public void StateNotifications_PublishesFinished_WhenOperationFailed()
		{
			Do("Throw error", () => throw new Exception())
				.ToTask(this.Executor);
			Assert.IsInstanceOf<TestOperationStateNotification.Finished>(this.notification);
		}

		[Test]
		public void StateNotifications_PublishesMatchingStates_WithMultipleOperations()
		{
			var tokenSource1 = new CancellationTokenSource();
			WaitForFrames(0).ToTask(this.Executor, tokenSource1.Token);
			var state1 = this.notification.State;

			var tokenSource2 = new CancellationTokenSource();
			WaitForFrames(0).ToTask(this.Executor, tokenSource2.Token);
			var state2 = this.notification.State;

			tokenSource1.Cancel();
			Assert.AreEqual(state1, this.notification.State);

			tokenSource2.Cancel();
			Assert.AreEqual(state2, this.notification.State);
		}
	}
}
