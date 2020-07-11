using JetBrains.Annotations;
using Responsible.State;

namespace Responsible
{
	/// <summary>
	/// Represents a condition to wait on, which returns a result when fulfilled.
	/// </summary>
	public interface ITestWaitCondition<out T> : ITestOperationContext
	{
		/// <summary>
		/// Waits for the condition to be fulfilled, an returns the result of the wait.
		/// </summary>
		[Pure]
		[NotNull]
		IOperationState<T> CreateState();
	}
}