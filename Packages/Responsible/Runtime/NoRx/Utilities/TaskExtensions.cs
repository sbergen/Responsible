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
					var ctsSource = new CancellationTokenSource();
					var task = factory(ctsSource.Token);
					return (ctsSource, task);
				})
				.ToList();

			using (cancellationToken.Register(() =>
			{
				foreach (var (ctsSource, _) in allTasks)
				{
					ctsSource.Cancel();
				}
			}))
			{
				var completedTask = await Task.WhenAny(allTasks.Select(data => data.task));

				foreach (var (ctsSource, task) in allTasks)
				{
					if (task != completedTask)
					{
						ctsSource.Cancel();
					}

					ctsSource.Dispose();
				}

				return await completedTask;
			}
		}
	}
}
