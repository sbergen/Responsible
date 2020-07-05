using System;
using System.Collections;
using System.Text.RegularExpressions;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.RF;

namespace Responsible.Tests.Runtime
{
	public class ErrorTests : ResponsibleTestBase
	{
		[UnityTest]
		public IEnumerator Executor_PublishesAndLogsError_WhenOperationTimesOut()
		{
			Exception error = null;
			Never
				.ExpectWithinSeconds(2)
				.Execute()
				.Subscribe(_ => { }, e => error = e);

			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;
			Assert.IsNull(error);

			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;

			Assert.IsInstanceOf<AssertionException>(error);
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
				.Execute()
				.CatchIgnore()
				.Subscribe();

			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;

			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(log => Regex.IsMatch(
					log,
					"NO.*YES.*Timed out.*Completed.*YES",
					RegexOptions.Singleline)));
		}

		[Test]
		public void Executor_PublishesAndLogsError_WhenWaitThrows()
		{
			Exception error = null;
			WaitForCondition(
					"FAIL",
					() => throw new Exception(ExceptionMessage))
				.ExpectWithinSeconds(1)
				.Execute()
				.Subscribe(_ => { }, e => error = e);

			Assert.IsInstanceOf<AssertionException>(error);
			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(str => str.Contains(ExceptionMessage)));
		}

		[Test]
		public void Executor_PublishesAndLogsError_WhenInstructionThrows()
		{
			Exception error = null;
			Do(() => throw new Exception(ExceptionMessage))
				.Execute()
				.Subscribe(_ => { }, e => error = e);

			Assert.IsInstanceOf<AssertionException>(error);
			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(str => str.Contains(ExceptionMessage)));
		}
	}
}