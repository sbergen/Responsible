using System.Threading.Tasks;

namespace Responsible.Utilities
{
	/// <summary>
	/// Represents multiple started tasks, which can be individually awaited for in completion order.
	/// </summary>
	/// <remarks>
	/// Intended only for internal use, but is public, because it's visible from other interfaces.
	/// </remarks>
	/// <typeparam name="T">Type of the tasks that this instance yields.</typeparam>
	internal interface IMultipleTaskAwaiter<T>
	{
		/// <summary>
		/// Determines if there are more tasks to await for.
		/// </summary>
		/// <value>True, if there are more tasks to wait for, otherwise false.</value>
		bool HasNext { get; }

		/// <summary>
		/// Awaits for the next task to complete
		/// </summary>
		/// <returns>A task that represents the result of the next completed task.</returns>
		Task<T> AwaitNext();
	}
}
