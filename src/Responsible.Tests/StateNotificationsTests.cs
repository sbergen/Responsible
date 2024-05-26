using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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
			this.notification.Should().BeNull();
			wait.ToTask(this.Executor);
			this.notification?.type.Should().Be(TestOperationStateTransition.Started);
		}

		[Test]
		public async Task StateNotifications_PublishesFinished_WhenOperationCanceled()
		{
			var tokenSource = new CancellationTokenSource();
			var task = WaitForFrames(0).ToTask(this.Executor, tokenSource.Token);
			tokenSource.Cancel();

			await AwaitFailureExceptionForUnity(task);

			this.notification?.type.Should().Be(TestOperationStateTransition.Finished);
		}

		[Test]
		public void StateNotifications_PublishesFinished_WhenOperationCompleted()
		{
			ImmediateTrue.ExpectWithinSeconds(1).ToTask(this.Executor);
			this.notification?.type.Should().Be(TestOperationStateTransition.Finished);
		}

		[Test]
		public void StateNotifications_PublishesFinished_WhenOperationFailed()
		{
			Do("Throw error", () => throw new Exception())
				.ToTask(this.Executor);
			this.notification?.type.Should().Be(TestOperationStateTransition.Finished);
		}

		[Test]
		public async Task StateNotifications_PublishesMatchingStates_WithMultipleOperations()
		{
			var tokenSource1 = new CancellationTokenSource();
			var task1 = WaitForFrames(0).ToTask(this.Executor, tokenSource1.Token);
			var state1 = this.notification?.state;

			var tokenSource2 = new CancellationTokenSource();
			var task2 = WaitForFrames(0).ToTask(this.Executor, tokenSource2.Token);
			var state2 = this.notification?.state;

			tokenSource1.Cancel();
			await AwaitFailureExceptionForUnity(task1);
			this.notification?.state.Should().Be(state1);

			tokenSource2.Cancel();
			await AwaitFailureExceptionForUnity(task2);
			this.notification?.state.Should().Be(state2);
		}
	}
}
