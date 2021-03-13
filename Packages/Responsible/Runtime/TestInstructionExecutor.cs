using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using NUnit.Framework;
using Responsible.Context;
using Responsible.State;
using UniRx;
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

		private static readonly Subject<TestOperationStateNotification> StateNotificationsSubject =
			new Subject<TestOperationStateNotification>();

		private readonly List<(LogType type, Regex regex)> expectedLogs = new List<(LogType, Regex)>();
		private readonly IDisposable pollSubscription;
		private readonly ILogger logger;
		private readonly IScheduler scheduler;
		private readonly IObservable<Unit> pollObservable;

		/// <summary>Notifications for started and finished test operations.</summary>
		/// <value>Observable for test operation state notification.</value>
		/// <remarks>
		/// Static, so that Unity EditorWindows can access it.
		/// Used by the test operation window available at "Window/Responsible/Operation State".
		/// </remarks>
		public static IObservable<TestOperationStateNotification> StateNotifications => StateNotificationsSubject;

		/// <summary>
		/// Constructs a new test instruction executor.
		/// </summary>
		/// <param name="scheduler">
		/// Optional scheduler override. <see cref="Scheduler.MainThread"/> is used by default.
		/// </param>
		/// <param name="pollObservable">
		/// Optional poll observable override. <see cref="Observable.EveryUpdate"/> is used by default.
		/// </param>
		/// <param name="logger">
		/// Optional logger override. <see cref="Debug.unityLogger"/> is used by default.
		/// </param>
		public TestInstructionExecutor(
			IScheduler scheduler = null,
			IObservable<Unit> pollObservable = null,
			ILogger logger = null)
		{
			this.scheduler = scheduler ?? Scheduler.MainThread;
			this.logger = logger ?? Debug.unityLogger;
			pollObservable = pollObservable ?? Observable.EveryUpdate().AsUnitObservable();

			// Workaround for how EveryUpdate works in Unity.
			// When nobody is subscribed to it, there will be a one-frame delay on the next Subscribe.
			var pollSubject = new Subject<Unit>();
			this.pollObservable = pollSubject;
			this.pollSubscription = pollObservable.Subscribe(pollSubject);
		}

		/// <summary>
		/// Disposes the executor. Should be called after finishing with tests.
		/// </summary>
		public void Dispose()
		{
			this.pollSubscription.Dispose();
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

		internal IObservable<T> RunInstruction<T>(
			ITestOperationState<T> rootState,
			SourceContext sourceContext)
		{
			var runContext = new RunContext(sourceContext, this.scheduler, this.pollObservable);
			return Observable
				.Amb(
					this.InterceptErrors<T>(),
					rootState.Execute(runContext))
				.Catch((Exception e) =>
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

					return Observable.Throw<T>(new AssertionException(message, e));
				})
				.DoOnSubscribe(() => StateNotificationsSubject.OnNext(
					new TestOperationStateNotification.Started(rootState)))
				.Finally(() => StateNotificationsSubject.OnNext(
					new TestOperationStateNotification.Finished(rootState)));
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
		[Pure]
		private IObservable<T> InterceptErrors<T>() => Observable
			.FromEvent<Application.LogCallback, (string condition, string stackTrace, LogType logType)>(
				action => (condition, stackTrace, type) => action((condition, stackTrace, type)),
				handler => Application.logMessageReceived += handler,
				handler => Application.logMessageReceived -= handler)
			.Where(data =>
				!LogAssert.ignoreFailingMessages &&
				data.logType != LogType.Log && data.logType != LogType.Warning)
			// Side-effects below!
			.SelectMany(data =>
			{
				var index = this.expectedLogs.FindIndex(entry =>
					entry.type == data.logType &&
					entry.regex.IsMatch(data.condition));
				if (index >= 0)
				{
					// Already expected, just remove it
					this.expectedLogs.RemoveAt(index);
					return Observable.Empty<T>();
				}
				else
				{
					LogAssert.Expect(data.logType, data.condition);
					return Observable.Throw<T>(new UnhandledLogMessageException(data.condition));
				}
			});
	}
}
