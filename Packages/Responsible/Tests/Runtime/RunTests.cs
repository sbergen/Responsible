using System;
using System.Collections;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;
using UniRx;
using UnityEngine;
using static Responsible.Responsibly;

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
				.ToObservable()
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

			RunCoroutine(
					"Throw from coroutine",
					10,
					ThrowFromCoroutine)
				.ToObservable()
				.Subscribe(Nop, this.StoreError);

			yield return null;

			Assert.IsInstanceOf<AssertionException>(this.Error);
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

			RunCoroutine(
					"Infinite coroutine",
					1,
					ThrowFromCoroutine)
				.ToObservable()
				.Subscribe(Nop, this.StoreError);

			yield return null;
			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;

			Assert.IsInstanceOf<AssertionException>(this.Error);
			this.Logger.Received(1).Log(
				LogType.Error,
				Arg.Is<string>(str =>
					str.Contains("Timed out") &&
					str.Contains("Infinite coroutine")));
		}
	}
}