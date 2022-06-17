// Unity 2021 cancellation can be asynchronous, so we need to do some extra checks.
// The checks can never be reached in .NET and older Unity versions,
// so branch coverage suffers if we always do the checks (even though they are safe).
// To maintain 100% coverage (as I want to see if I'm missing branches),
// enable these checks only in newer Unity versions.
#if UNITY_2021_1_OR_NEWER
#define ASYNC_CANCELLATION
#endif

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Responsible.Utilities
{
	internal static class SchedulerExtensions
	{
		public static async Task<T> PollForCondition<T>(
			this ITestScheduler scheduler,
			Func<T> getState,
			Func<T, bool> condition,
			CancellationToken cancellationToken)
		{
			var tcs = new TaskCompletionSource<T>();

			void CheckCondition()
			{
				try
				{
#if ASYNC_CANCELLATION
					if (!cancellationToken.IsCancellationRequested)
#endif
					{
						var state = getState();
						if (condition(state))
						{
							tcs.SetResult(state);
						}
					}
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			}

			using (scheduler.RegisterPollCallback(CheckCondition))
			using (cancellationToken.Register(tcs.SetCanceled))
			{
				// Might be cancelled before execution
				if (!cancellationToken.IsCancellationRequested)
				{
					CheckCondition(); // Complete immediately if already met
				}

				return await tcs.Task;
			}
		}

		public static Task<T> TimeoutAfter<T>(
			this ITestScheduler testScheduler,
			TimeSpan timeout,
			CancellationToken cancellationToken,
			DeferredTask<T> deferredTask)
			=> cancellationToken.Amb(
				deferredTask,
				ct => testScheduler.Timeout(timeout, ct).ThrowResult<T>());

		private static async Task<Exception> Timeout(
			this ITestScheduler scheduler,
			TimeSpan timeout,
			CancellationToken cancellationToken)
		{
			var deadline = scheduler.TimeNow + timeout;
			var completionSource = new TaskCompletionSource<object>();

			void CheckTimeout()
			{
				if (
#if ASYNC_CANCELLATION
					!cancellationToken.IsCancellationRequested &&
#endif
					scheduler.TimeNow >= deadline)
				{
					completionSource.SetException(new TimeoutException());
				}
			}

			using (scheduler.RegisterPollCallback(CheckTimeout))
			using (cancellationToken.Register(completionSource.SetCanceled))
			{
				return await completionSource.Task.ExpectException();
			}
		}
	}
}
