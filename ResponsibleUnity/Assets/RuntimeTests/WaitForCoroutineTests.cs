using System;
using System.Collections;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Responsible.UnityTests
{
	public class WaitForCoroutineTests : UnityTestBase
	{
		private bool coroutineCompleted;
		private bool coroutineRan;
		private Exception testException = new Exception("Test exception");

		[SetUp]
		public void SetUp()
		{
			this.coroutineCompleted = false;
			this.coroutineRan = false;
		}

		[UnityTest]
		public IEnumerator WaitForCoroutine_CompletesWithCoroutine()
		{
			var startFrame = Time.frameCount;

			yield return Responsibly
				.WaitForCoroutine("Coroutine", this.CompleteAfterOneFrame)
				.ExpectWithinSeconds(1)
				.ToYieldInstruction(this.Executor);

			Assert.AreEqual(
				1,
				Time.frameCount - startFrame,
				"Coroutine should complete after one frame");
			Assert.IsTrue(this.coroutineCompleted);
		}

		[UnityTest]
		public IEnumerator WaitForCoroutine_CompletesWithError_WhenCoroutineThrows()
		{
			var instruction = Responsibly
				.WaitForCoroutine("Throw", this.ThrowAfterOneFrame)
				.ExpectWithinSeconds(1)
				.ToYieldInstruction(this.Executor, throwOnError: false);

			yield return instruction;

			Assert.IsTrue(instruction.CompletedWithError);
			Assert.AreSame(this.testException, instruction.Error.InnerException);
		}

		[UnityTest]
		public IEnumerator WaitForCoroutine_CancelsCoroutine_WhenOperationCanceled()
		{
			using (var cancellationSource = new CancellationTokenSource())
			{
				var instruction = Responsibly
					.WaitForCoroutine("Forever", this.Forever)
					.ExpectWithinSeconds(1)
					.ToYieldInstruction(this.Executor, throwOnError: false, cancellationSource.Token);

				Assert.IsTrue(this.coroutineRan);
				yield return null;

				this.coroutineRan = false;
				cancellationSource.Cancel();

				yield return null;
				yield return null;
				Assert.IsFalse(this.coroutineRan);
				Assert.IsTrue(instruction.WasCanceled);
			}
		}

		[Test]
		public void WaitForCoroutine_ContainsCorrectDescription()
		{
			var instruction = Responsibly
				.WaitForCoroutine("Manual", this.ThrowImmediately)
				.ExpectWithinSeconds(1)
				.ToYieldInstruction(this.Executor);

			Assert.IsTrue(instruction.CompletedWithError);
			StringAssert.Contains("Manual (Coroutine)", instruction.Error.Message);
		}

		[Test]
		public void WaitForCoroutineMethod_ContainsCorrectDescription()
		{
			var instruction = Responsibly
				.WaitForCoroutineMethod(this.ThrowImmediately)
				.ExpectWithinSeconds(1)
				.ToYieldInstruction(this.Executor);

			Assert.IsTrue(instruction.CompletedWithError);
			StringAssert.Contains("ThrowImmediately (Coroutine)", instruction.Error.Message);
		}

		private IEnumerator CompleteAfterOneFrame()
		{
			yield return null;
			this.coroutineCompleted = true;
		}

		private IEnumerator ThrowAfterOneFrame()
		{
			yield return null;
			throw this.testException;
		}

		private IEnumerator ThrowImmediately()
		{
			throw this.testException;
		}

		private IEnumerator Forever()
		{
			while (true)
			{
				this.coroutineRan = true;
				yield return null;
			}
		}
	}
}
