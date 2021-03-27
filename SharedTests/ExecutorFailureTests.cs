using System;
using System.Text.RegularExpressions;
using NSubstitute;
using NUnit.Framework;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class ExecutorFailureTests : ResponsibleTestBase
	{
		private const string ExceptionMessage = "Test Exception";

		protected override IFailureListener MakeFailureListener()
			=> Substitute.For<IFailureListener>();

		[Test]
		public void Executor_PropagatesAndNotifiesFailure_WhenOperationTimesOut()
		{
			var task = WaitForAllOf(
					WaitForCondition("NO", () => false),
					WaitForCondition("YES", () => true))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.TimeProvider.AdvanceFrame(OneSecond);

			Assert.IsNotNull(GetFailureException(task));
			this.FailureListener.Received(1).OperationFailed(
				Arg.Any<TimeoutException>(),
				Arg.Is<string>(log => Regex.IsMatch(
					log,
					@"timed out.*\[\-\] NO.*\[✓\] YES",
					RegexOptions.Singleline)));
		}

		[Test]
		public void Executor_PropagatesAndNotifiesFailure_WhenWaitThrows()
		{
			var task = WaitForCondition(
					"FAIL",
					() => throw new Exception(ExceptionMessage))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			Assert.IsNotNull(GetFailureException(task));
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

			this.TimeProvider.AdvanceFrame(TimeSpan.FromSeconds(2));

			this.FailureListener.Received(1).OperationFailed(
				Arg.Any<TimeoutException>(),
				Arg.Is<string>(str =>
					str.Contains("Should be in logs") &&
					str.Contains("Nested details")));
		}
	}
}
