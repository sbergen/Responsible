using System.Threading;
using System.Threading.Tasks;

namespace Responsible.Utilities
{
	/// <summary>
	/// Implementation of <see cref="IMultipleTaskSource{T}"/> which repeats one task indefinitely.
	/// </summary>
	internal class RepeatedTaskSource<T> : IMultipleTaskSource<T>
	{
		private readonly DeferredTask<T> taskFactory;

		public RepeatedTaskSource(DeferredTask<T> taskFactory)
		{
			this.taskFactory = taskFactory;
		}

		IMultipleTaskAwaiter<T> IMultipleTaskSource<T>.Start(CancellationToken cancellationToken)
		{
			return new Awaiter(this.taskFactory, cancellationToken);
		}

		private sealed class Awaiter : IMultipleTaskAwaiter<T>
		{
			private readonly DeferredTask<T> taskFactory;
			private readonly CancellationToken cancellationToken;

			public Awaiter(DeferredTask<T> taskFactory, CancellationToken cancellationToken)
			{
				this.taskFactory = taskFactory;
				this.cancellationToken = cancellationToken;
			}

			public bool HasNext => true;
			public Task<T> AwaitNext() => this.taskFactory(this.cancellationToken);
		}
	}
}
