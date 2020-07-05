using System;
using System.Collections;
using System.Text.RegularExpressions;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.RF;
using static Responsible.Tests.Runtime.TestInstances;

namespace Responsible.Tests.Runtime
{
	public class ErrorTests
	{
		private ILogger logger;
		private TestScheduler scheduler;
		private IDisposable setup;

		[SetUp]
		public void SetUp()
		{
			this.logger = Substitute.For<ILogger>();
			this.scheduler = new TestScheduler();
			this.setup = TestInstructionExtensions.OverrideExecutor(this.scheduler, logger: this.logger);
		}

		[TearDown]
		public void TearDown()
		{
			this.setup.Dispose();
		}

		[UnityTest]
		public IEnumerator Executor_PublishesAndLogsError_WhenOperationTimesOut()
		{
			Exception error = null;
			Never
				.ExpectWithinSeconds(2)
				.Execute()
				.Subscribe(_ => { }, e => error = e);

			this.scheduler.AdvanceBy(OneSecond);
			yield return null;
			Assert.IsNull(error);

			this.scheduler.AdvanceBy(OneSecond);
			yield return null;

			Assert.IsInstanceOf<AssertionException>(error);
			this.logger.Received(1).Log(
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

			this.scheduler.AdvanceBy(OneSecond);
			yield return null;

			this.logger.Received(1).Log(
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
					() => throw new Exception("THE ERROR"))
				.ExpectWithinSeconds(1)
				.Execute()
				.Subscribe(_ => { }, e => error = e);

			Assert.IsInstanceOf<AssertionException>(error);
			this.logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(str => str.Contains("THE ERROR")));
		}

		[Test]
		public void Executor_PublishesAndLogsError_WhenInstructionThrows()
		{
			Exception error = null;
			Do(() => throw new Exception("THE ERROR"))
				.Execute()
				.Subscribe(_ => { }, e => error = e);

			Assert.IsInstanceOf<AssertionException>(error);
			this.logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(str => str.Contains("THE ERROR")));
		}
	}
}