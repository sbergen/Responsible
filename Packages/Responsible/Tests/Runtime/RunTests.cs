using System;
using System.Collections;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;
using UniRx;
using UnityEngine;
using static Responsible.RF;

namespace Responsible.Tests.Runtime
{
	public class RunTests : ResponsibleTestBase
	{
		[UnityTest]
		public IEnumerator RunCoroutine_Completes_WhenCoroutineIsComplete()
		{
			var complete = false;
			var completed = false;

			IEnumerator WaitForComplete()
			{
				// ReSharper disable once AccessToModifiedClosure
				while (!complete)
				{
					yield return null;
				}
			}

			RunCoroutine(
					"Wait for completion",
					10,
					WaitForComplete)
				.Execute()
				.Subscribe(_ => completed = true);

			yield return null;
			Assert.IsFalse(completed);

			complete = true;
			yield return null;
			Assert.IsTrue(completed);
		}

		[UnityTest]
		public IEnumerator Executor_PublishesAndLogsError_WhenCoroutineThrows()
		{
			IEnumerator ThrowFromCoroutine()
			{
				yield return null;
				throw new Exception(ExceptionMessage);
			}

			Exception error = null;
			RunCoroutine(
					"Throw from coroutine",
					10,
					ThrowFromCoroutine)
				.Execute()
				.Subscribe(_ => { }, e => error = e);

			yield return null;

			Assert.IsInstanceOf<AssertionException>(error);
			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(str => str.Contains(ExceptionMessage)));
		}

		[UnityTest]
		public IEnumerator Executor_PublishesAndLogsError_WhenCoroutineTimesOut()
		{
			IEnumerator ThrowFromCoroutine()
			{
				while (true)
				{
					yield return null;
				}
				// ReSharper disable once IteratorNeverReturns
			}

			Exception error = null;
			RunCoroutine(
					"Infinite coroutine",
					1,
					ThrowFromCoroutine)
				.Execute()
				.Subscribe(_ => { }, e => error = e);

			yield return null;
			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;

			Assert.IsInstanceOf<AssertionException>(error);
			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(str =>
					str.Contains("Timed out") &&
					str.Contains("Infinite coroutine")));
		}
	}
}