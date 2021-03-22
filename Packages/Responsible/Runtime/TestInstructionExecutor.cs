using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using Responsible.Context;
using Responsible.State;
using Responsible.Utilities;
using UnityEngine;
using UnityEngine.TestTools;

namespace Responsible
{
	/// <summary>
	/// Handles execution of test instruction and intercepting logged errors during test execution.
	/// </summary>
	/// <remarks>
	/// It is recommended to use a base class for your tests using Responsible,
	/// which sets up and disposes the test instruction executor.
	/// </remarks>
	public class TestInstructionExecutor : IDisposable
	{
		// Add spaces to lines so that the Unity console doesn't strip them
		private const string UnityEmptyLine = "\n \n";

		private static readonly SafeIterationList<Action<TestOperationStateNotification>> NotificationCallbacks =
			new SafeIterationList<Action<TestOperationStateNotification>>();

		private readonly CancellationTokenSource mainCancellationTokenSource = new CancellationTokenSource();
		private readonly List<(LogType type, Regex regex)> expectedLogs = new List<(LogType, Regex)>();
		private readonly ILogger logger;
		private readonly ITimeProvider timeProvider;

		/// <summary>
		/// Will call <paramref name="callback"/> when operations start or finish.
		/// </summary>
		/// <param name="callback">Callback to call when an operation starts or finishes.</param>
		/// <returns>A disposable, which will remove the callback when disposed.</returns>
		/// <remarks>
		/// Static, so that Unity EditorWindows can access it.
		/// Used by the test operation window available at "Window/Responsible/Operation State".
		/// </remarks>
		public static IDisposable SubscribeToStates(Action<TestOperationStateNotification> callback)
			=> NotificationCallbacks.Add(callback);

		/// <summary>
		/// Constructs a new test instruction executor.
		/// </summary>
		/// <param name="timeProvider">Implementation for counting time and frames.</param>
		/// <param name="logger">
		/// Optional logger override. <see cref="Debug.unityLogger"/> is used by default.
		/// </param>
		public TestInstructionExecutor(
			ITimeProvider timeProvider,
			ILogger logger = null)
		{
			this.timeProvider = timeProvider;
			this.logger = logger ?? Debug.unityLogger;
		}

		/// <summary>
		/// Disposes the executor. Should be called after finishing with tests.
		/// </summary>
		public virtual void Dispose()
		{
			// Ensure that nothing is left hanging.
			// If the executor is used sloppily, it's possible to otherwise
			// leave e.g. static Unity log event subscriptions hanging.
			this.mainCancellationTokenSource.Cancel();
			this.mainCancellationTokenSource.Dispose();
		}

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
			this.expectedLogs.Add((logType, regex));
		}

		internal async Task<T> RunInstruction<T>(
			ITestOperationState<T> rootState,
			SourceContext sourceContext,
			CancellationToken cancellationToken)
		{
			var runContext = new RunContext(sourceContext, this.timeProvider);
			using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
				cancellationToken, this.mainCancellationTokenSource.Token))
			{
				try
				{
					NotificationCallbacks.ForEach(cb =>
						cb(new TestOperationStateNotification.Started(rootState)));

					return await linkedTokenSource.Token.Amb(
						this.InterceptErrors<T>,
						ct => rootState.Execute(runContext, ct));
				}
				catch (Exception e)
				{
					var message = e is TimeoutException
						? MakeTimeoutMessage(rootState)
						: MakeErrorMessage(rootState, e);

					// The Unity test runner can swallow exceptions, so both log an error and throw an exception.
					// For already logged errors, log this as a warning, as it contains extra context.
					var logType = e is UnhandledLogMessageException
						? LogType.Warning
						: LogType.Error;
					this.logger.Log(logType, message);

					throw new AssertionException(message, e);
				}
				finally
				{
					NotificationCallbacks.ForEach(cb =>
						cb(new TestOperationStateNotification.Finished(rootState)));
				}
			}
		}

		[Pure]
		private static string MakeTimeoutMessage<T>(ITestOperationState<T> rootOperation)
			=> string.Join(UnityEmptyLine, FailureLines(rootOperation, "timed out"));

		[Pure]
		private static string MakeErrorMessage<T>(
			ITestOperationState<T> rootOperation,
			Exception exception)
			=> string.Join(
				UnityEmptyLine,
				FailureLines(rootOperation,"failed").Append($"Error: {exception}"));

		[Pure]
		private static IEnumerable<string> FailureLines(
			ITestOperationState rootOperation,
			string what)
			=> new[]
			{
				$"Test operation execution {what}!",
				$"Failure context:\n{StateStringBuilder.MakeState(rootOperation)}",
			};

		// Intercept errors respecting and not toggling the globally mutable (yuck) LogAssert.ignoreFailingMessages
		private async Task<T> InterceptErrors<T>(CancellationToken token)
		{
			var completionSource = new TaskCompletionSource<T>();

			void HandleSingleLog(string condition, LogType type)
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
					completionSource.SetException(new UnhandledLogMessageException(condition));
				}
			}

			void LogHandler(string condition, string _, LogType type)
			{
				if (!LogAssert.ignoreFailingMessages &&
					(type == LogType.Error || type == LogType.Exception))
				{
					HandleSingleLog(condition, type);
				}
			}

			using (token.Register(() => completionSource.SetCanceled()))
			{
				Application.logMessageReceived += LogHandler;
				try
				{
					return await completionSource.Task;
				}
				finally
				{
					Application.logMessageReceived -= LogHandler;
				}
			}
		}
	}
}
