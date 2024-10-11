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
	/// Handles execution of test instructions, external result sources, and failure notifications.
	/// </summary>
	/// <remarks>
	/// It is recommended to use a base class for your tests using Responsible,
	/// which sets up and disposes the test instruction executor.
	/// </remarks>
	[PublicAPI]
	public class TestInstructionExecutor : IDisposable
	{
		// Add spaces to lines so that the Unity console doesn't strip them
		private const string UnityEmptyLine = "\n \n";

		private static readonly SafeIterationList<StateNotificationCallback> NotificationCallbacks =
			new SafeIterationList<StateNotificationCallback>();

		private readonly CancellationTokenSource mainCancellationTokenSource = new CancellationTokenSource();
		private readonly ITestScheduler scheduler;
		private readonly IExternalResultSource externalResultSource;
		private readonly IFailureListener failureListener;
		private readonly IGlobalContextProvider globalContextProvider;
		private readonly IReadOnlyList<Type> rethrowableExceptions;

		/// <summary>
		/// Callback delegate type for test operation state notifications.
		/// </summary>
		/// <param name="transitionType">The type of the operation state transition.</param>
		/// <param name="state">The state of the operation.</param>
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
		/// Used by the test operation window available in Unity at Window/Responsible/Operation State.
		/// </remarks>
		public static IDisposable SubscribeToStates(StateNotificationCallback callback)
			=> NotificationCallbacks.Add(callback);

		/// <summary>
		/// Constructs a new test instruction executor.
		/// </summary>
		/// <param name="scheduler">Implementation for time and frame based operations.</param>
		/// <param name="externalResultSource">
		/// Optional source for premature completion of test operations.
		/// </param>
		/// <param name="failureListener">
		/// Optional failure listener, to get notifications on test operation failures.
		/// </param>
		/// <param name="globalContextProvider">
		/// Optional provider for global context, which gets included in failure messages.
		/// </param>
		/// <param name="rethrowableExceptions">
		/// Optional collection of exception types to rethrow, instead of wrapping them in
		/// <see cref="TestFailureException"/>.
		/// Test instructions terminating with any of the given exception types will be considered
		/// completed, and not failed. Can be used with e.g. NUnit's <c>IgnoreException</c>.
		/// </param>
		public TestInstructionExecutor(
			ITestScheduler scheduler,
			IExternalResultSource externalResultSource = null,
			IFailureListener failureListener = null,
			IGlobalContextProvider globalContextProvider = null,
			IReadOnlyList<Type> rethrowableExceptions = null)
		{
			this.scheduler = scheduler;
			this.externalResultSource = externalResultSource;
			this.failureListener = failureListener;
			this.globalContextProvider = globalContextProvider;
			this.rethrowableExceptions = rethrowableExceptions;
		}

		/// <summary>
		/// Disposes the executor.
		/// For extra safety, it is recommended to construct a new executor for each test,
		/// and dispose it after the test has finished.
		/// </summary>
		public virtual void Dispose()
		{
			// Ensure that nothing is left hanging.
			// If the executor is used sloppily, it's possible to otherwise
			// leave e.g. static Unity log event subscriptions hanging.
			if (!this.mainCancellationTokenSource.IsCancellationRequested)
			{
				this.mainCancellationTokenSource.Cancel();
				this.mainCancellationTokenSource.Dispose();
			}
		}

		internal async Task<T> RunInstruction<T>(
			ITestOperationState<T> rootState,
			SourceContext sourceContext,
			CancellationToken cancellationToken)
		{
			var runContext = new RunContext(sourceContext, this.scheduler);
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
				catch (Bdd.PendingStepException)
				{
					throw;
				}
				catch (Exception e)
				{
					if (this.ShouldRethrow(e))
					{
						throw;
					}

					await Task.Yield();

					var message = e is TimeoutException
						? this.MakeTimeoutMessage(rootState)
						: this.MakeErrorMessage(rootState, e);
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

		private bool ShouldRethrow(Exception e)
		{
			var exceptionType = e.GetType();
			return this.rethrowableExceptions?.Any(type => type == exceptionType) == true;
		}

		[Pure]
		private string MakeTimeoutMessage(ITestOperationState rootOperation)
			=> string.Join(
				UnityEmptyLine,
				FailureLines(rootOperation, "timed out")
					.Concat(this.MakeGlobalContext()));

		[Pure]
		private string MakeErrorMessage(
			ITestOperationState rootOperation,
			Exception exception)
			=> string.Join(
				UnityEmptyLine,
				FailureLines(rootOperation,"failed")
					.Concat(this.MakeGlobalContext())
					.Append($"Error: {exception}"));

		private IEnumerable<string> MakeGlobalContext()
		{
			if (this.globalContextProvider != null)
			{
				var builder = new StateStringBuilder();
				builder.AddNestedDetails("Global context:", this.globalContextProvider.BuildGlobalContext);
				yield return builder.ToString();
			}
		}

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
