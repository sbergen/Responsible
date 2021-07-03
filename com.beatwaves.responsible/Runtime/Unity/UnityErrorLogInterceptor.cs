using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Utilities;
using UnityEngine;
using UnityEngine.TestTools;

namespace Responsible.Unity
{
	/// <summary>
	/// Class used to replicate the Unity test runner behaviour of failing tests when either
	/// <see cref="Debug.LogError(object)"/> or <see cref="Debug.LogException(System.Exception)"/> is called.
	/// </summary>
	public class UnityErrorLogInterceptor : IExternalResultSource
	{
		private readonly List<(LogType type, Regex regex)> expectedLogs = new List<(LogType, Regex)>();

		/// <summary>
		/// Expect a log message during execution of any subsequent test instructions.
		/// Similar to <see cref="LogAssert.Expect(LogType, Regex)"/>,
		/// but needs to be called instead of that, in order to work when executing test instructions.
		/// </summary>
		/// <remarks>
		/// Will call <see cref="LogAssert.Expect(LogType, Regex)"/>, so it doesn't need to be called separately.
		/// </remarks>
		/// <param name="logType">Log type to expect.</param>
		/// <param name="regex">Regex to match the message to be expected.</param>
		public void ExpectLog(LogType logType, Regex regex)
		{
			LogAssert.Expect(logType, regex);

			lock (this.expectedLogs)
			{
				this.expectedLogs.Add((logType, regex));
			}
		}

		/// <summary>
		/// Intercept errors, respecting and not toggling the globally mutable
		/// <see cref="LogAssert.ignoreFailingMessages"/>.
		/// Will complete with an error if either an error or exception is logged,
		/// and will never complete if neither of those happens.
		/// </summary>
		/// <param name="cancellationToken">Token used to cancel this operation.</param>
		/// <typeparam name="T">Type of the test operation being run.</typeparam>
		/// <returns>Task, which will complete with an error if either an error or exception is logged.</returns>
		public Task<T> GetExternalResult<T>(CancellationToken cancellationToken)
			=> this.InterceptLoggedErrors(cancellationToken).ThrowResult<T>();

		private async Task<Exception> InterceptLoggedErrors(CancellationToken cancellationToken)
		{
			var completionSource = new TaskCompletionSource<object>();

			void HandleSingleLog(string condition, string stackTrace, LogType type)
			{
				lock (this.expectedLogs)
				{
					var index = this.expectedLogs.FindIndex(entry =>
						entry.type == type &&
						entry.regex.IsMatch(condition));

					if (index >= 0)
					{
						// Already expected, just remove it
						this.expectedLogs.RemoveAt(index);
					}
					else
					{
						LogAssert.Expect(type, condition);
						completionSource.SetException(new UnhandledLogMessageException(condition, stackTrace));
					}
				}
			}

			void LogHandler(string condition, string stackTrace, LogType type)
			{
				if (!cancellationToken.IsCancellationRequested &&
					!LogAssert.ignoreFailingMessages &&
					(type == LogType.Error || type == LogType.Exception))
				{
					HandleSingleLog(condition, stackTrace, type);
				}
			}

			using (cancellationToken.Register(CancelOrThrow(completionSource)))
			{
				Application.logMessageReceivedThreaded += LogHandler;
				try
				{
					return await completionSource.Task.ExpectException();
				}
				finally
				{
					Application.logMessageReceivedThreaded -= LogHandler;
				}
			}
		}

		[ExcludeFromCoverage] // Contains only defensive conditions, which can't be triggered
		private static Action CancelOrThrow(TaskCompletionSource<object> completionSource) => () =>
		{
			if (!completionSource.TrySetCanceled())
			{
				var exceptionCandidate = completionSource.Task.Exception?.InnerException;
				if (exceptionCandidate is UnhandledLogMessageException)
				{
					throw exceptionCandidate;
				}
				else
				{
					throw new InvalidOperationException(
						"Error log interception task was completed in an unexpected way");
				}
			}
		};
	}
}
