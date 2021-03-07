using System;
using NUnit.Framework;
using Responsible.State;
using UniRx;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class StateNotificationsTests : ResponsibleTestBase
	{
		private TestOperationStateNotification notification;
		private IDisposable subscription;

		[SetUp]
		public void SetUp()
		{
			this.notification = null;
			this.subscription = TestInstructionExecutor.StateNotifications.Subscribe(s => this.notification = s);
		}

		[TearDown]
		public void TearDown()
		{
			this.subscription.Dispose();
		}

		[Test]
		public void StateNotifications_PublishesStarted_WhenOperationStarted()
		{
			var wait = WaitForFrames(0).ToObservable(this.Executor);
			Assert.IsNull(this.notification);
			using (wait.Subscribe())
			{
				Assert.IsInstanceOf<TestOperationStateNotification.Started>(this.notification);
			}
		}

		[Test]
		public void StateNotifications_PublishesFinished_WhenOperationCanceled()
		{
			WaitForFrames(0).ToObservable(this.Executor).Subscribe().Dispose();
			Assert.IsInstanceOf<TestOperationStateNotification.Finished>(this.notification);
		}

		[Test]
		public void StateNotifications_PublishesFinished_WhenOperationCompleted()
		{
			ImmediateTrue.ExpectWithinSeconds(1).ToObservable(this.Executor).Subscribe();
			Assert.IsInstanceOf<TestOperationStateNotification.Finished>(this.notification);
		}

		[Test]
		public void StateNotifications_PublishesFinished_WhenOperationFailed()
		{
			Do("Throw error", () => throw new Exception())
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);
			Assert.IsInstanceOf<TestOperationStateNotification.Finished>(this.notification);
		}

		[Test]
		public void StateNotifications_PublishesMatchingStates_WithMultipleOperations()
		{
			var subscription1 = WaitForFrames(0).ToObservable(this.Executor).Subscribe();
			var state1 = this.notification.State;

			var subscription2 = WaitForFrames(0).ToObservable(this.Executor).Subscribe();
			var state2 = this.notification.State;

			subscription1.Dispose();
			Assert.AreEqual(state1, this.notification.State);

			subscription2.Dispose();
			Assert.AreEqual(state2, this.notification.State);
		}
	}
}
