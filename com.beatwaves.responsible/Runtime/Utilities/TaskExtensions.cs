using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Responsible.Utilities
{
	internal static class TaskExtensions
	{
		/// <summary>
		/// Returns the result of the first completed task, and cancels the rest.
		/// Cancels all if <paramref name="cancellationToken"/> is canceled.
		/// </summary>
		public static async Task<T> Amb<T>(
			this CancellationToken cancellationToken,
			params DeferredTask<T>[] deferredTasks)
		{
			var allTasks = deferredTasks
				.Select(factory =>
				{
					var ctsSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
					var task = factory(ctsSource.Token);
					return (ctsSource, task);
				})
				.ToList();

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

		public static async Task<T> CancelOnError<T>(
			this Task<T> task,
			CancellationTokenSource cancellationTokenSource)
		{
			try
			{
				return await task;
			}
			catch (Exception e) when (!(e is OperationCanceledException))
			{
				cancellationTokenSource.Cancel();
				throw;
			}
		}

		public static async Task<T> CancelOnTermination<T>(
			this Task<T> task,
			CancellationTokenSource cancellationTokenSource)
		{
			try
			{
				return await task;
			}
			finally
			{
				cancellationTokenSource.Cancel();
			}
		}

		public static async Task<T> ThrowResult<T>(this Task<Exception> task)
		{
			throw await task;
		}

		// Get 100% coverage with tasks that never return a result.
		// Also, adds a bit of safety!
		[ExcludeFromCodeCoverage]
		public static async Task<Exception> ExpectException(this Task task)
		{
			try
			{
				await task;
			}
			catch (Exception e)
			{
				return e;
			}

			throw new InvalidOperationException(
				"Task completed successfully when exception was expected!");
		}
	}
}
