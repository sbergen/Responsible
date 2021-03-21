using System.Threading;

namespace Responsible.Utilities
{
	// ≈ Replacement for IAsyncEnumerable, as Unity does not have that
	public interface IMultipleTaskSource<T>
	{
		IMultipleTaskAwaiter<T> Start(CancellationToken cancellationToken);
	}
}
