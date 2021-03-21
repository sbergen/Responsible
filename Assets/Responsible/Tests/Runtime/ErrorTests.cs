using System;
using System.Text.RegularExpressions;
using NSubstitute;
using NUnit.Framework;
using Responsible.NoRx;
using UnityEngine;
using static Responsible.NoRx.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class ErrorTests : ResponsibleTestBase
	{
		private const string ExceptionMessage = "Test Exception";

		[Test]
		public void Executor_PublishesAndLogsError_WhenOperationTimesOut()
		{
			var task = Never
				.ExpectWithinSeconds(2)
				.ToTask(this.Executor);

			this.TimeProvider.AdvanceFrame(OneSecond);
			Assert.IsFalse(task.IsFaulted);

			this.TimeProvider.AdvanceFrame(OneSecond);

			Assert.IsNotNull(GetAssertionException(task));
			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Any<string>());
		}

		[Test]
		public void Executor_LogsErrorWithContext_WhenOperationTimesOut()
		{
			WaitForAllOf(
					WaitForCondition("NO", () => false),
					WaitForCondition("YES", () => true))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.TimeProvider.AdvanceFrame(OneSecond);

			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(log => Regex.IsMatch(
					log,
					@"timed out.*\[\-\] NO.*\[✓\] YES",
					RegexOptions.Singleline)));
		}

		[Test]
		public void Executor_PublishesAndLogsError_WhenWaitThrows()
		{
			var task = WaitForCondition(
					"FAIL",
					() => throw new Exception(ExceptionMessage))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			Assert.IsNotNull(GetAssertionException(task));
			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(str => str.Contains(ExceptionMessage)));
		}

		[Test]
		public void Executor_LogsExtraWaitContext_WhenWaitTimesOut()
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

			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(str =>
					str.Contains("Should be in logs") &&
					str.Contains("Nested details")));
		}
	}
}
