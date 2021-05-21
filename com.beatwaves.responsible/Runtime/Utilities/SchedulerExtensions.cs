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
					var state = getState();
					if (condition(state))
					{
						tcs.SetResult(state);
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
				CheckCondition(); // Complete immediately if already met
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
				if (scheduler.TimeNow >= deadline)
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
