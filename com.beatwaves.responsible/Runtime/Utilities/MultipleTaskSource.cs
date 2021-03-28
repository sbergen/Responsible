using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Responsible.Utilities
{
	internal static class MultipleTaskSource
	{
		public static IMultipleTaskSource<T> Make<T>(IEnumerable<DeferredTask<T>> deferredTasks)
			=> new MultipleTaskSource<T>(deferredTasks);
	}

	internal class MultipleTaskSource<T> : IMultipleTaskSource<T>
	{
		private readonly List<DeferredTask<T>> taskFactories;

		public MultipleTaskSource(IEnumerable<DeferredTask<T>> deferredTasks)
		{
			this.taskFactories = deferredTasks.ToList();
		}

		public IMultipleTaskAwaiter<T> Start(CancellationToken cancellationToken)
			=> MultipleTaskAwaiter.Make(this.taskFactories
				.Select(factory => factory(cancellationToken)));
	}
}
