using System.Threading;

namespace Responsible.Utilities
{
	/// <summary>
	/// Approximate replacement for IAsyncEnumerable, as we support dotnet core 2.0
	/// </summary>
	/// <remarks>
	/// Intended only for internal use, but is public, because it's visible from other interfaces.
	/// </remarks>
	/// <typeparam name="T">Type of the tasks that the returned awaiter yields.</typeparam>
	public interface IMultipleTaskSource<T>
	{
		IMultipleTaskAwaiter<T> Start(CancellationToken cancellationToken);
	}
}
