using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Responsible.Utilities
{
	internal static class MultipleTaskAwaiter
	{
		public static IMultipleTaskAwaiter<T> Make<T>(IEnumerable<Task<T>> tasks) =>
			new MultipleTaskAwaiter<T>(tasks);
	}

	internal class MultipleTaskAwaiter<T> : IMultipleTaskAwaiter<T>
	{
		private readonly List<Task<T>> tasks;

		public bool HasNext => this.tasks.Count > 0;

		public MultipleTaskAwaiter(IEnumerable<Task<T>> tasks)
		{
			this.tasks = tasks.ToList();
		}

		public async Task<T> AwaitNext()
		{
			this.AssertTasksAvailable();
			var completedTask = await Task.WhenAny(this.tasks);
			this.tasks.Remove(completedTask);
			return await completedTask;
		}

		[AssertionMethod]
		[ExcludeFromCodeCoverage] // Should never trigger an exception
		private void AssertTasksAvailable()
		{
			if (this.tasks.Count == 0)
			{
				throw new InvalidOperationException("No more tasks to await!");
			}
		}
	}
}
