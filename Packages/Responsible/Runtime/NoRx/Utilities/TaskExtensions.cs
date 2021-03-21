using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Responsible.NoRx.Utilities
{
	internal static class TaskExtensions
	{
		public static async Task<T> Amb<T>(
			this IEnumerable<Func<CancellationToken, Task<T>>> taskFactories,
			CancellationToken cancellationToken)
		{
			var allTasks = taskFactories
				.Select(factory =>
				{
					var ctsSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
					var task = factory(ctsSource.Token);
					return (ctsSource, task);
				})
				.ToList();

			var completedTask = await Task.WhenAny(allTasks.Select(data => data.task));

			if (!cancellationToken.IsCancellationRequested)
			{
				foreach (var (ctsSource, task) in allTasks)
				{
					if (task != completedTask)
					{
						ctsSource.Cancel();
					}

					ctsSource.Dispose();
				}
			}

			return await completedTask;
		}

		public static Task<T> CancelOnCompletion<T>(
			this Task<T> task,
			CancellationTokenSource cancellationTokenSource,
			CancellationToken cancellationToken) =>
			task.ContinueWith(resultTask =>
				{
					cancellationTokenSource.Cancel();
					return resultTask.Result;
				},
				cancellationToken,
				TaskContinuationOptions.ExecuteSynchronously,
				TaskScheduler.FromCurrentSynchronizationContext());
	}
}
