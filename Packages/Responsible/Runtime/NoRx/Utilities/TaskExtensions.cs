using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Responsible.Utilities
{
	internal static class TaskExtensions
	{
		public static async Task<T> TimeoutAfter<T>(
			this Task<T> task,
			TimeSpan timeout,
			CancellationToken cancellationToken)
		{
			// Cancel timeout task when either the whole task is canceled, completed or faulted
			using var timeoutCtsSource = new CancellationTokenSource();
			using var _ = cancellationToken.Register(timeoutCtsSource.Cancel);

			if (task == await Task.WhenAny(task, Task.Delay(timeout, timeoutCtsSource.Token)))
			{
				timeoutCtsSource.Cancel();
				return await task;
			}
			else
			{
				throw new TimeoutException();
			}
		}
	}
}
