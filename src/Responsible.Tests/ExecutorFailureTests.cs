using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Responsible.State;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class ExecutorFailureTests : ResponsibleTestBase
	{
		private const string ExceptionMessage = "Test Exception";

		protected override IFailureListener MakeFailureListener()
			=> Substitute.For<IFailureListener>();

		protected override IGlobalContextProvider MakeGlobalContextProvider()
			=> Substitute.For<IGlobalContextProvider>();

		[Test]
		public async Task Executor_PropagatesAndNotifiesFailure_WhenOperationTimesOut()
		{
			string receivedMessage = null;
			this.FailureListener.OperationFailed(
				Arg.Any<Exception>(),
				Arg.Do<string>(msg => receivedMessage = msg));

			var task = WaitForAllOf(
					WaitForCondition("NO", () => false),
					WaitForCondition("YES", () => true))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.Scheduler.AdvanceFrame(OneSecond);

			Assert.IsNotNull(await AwaitFailureException(task));

			this.FailureListener.Received(1).OperationFailed(
				Arg.Any<TimeoutException>(),
				Arg.Any<string>());

			StateAssert
				.StringContainsInOrder(receivedMessage)
				.Details("timed out")
				.JustCanceled("NO")
				.Completed("YES");
		}

		[Test]
		public async Task Executor_PropagatesAndNotifiesFailure_WhenWaitThrows()
		{
			var task = WaitForCondition(
					"FAIL",
					() => throw new Exception(ExceptionMessage))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			Assert.IsNotNull(await AwaitFailureException(task));
			this.FailureListener.Received(1).OperationFailed(
				Arg.Is<Exception>(e => e.Message == ExceptionMessage),
				Arg.Is<string>(str => str.Contains(ExceptionMessage)));
		}

		[Test]
		public void Executor_NotifiesWaitContext_WhenWaitTimesOut()
		{
			WaitForCondition(
					"Never",
					() => false,
					builder => builder.AddNestedDetails(
						"Should be in logs",
						b => b.AddDetails("Nested details")))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.Scheduler.AdvanceFrame(TimeSpan.FromSeconds(2));

			this.FailureListener.Received(1).OperationFailed(
				Arg.Any<TimeoutException>(),
				Arg.Is<string>(str =>
					str.Contains("Should be in logs") &&
					str.Contains("Nested details")));
		}

		[Test]
		public void Executor_RequestsGlobalContext_OnFailure()
		{
			this.GlobalContextProvider.BuildGlobalContext(Arg.Do<StateStringBuilder>(
				b => b.AddDetails("Global details")));

			Do("Throw", () => throw new Exception())
				.ToTask(this.Executor);

			this.FailureListener.Received(1).OperationFailed(
				Arg.Any<Exception>(),
				Arg.Is<string>(str =>
					str.Contains("Global context:") &&
					str.Contains("  Global details")));
		}

		[Test]
		public void Executor_RequestsGlobalContext_OnTimeout()
		{
			this.GlobalContextProvider.BuildGlobalContext(Arg.Do<StateStringBuilder>(
				b => b.AddDetails("Global details")));

			WaitForCondition("Never", () => false)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.Scheduler.AdvanceFrame(TimeSpan.FromSeconds(2));

			this.FailureListener.Received(1).OperationFailed(
				Arg.Any<TimeoutException>(),
				Arg.Is<string>(str =>
					str.Contains("Global context:") &&
					str.Contains("  Global details")));
		}
	}
}
