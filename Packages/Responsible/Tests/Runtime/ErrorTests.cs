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
		[UnityTest]
		public IEnumerator Executor_PublishesAndLogsError_WhenOperationTimesOut()
		{
			Never
				.ExpectWithinSeconds(2)
				.ToObservable()
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
				.ToObservable()
				.CatchIgnore()
				.Subscribe();

			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;

			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(log => Regex.IsMatch(
					log,
					@"Timed out.*\[\.\] NO.*\[✓\] YES",
					RegexOptions.Singleline)));
		}

		[Test]
		public void Executor_PublishesAndLogsError_WhenWaitThrows()
		{
			WaitForCondition(
					"FAIL",
					() => throw new Exception(ExceptionMessage))
				.ExpectWithinSeconds(1)
				.ToObservable()
				.Subscribe(Nop, this.StoreError);

			Assert.IsInstanceOf<AssertionException>(this.Error);
			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(str => str.Contains(ExceptionMessage)));
		}

		[Test]
		public void Executor_OnlyPublishesError_WhenSynchronousInstructionThrows()
		{
			// If a synchronous instruction is executed on its own, it should be enough to just publish an error,
			// as you shouldn't need to use Do at the top level of a test method.

			Do(() => throw new Exception(ExceptionMessage))
				.ToObservable()
				.Subscribe(Nop, this.StoreError);

			Assert.IsInstanceOf<AssertionException>(this.Error);
			Assert.That(this.Error.Message, Contains.Substring(ExceptionMessage));
			this.Logger.DidNotReceive().Log(
				LogType.Error,
				Arg.Any<string>());
		}

		[UnityTest]
		public IEnumerator Executor_PublishesAndLogsError_WhenSynchronousInstructionThrowsInAsyncContext()
		{
			IEnumerator Coroutine()
			{
				yield return null;
				yield return Do(() => throw new Exception(ExceptionMessage)).ToYieldInstruction();
			}

			RunCoroutine(
					"Run throwing coroutine",
					1,
					Coroutine)
				.ToObservable()
				.Subscribe(Nop, this.StoreError);

			yield return null;
			yield return null;

			Assert.IsInstanceOf<AssertionException>(this.Error);
			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(msg => msg.Contains(ExceptionMessage)));
		}

		[Test]
		public void Executor_LogsExtraWaitContext_WhenWaitTimesOut()
		{
			WaitForCondition(
					"Never",
					() => false,
					builder => builder.Add("Should be in logs"))
				.ExpectWithinSeconds(0)
				.ToObservable()
				.CatchIgnore()
				.Subscribe();

			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(str => str.Contains("Should be in logs")));
		}
	}
}