using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Responsible.NoRx;
using UnityEngine;
using UnityEngine.TestTools;
using static Responsible.NoRx.Responsibly;

namespace Responsible.Tests.Runtime.NoRx
{
	public class ErrorLogHandlingTests : ResponsibleTestBase
	{
		private const string ErrorMessage = "Error!";

		// JIC someone runs these together with their test suite...
		private bool wereLogsIgnored;

		[SetUp]
		public void SetUp()
		{
			this.wereLogsIgnored = LogAssert.ignoreFailingMessages;
			LogAssert.ignoreFailingMessages = false;
		}

		[TearDown]
		public void TearDown()
		{
			LogAssert.ignoreFailingMessages = this.wereLogsIgnored;
		}

		[Test]
		public void LoggingError_CausesProperFailure_WhenErrorIsIntercepted()
		{
			var task = this.LogErrorFromInstructionSynchronously();

			var exception = GetAssertionException(task);
			Assert.IsInstanceOf<UnhandledLogMessageException>(exception.InnerException);
		}

		[Test]
		public void LoggingError_DoesNotAlsoLogWarning_WhenTestTerminatesWithException()
		{
			Do("Throw exception", () => throw new Exception())
				.ToTask(this.Executor);

			this.Logger.DidNotReceive().Log(LogType.Warning, Arg.Any<string>());
			this.Logger.Received(1).Log(LogType.Error, Arg.Any<string>());
		}

		[Test]
		public void LoggingError_DoesNotCauseFailure_WhenErrorIsNotIntercepted()
		{
			LogAssert.ignoreFailingMessages = true;
			var task = this.LogErrorFromInstructionSynchronously();
			Assert.IsFalse(task.IsFaulted);
		}

		[Test]
		public void LoggingError_LogsDetailsAsWarning()
		{
			this.LogErrorFromInstructionSynchronously();
			this.Logger.Received().Log(
				LogType.Warning,
				Arg.Is<string>(msg => msg.Contains("Failure context")));
		}

		[Test]
		public void ExpectLog_Works_WhenErrorIsIntercepted()
		{
			this.Executor.ExpectLog(LogType.Error, new Regex(ErrorMessage));
			this.LogErrorFromInstructionSynchronously();
		}

		[Test]
		public void LogAssert_Works_WhenErrorIsNotIntercepted()
		{
			LogAssert.ignoreFailingMessages = true;
			this.LogErrorFromInstructionSynchronously();
			LogAssert.Expect(LogType.Error, ErrorMessage);
		}

		[Test]
		[Ignore("Should fail, can't assert that with the Unity test runner, run manually")]
		public void NotLoggedButExpectedError_FailsTest()
		{
			this.Executor.ExpectLog(LogType.Error, new Regex("foo"));
		}

		private Task LogErrorFromInstructionSynchronously()
		{
			return Do("Log error", () => Debug.LogError(ErrorMessage))
				.ToTask(this.Executor);
		}
	}
}
