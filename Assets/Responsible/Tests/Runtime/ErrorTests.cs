using System;
using System.Collections;
using System.Text.RegularExpressions;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class ErrorTests : ResponsibleTestBase
	{
		private const string ExceptionMessage = "Test Exception";

		[UnityTest]
		public IEnumerator Executor_PublishesAndLogsError_WhenOperationTimesOut()
		{
			Never
				.ExpectWithinSeconds(2)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;
			Assert.IsNull(this.Error);

			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;

			Assert.IsInstanceOf<AssertionException>(this.Error);
			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Any<string>());
		}

		[UnityTest]
		public IEnumerator Executor_LogsErrorWithContext_WhenOperationTimesOut()
		{
			WaitForAllOf(
					WaitForCondition("NO", () => false),
					WaitForCondition("YES", () => true))
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.CatchIgnore()
				.Subscribe();

			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;

			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(log => Regex.IsMatch(
					log,
					@"timed out.*\[\.\] NO.*\[✓\] YES",
					RegexOptions.Singleline)));
		}

		[Test]
		public void Executor_PublishesAndLogsError_WhenWaitThrows()
		{
			WaitForCondition(
					"FAIL",
					() => throw new Exception(ExceptionMessage))
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			Assert.IsInstanceOf<AssertionException>(this.Error);
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
				.ToObservable(this.Executor)
				.CatchIgnore()
				.Subscribe();

			this.Scheduler.AdvanceBy(TimeSpan.FromSeconds(2));

			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(str =>
					str.Contains("Should be in logs") &&
					str.Contains("Nested details")));
		}
	}
}