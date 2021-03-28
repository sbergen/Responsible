using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.State;
using Responsible.Utilities;

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

		private static readonly SafeIterationList<StateNotificationCallback> NotificationCallbacks =
			new SafeIterationList<StateNotificationCallback>();

		private readonly CancellationTokenSource mainCancellationTokenSource = new CancellationTokenSource();
		private readonly ITimeProvider timeProvider;
		private readonly IExternalResultSource externalResultSource;
		private readonly IFailureListener failureListener;

		public delegate void StateNotificationCallback(
			TestOperationStateTransition transitionType,
			ITestOperationState state);

		/// <summary>
		/// Will call <paramref name="callback"/> when operations start or finish.
		/// </summary>
		/// <param name="callback">Callback to call when an operation starts or finishes.</param>
		/// <returns>A disposable, which will remove the callback when disposed.</returns>
		/// <remarks>
		/// Static, so that Unity EditorWindows can access it.
		/// Used by the test operation window available at "Window/Responsible/Operation State".
		/// </remarks>
		public static IDisposable SubscribeToStates(StateNotificationCallback callback)
			=> NotificationCallbacks.Add(callback);

		/// <summary>
		/// Constructs a new test instruction executor.
		/// </summary>
		/// <param name="timeProvider">Implementation for counting time and frames.</param>
		/// <param name="externalResultSource">
		/// Optional source for premature completion of test operations.
		/// </param>
		/// <param name="failureListener">
		/// Optional failure listener, to get notifications on test operation failures.
		/// </param>
		public TestInstructionExecutor(
			ITimeProvider timeProvider,
			IExternalResultSource externalResultSource = null,
			IFailureListener failureListener = null)
		{
			this.timeProvider = timeProvider;
			this.externalResultSource = externalResultSource;
			this.failureListener = failureListener;
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
					NotificationCallbacks.ForEach(callback =>
						callback(TestOperationStateTransition.Started, rootState));

					if (this.externalResultSource != null)
					{
						return await linkedTokenSource.Token.Amb(
							this.externalResultSource.GetExternalResult<T>,
							ct => rootState.Execute(runContext, ct));
					}
					else
					{
						return await rootState.Execute(runContext, linkedTokenSource.Token);
					}
				}
				catch (Exception e)
				{
					var message = e is TimeoutException
						? MakeTimeoutMessage(rootState)
						: MakeErrorMessage(rootState, e);
					this.failureListener?.OperationFailed(e, message);
					throw new TestFailureException(message, e);
				}
				finally
				{
					NotificationCallbacks.ForEach(callback =>
						callback(TestOperationStateTransition.Finished, rootState));
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
	}
}
