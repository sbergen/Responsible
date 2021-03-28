using System.Threading;

namespace Responsible.Utilities
{
	/// <summary>
	/// Represents a source for starting and awaiting for multiple tasks at once.
	/// </summary>
	/// <remarks>
	/// Intended only for internal use, but is public, because it's visible from other interfaces.
	/// </remarks>
	/// <typeparam name="T">Type of the tasks that the returned awaiter yields.</typeparam>
	public interface IMultipleTaskSource<T>
	{
		/// <summary>
		/// Creates a <see cref="IMultipleTaskAwaiter{T}"/> which can be used to await for
		/// the deferred tasks this source represents, in the order they complete in.
		/// </summary>
		/// <param name="cancellationToken">
		/// Cancellation token passed to all the tasks started by this source.
		/// </param>
		/// <returns>A multiple task awaiter for the deferred tasks in this source.</returns>
		IMultipleTaskAwaiter<T> Start(CancellationToken cancellationToken);
	}
}
