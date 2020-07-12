using JetBrains.Annotations;
using Responsible.State;

namespace Responsible
{
	public interface ITestOperation<out T>
	{
		/// <summary>
		/// Creates the state object for a single run of this test operation.
		/// </summary>
		/// <remarks>
		/// Converting a state to a string will dump full information of its current execution status.
		/// </remarks>
		[Pure]
		[NotNull]
		ITestOperationState<T> CreateState();
	}
}