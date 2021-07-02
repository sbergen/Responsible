using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.Unity;
using UnityEngine;
using UnityEngine.TestTools;

namespace Responsible.UnityTests
{
	public class UnityErrorLogInterceptorTests
	{
		private const string ErrorMessage = "Test Error";

		private UnityErrorLogInterceptor errorInterceptor;
		private CancellationTokenSource cancellationSource;
		private Task<int> task;

		// JIC someone runs these together with their own test suite...
		private bool wereLogsIgnored;

		[SetUp]
		public void SetUp()
		{
			this.wereLogsIgnored = LogAssert.ignoreFailingMessages;
			LogAssert.ignoreFailingMessages = false;

			this.cancellationSource = new CancellationTokenSource();
			this.errorInterceptor = new UnityErrorLogInterceptor();
			this.task = this.errorInterceptor.GetExternalResult<int>(this.cancellationSource.Token);
		}

		[TearDown]
		public void TearDown()
		{
			this.cancellationSource.Cancel();
			this.cancellationSource.Dispose();
			LogAssert.ignoreFailingMessages = this.wereLogsIgnored;
		}

		[Test]
		public void ExceptionMessage_ContainsStackTrace()
		{
			Debug.LogError(ErrorMessage);
			var message = this.GetLogException()?.Message;
			Assert.IsNotNull(message);

			StringAssert.Contains(nameof(UnityErrorLogInterceptorTests), message);
		}

		[Test]
		public void LoggingError_CausesFailure_WhenErrorsNotIgnored()
		{
			Debug.LogError(ErrorMessage);
			Assert.IsNotNull(this.GetLogException());
		}

		[UnityTest]
		public IEnumerator LoggingError_EventuallyCausesFailure_WhenLoggedFromBackgroundThread()
		{
			var thread = new Thread(() => Debug.LogError("Test error"));
			thread.Start();

			while (thread.IsAlive)
			{
				yield return null;
			}

			// In order to dispatch the error back to the Unity thread, we need to yield at least one frame.
			// If this yield is not in place, cancellation will throw instead, see next test.
			yield return null;

			Assert.IsNotNull(this.GetLogException());
		}

		[Test]
		public void CancelingSubscription_ThrowsError_WhenThreadedExceptionWasNotWaitedFor()
		{
			var thread = new Thread(() => Debug.LogError("Test error"));
			thread.Start();
			thread.Join(TimeSpan.FromSeconds(1));

			var aggregateException = Assert.Throws<AggregateException>(this.cancellationSource.Cancel);
			Assert.IsInstanceOf<UnhandledLogMessageException>(aggregateException.InnerException);
		}

		[Test]
		public void LoggingError_DoesNotCauseFailure_WhenErrorIgnored()
		{
			LogAssert.ignoreFailingMessages = true;
			Debug.LogError(ErrorMessage);
			Assert.IsNull(this.GetLogException());
		}

		[Test]
		public void ExpectLog_Works_WhenErrorsNotIgnored()
		{
			this.errorInterceptor.ExpectLog(LogType.Error, new Regex(ErrorMessage));
			Debug.LogError(ErrorMessage);
			Assert.IsNull(this.task.Exception);
		}

		[Test]
		public void LogAssert_Works_WhenErrorIsNotIntercepted()
		{
			LogAssert.ignoreFailingMessages = true;
			Debug.LogError(ErrorMessage);
			LogAssert.Expect(LogType.Error, ErrorMessage);
		}

		// Should fail, can't assert that with the Unity test runner, run manually
		/*
		[Test]
		public void NotLoggedButExpectedError_FailsTest()
		{
			this.errorInterceptor.ExpectLog(LogType.Error, new Regex("foo"));
		}
		*/

		private UnhandledLogMessageException GetLogException()
			=> this.task.Exception?.InnerExceptions.SingleOrDefault() as UnhandledLogMessageException;
	}
}
