using JetBrains.Annotations;
using Responsible.State;

namespace Responsible
{
	public interface ITestOperation<out T>
	{
		/// <summary>
		/// Creates the state object for a single run of this test operation.
		///
		/// You should normally prefer the following extension methods over
		/// creating a state instance manually:
		/// * <seealso cref="TestInstruction.ToYieldInstruction{T}"/>
		/// * <seealso cref="TestInstruction.ToObservable{T}"/>
		/// </summary>
		/// <remarks>
		/// Converting an instance of <see cref="ITestOperationState{T}"/>
		/// to a string will dump full information of its current execution status.
		/// </remarks>
		[Pure]
		[NotNull]
		ITestOperationState<T> CreateState();
	}
}
