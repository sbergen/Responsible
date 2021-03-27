using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.UnityTests.Utilities;
using UnityEngine;
using UnityEngine.TestTools;

namespace Responsible.UnityTests
{
	public class ToYieldInstructionTests : UnityTestBase
	{
		private static readonly ITestInstruction<object> Never = Responsibly
			.WaitForCondition("Never", () => false)
			.ExpectWithinSeconds(1);

		private CoroutineRunner coroutineRunner;
		private int? completedOnFrame;
		private bool mayComplete;

		[SetUp]
		public void SetUp()
		{
			this.completedOnFrame = null;
			this.mayComplete = false;
			this.coroutineRunner = CoroutineRunner.Create();
		}

		[TearDown]
		public void TearDown()
		{
			this.coroutineRunner.Destroy();
		}

		[Test]
		public void InitialProperties_AreAsExpected()
		{
			var yieldInstruction = Never.ToYieldInstruction(this.Executor);

			object unused;
			Assert.IsFalse(yieldInstruction.WasCanceled);
			Assert.IsFalse(yieldInstruction.CompletedWithError);
			Assert.IsFalse(yieldInstruction.CompletedSuccessfully);
			Assert.Throws<InvalidOperationException>(() => unused = yieldInstruction.Result);
			Assert.Throws<InvalidOperationException>(() => unused = yieldInstruction.Error);
		}

		[UnityTest]
		public IEnumerator ToYieldInstruction_CompletesAsExpected()
		{
			this.coroutineRunner.StartCoroutine(this.CompleteAfterOneFrame());

			var yieldInstruction = Responsibly
				.WaitForCondition("may complete", () => this.mayComplete)
				.ExpectWithinSeconds(1)
				.ToYieldInstruction(this.Executor);

			yield return yieldInstruction;

			// Completes one frame after
			Assert.AreEqual(this.completedOnFrame + 1, Time.frameCount);

			object unused;
			Assert.IsFalse(yieldInstruction.WasCanceled);
			Assert.IsFalse(yieldInstruction.CompletedWithError);
			Assert.IsTrue(yieldInstruction.CompletedSuccessfully);
			Assert.IsNotNull(yieldInstruction.Result);
			Assert.Throws<InvalidOperationException>(() => unused = yieldInstruction.Error);
		}

		[UnityTest]
		public IEnumerator ToYieldInstruction_CompletesAsExpected_WhenConstructedFromWait()
		{
			this.coroutineRunner.StartCoroutine(this.CompleteAfterOneFrame());

			var yieldInstruction = Responsibly
				.WaitForCondition("may complete", () => this.mayComplete)
				.CreateState()
				.ToYieldInstruction(this.Executor);

			yield return yieldInstruction;

			// Completes one frame "late", because of Update ordering
			Assert.AreEqual(this.completedOnFrame + 1, Time.frameCount);
			Assert.IsTrue(yieldInstruction.CompletedSuccessfully);
		}

		[UnityTest]
		public IEnumerator ToYieldInstruction_ErrorsAsExpected()
		{
			this.coroutineRunner.StartCoroutine(this.CompleteAfterOneFrame());

			var exception = new Exception("Test exception");
			var yieldInstruction = Responsibly
				.WaitForCondition("may complete", () => this.mayComplete)
				.Select<object, int>(_ => throw exception)
				.ExpectWithinSeconds(1)
				.ToYieldInstruction(this.Executor, false);

			yield return yieldInstruction;

			// Completes one frame "late", because of Update ordering
			Assert.AreEqual(this.completedOnFrame + 1, Time.frameCount);

			object unused;
			Assert.IsFalse(yieldInstruction.WasCanceled);
			Assert.IsTrue(yieldInstruction.CompletedWithError);
			Assert.IsFalse(yieldInstruction.CompletedSuccessfully);
			Assert.Throws<InvalidOperationException>(() => unused = yieldInstruction.Result);
			Assert.AreSame(exception, yieldInstruction.Error.InnerException);
		}

		[UnityTest]
		public IEnumerator ToYieldInstruction_CancelsAsExpected()
		{
			using (var cancellationSource = new CancellationTokenSource())
			{
				this.coroutineRunner.StartCoroutine(this.CancelAfterOneFrame(cancellationSource));
				var yieldInstruction = Never.ToYieldInstruction(this.Executor, false, cancellationSource.Token);

				yield return yieldInstruction;

				object unused;
				Assert.IsTrue(yieldInstruction.WasCanceled);
				Assert.IsFalse(yieldInstruction.CompletedWithError);
				Assert.IsFalse(yieldInstruction.CompletedSuccessfully);
				Assert.Throws<InvalidOperationException>(() => unused = yieldInstruction.Result);
				Assert.IsInstanceOf<TaskCanceledException>(yieldInstruction.Error.InnerException);
			}
		}

		[UnityTest]
		public IEnumerator ToYieldInstruction_Throws_WhenThrowOnErrorTrue()
		{
			this.coroutineRunner.StartCoroutine(this.CompleteAfterOneFrame());

			var exception = new Exception("Test exception");
			var yieldInstruction = Responsibly
				.WaitForCondition("may complete", () => this.mayComplete)
				.Select<object, int>(_ => throw exception)
				.ExpectWithinSeconds(1)
				.ToYieldInstruction(this.Executor, throwOnError: true);

			TestFailureException failureException;
			while (true)
			{
				try
				{
					yieldInstruction.MoveNext();
				}
				catch (TestFailureException e)
				{
					failureException = e;
					break;
				}

				yield return null;
				this.mayComplete = true;
			}

			Assert.AreEqual(exception, failureException.InnerException);
		}

		private IEnumerator CompleteAfterOneFrame()
		{
			yield return null;
			this.mayComplete = true;
			this.completedOnFrame = Time.frameCount;
		}

		private IEnumerator CancelAfterOneFrame(CancellationTokenSource cancellationSource)
		{
			yield return null;
			cancellationSource.Cancel();
		}
	}
}
