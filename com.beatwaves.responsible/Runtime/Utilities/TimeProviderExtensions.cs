using System;
using System.Threading;
using System.Threading.Tasks;

namespace Responsible.Utilities
{
	internal static class TimeProviderExtensions
	{
		public static async Task<T> PollForCondition<T>(
			this ITimeProvider timeProvider,
			Func<T> getState,
			Func<T, bool> condition,
			CancellationToken cancellationToken)
		{
			var tcs = new TaskCompletionSource<T>();

			void CheckCondition()
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return;
				}

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

			using (timeProvider.RegisterPollCallback(CheckCondition))
			using (cancellationToken.Register(tcs.SetCanceled))
			{
				CheckCondition(); // Complete immediately if already met
				return await tcs.Task;
			}
		}

		public static Task<T> TimeoutAfter<T>(
			this ITimeProvider timeProvider,
			TimeSpan timeout,
			CancellationToken cancellationToken,
			DeferredTask<T> deferredTask)
			=> cancellationToken.Amb(
				deferredTask,
				ct => timeProvider.Timeout(timeout, ct).ThrowResult<T>());

		private static async Task<Exception> Timeout(
			this ITimeProvider timeProvider,
			TimeSpan timeout,
			CancellationToken cancellationToken)
		{
			var deadline = timeProvider.TimeNow + timeout;
			var completionSource = new TaskCompletionSource<object>();

			void CheckTimeout()
			{
				if (timeProvider.TimeNow >= deadline)
				{
					completionSource.SetException(new TimeoutException());
				}
			}

			using (timeProvider.RegisterPollCallback(CheckTimeout))
			using (cancellationToken.Register(completionSource.SetCanceled))
			{
				return await completionSource.Task.ExpectException();
			}
		}
	}
}
