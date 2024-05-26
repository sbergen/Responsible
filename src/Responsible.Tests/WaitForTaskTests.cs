using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Responsible.State;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class WaitForTaskTests : ResponsibleTestBase
	{
		private const string Description = "Task";

		private TaskCompletionSource<int> completionSource;
		private bool taskStarted;
		private ITestOperationState<int> state;

		[SetUp]
		public void SetUp()
		{
			this.completionSource = new TaskCompletionSource<int>();
			this.taskStarted = false;
			this.state = WaitForTask(Description, this.RunTask).CreateState();
		}

		[Test]
		public void BuildingInitialState_DoesNotExecuteTask()
		{
			this.taskStarted.Should().BeFalse();
			StateAssert.StringContainsInOrder(this.state.ToString()).NotStarted(Description);
		}

		[Test]
		public void ExecutingInstruction_StartsTask()
		{
			this.state.ToTask(this.Executor);

			this.taskStarted.Should().BeTrue();
			StateAssert.StringContainsInOrder(this.state.ToString()).Waiting(Description);
		}

		[Test]
		public void SuccessfulExecution_RunsCorrectly()
		{
			var task = this.state.ToTask(this.Executor);
			this.completionSource.SetResult(42);

			StateAssert.StringContainsInOrder(this.state.ToString()).Completed(Description);
			task.IsCompleted.Should().BeTrue();
			task.Result.Should().Be(42);
		}

		[Test]
		public async Task FailedExecution_RunsCorrectly()
		{
			var task = this.state.ToTask(this.Executor);
			var exception = new Exception("Test failure");
			this.completionSource.SetException(exception);

			StateAssert.StringContainsInOrder(this.state.ToString()).Failed(Description);
			var error = await AwaitFailureExceptionForUnity(task);
			error.InnerException.Should().BeSameAs(exception);
		}

		[Test]
		public async Task CanceledExecution_RunsCorrectly()
		{
			using (var cancellationSource = new CancellationTokenSource())
			{
				var task = this.state.ToTask(this.Executor, cancellationSource.Token);
				cancellationSource.Cancel();
				var error = await AwaitFailureExceptionForUnity(task);

				StateAssert.StringContainsInOrder(this.state.ToString()).Canceled(Description);
				error.InnerException.Should().BeOfType<TaskCanceledException>();
			}
		}

		[Test]
		public void StateDescription_MatchesExpected_WhenExpectInlined()
		{
			var expectState = WaitForTask("Task", this.RunTask)
				.ExpectWithinSeconds(1)
				.CreateState();

			StateAssert.StringContainsInOrder(expectState.ToString())
				.NotStarted("Task EXPECTED WITHIN");
		}

		[Test]
		public void StateDescription_MatchesExpected_WhenExpectNotInlined()
		{
			var expectState = WaitForTask("Task", this.RunTask)
				.AndThen(WaitForTask("Task2", this.RunTask))
				.ExpectWithinSeconds(1)
				.CreateState();

			StateAssert.StringContainsInOrder(expectState.ToString())
				.NotStarted("EXPECT WITHIN")
				.NotStarted("Task")
				.NotStarted("Task2");
		}

		private async Task<int> RunTask(CancellationToken cancellationToken)
		{
			this.taskStarted = true;
			using (cancellationToken.Register(this.completionSource.SetCanceled))
			{
				return await this.completionSource.Task;
			}
		}
	}
}
