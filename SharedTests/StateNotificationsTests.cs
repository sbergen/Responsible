using System;
using System.Threading;
using NUnit.Framework;
using Responsible.State;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class StateNotificationsTests : ResponsibleTestBase
	{
		private (TestOperationStateTransition type, ITestOperationState state)? notification;
		private IDisposable subscription;

		[SetUp]
		public void SetUp()
		{
			this.notification = null;
			this.subscription = TestInstructionExecutor.SubscribeToStates(
				(type, state)  => this.notification = (type, state));
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
			Assert.AreEqual(TestOperationStateTransition.Started, this.notification?.type);
		}

		[Test]
		public void StateNotifications_PublishesFinished_WhenOperationCanceled()
		{
			var tokenSource = new CancellationTokenSource();
			WaitForFrames(0).ToTask(this.Executor, tokenSource.Token);
			tokenSource.Cancel();
			Assert.AreEqual(TestOperationStateTransition.Finished, this.notification?.type);
		}

		[Test]
		public void StateNotifications_PublishesFinished_WhenOperationCompleted()
		{
			ImmediateTrue.ExpectWithinSeconds(1).ToTask(this.Executor);
			Assert.AreEqual(TestOperationStateTransition.Finished, this.notification?.type);
		}

		[Test]
		public void StateNotifications_PublishesFinished_WhenOperationFailed()
		{
			Do("Throw error", () => throw new Exception())
				.ToTask(this.Executor);
			Assert.AreEqual(TestOperationStateTransition.Finished, this.notification?.type);
		}

		[Test]
		public void StateNotifications_PublishesMatchingStates_WithMultipleOperations()
		{
			var tokenSource1 = new CancellationTokenSource();
			WaitForFrames(0).ToTask(this.Executor, tokenSource1.Token);
			var state1 = this.notification?.state;

			var tokenSource2 = new CancellationTokenSource();
			WaitForFrames(0).ToTask(this.Executor, tokenSource2.Token);
			var state2 = this.notification?.state;

			tokenSource1.Cancel();
			Assert.AreEqual(state1, this.notification?.state);

			tokenSource2.Cancel();
			Assert.AreEqual(state2, this.notification?.state);
		}
	}
}
