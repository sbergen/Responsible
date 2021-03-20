using System;
using System.Threading;
using System.Threading.Tasks;

namespace Responsible.NoRx.Utilities
{
	internal static class TimeProviderExtensions
	{
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
