using System.Threading.Tasks;

namespace Responsible.Utilities
{
	/// <summary>
	/// Approximate replacement for IAsyncEnumerator, as we support dotnet core 2.0
	/// </summary>
	/// <remarks>
	/// Intended only for internal use, but is public, because it's visible from other interfaces.
	/// </remarks>
	/// <typeparam name="T">Type of the tasks that this yields.</typeparam>
	public interface IMultipleTaskAwaiter<T>
	{
		bool HasNext { get; }
		Task<T> AwaitNext();
	}
}
