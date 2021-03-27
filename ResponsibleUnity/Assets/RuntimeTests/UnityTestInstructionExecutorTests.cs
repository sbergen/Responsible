using System;
using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Responsible.Unity;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Responsible.UnityTests
{
	public class UnityTestInstructionExecutorTests
	{
		[Test]
		public void Errors_AreLogged_WhenLogErrorsIsTrue()
		{
			using (var executor = new UnityTestInstructionExecutor(logErrors: true))
			{
				var message = "Should be in log";
				Responsibly
					.Do("Throw exception", () => throw new Exception(message))
					.ToYieldInstruction(executor, throwOnError: false); // Should complete synchronously
				LogAssert.Expect(LogType.Error, new Regex(message));
			}
		}

		[Test]
		public void UnhandledErrorLog_IsLoggedAsWarning_WhenLogErrorsIsTrue()
		{
			using (var executor = new UnityTestInstructionExecutor(logErrors: true))
			{
				var expected = "expected message";
				Responsibly
					.Do("Throw exception", () => Debug.LogError(expected))
					.ToYieldInstruction(executor, throwOnError: false); // Should complete synchronously

				LogAssert.Expect(LogType.Warning, new Regex(expected)); // The one from us
			}
		}

		[Test]
		public void Errors_AreNotLogged_WhenLogErrorsIsFalse()
		{
			using (var executor = new UnityTestInstructionExecutor(logErrors: false))
			{
				var instruction = Responsibly
					.Do("Throw exception", () => throw new Exception())
					.ToYieldInstruction(executor, throwOnError: false);
				Assert.IsTrue(instruction.CompletedSuccessfully);
				// Should not fail the test with logged errors
			}
		}

		[UnityTest]
		public IEnumerator Polling_HappensOncePerFrame()
		{
			using (var executor = new UnityTestInstructionExecutor(logErrors: false))
			{
				var pollCount = 0;

				Responsibly
					.WaitForCondition("Never", () =>
					{
						++pollCount;
						return false;
					})
					.ExpectWithinSeconds(1)
					.ToYieldInstruction(executor);

				Assert.AreEqual(1, pollCount, "Should poll once synchronously");

				yield return null;
				Assert.AreEqual(2, pollCount, "Should poll once per frame");

				yield return null;
				Assert.AreEqual(3, pollCount, "Should poll once per frame");
			}
		}

		[UnityTest]
		public IEnumerator Polling_IsStopped_WhenDisposed()
		{
			var pollCount = 0;

			using (var executor = new UnityTestInstructionExecutor(logErrors: false))
			{
				Responsibly
					.WaitForCondition("Never", () =>
					{
						++pollCount;
						return false;
					})
					.ExpectWithinSeconds(1)
					.ToYieldInstruction(executor);
			}

			Assert.AreEqual(1, pollCount, "Should poll once synchronously");

			yield return null;
			Assert.AreEqual(1, pollCount, "Should not poll after being disposed");
		}

		[Test]
		public void LoggingError_CausesFailureSynchronously()
		{
			using (var executor = new UnityTestInstructionExecutor(logErrors: false))
			{
				var yieldInstruction = Responsibly
					.WaitForCondition("Never", () => false)
					.ExpectWithinSeconds(1)
					.ToYieldInstruction(executor);

				Debug.LogError("Should fail the instruction");

				Assert.IsTrue(yieldInstruction.CompletedWithError);
			}
		}

		[Test]
		public void LoggingError_DoesNotFail_WhenUsingExpectLog()
		{
			using (var executor = new UnityTestInstructionExecutor(logErrors: false))
			{
				var yieldInstruction = Responsibly
					.WaitForCondition("Never", () => false)
					.ExpectWithinSeconds(1)
					.ToYieldInstruction(executor);

				var message = "Should not fail the instruction";
				executor.ExpectLog(LogType.Error, new Regex(message));
				Debug.LogError(message);

				Assert.IsFalse(yieldInstruction.WasCanceled);
				Assert.IsFalse(yieldInstruction.CompletedSuccessfully);
				Assert.IsFalse(yieldInstruction.CompletedWithError);
			}
		}

		[UnityTest]
		public IEnumerator Dispose_CleansUpAllCreatedComponents()
		{
			int GetComponentCount() => Object.FindObjectsOfType(typeof(MonoBehaviour)).Length;
			var componentCountAtStart = GetComponentCount();

			using (new UnityTestInstructionExecutor())
			{
				yield return null;
				Assert.Greater(GetComponentCount(), componentCountAtStart);
			}

			yield return null;
			Assert.AreEqual(componentCountAtStart, GetComponentCount());
		}
	}
}
