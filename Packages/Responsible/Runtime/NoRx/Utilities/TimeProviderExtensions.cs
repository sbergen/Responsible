using System;
using System.Threading;
using System.Threading.Tasks;

namespace Responsible.NoRx.Utilities
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

		public static async Task<T> TimeoutAfter<T>(
			this ITimeProvider timeProvider,
			TimeSpan timeout,
			CancellationToken cancellationToken,
			Func<CancellationToken, Task<T>> taskFactory)
			=> await new[]
			{
				taskFactory,
				ct => timeProvider.Timeout<T>(timeout, ct)
			}.Amb(cancellationToken);

		// T is for convenience
		public static async Task<T> Timeout<T>(
			this ITimeProvider timeProvider,
			TimeSpan timeout,
			CancellationToken cancellationToken)
		{
			var deadline = timeProvider.TimeNow + timeout;
			var completionSource = new TaskCompletionSource<T>();

			void CheckTimeout()
			{
				if (timeProvider.TimeNow >= deadline)
				{
					completionSource.SetException(new TimeoutException());
				}
			}

			using var pollHandle = timeProvider.RegisterPollCallback(CheckTimeout);
			using (cancellationToken.Register(pollHandle.Dispose))
			{
				return await completionSource.Task;
			}
		}
	}
}
