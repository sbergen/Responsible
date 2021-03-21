using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Responsible.NoRx.Utilities
{
	public static class MultipleTaskSource
	{
		public static IMultipleTaskSource<T> Make<T>(IEnumerable<Func<CancellationToken, Task<T>>> taskFactories)
			=> new MultipleTaskSource<T>(taskFactories);
	}

	public class MultipleTaskSource<T> : IMultipleTaskSource<T>
	{
		private readonly List<Func<CancellationToken, Task<T>>> taskFactories;

		public MultipleTaskSource(IEnumerable<Func<CancellationToken, Task<T>>> taskFactories)
		{
			this.taskFactories = taskFactories.ToList();
		}

		public IMultipleTaskAwaiter<T> Start(CancellationToken cancellationToken)
			=> MultipleTaskAwaiter.Make(this.taskFactories
				.Select(factory => factory(cancellationToken)));
	}
}
