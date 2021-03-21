using System.Threading.Tasks;

namespace Responsible.NoRx.Utilities
{
	// Replacement for IAsyncEnumerator, as Unity does not have that
	public interface IMultipleTaskAwaiter<T>
	{
		bool HasNext { get; }
		Task<T> AwaitNext();
	}
}
